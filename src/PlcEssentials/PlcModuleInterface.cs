/* Copyright 2017 Cimpress

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. */


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Encapsulated interaction with PLC modules based on the PLC BaseConcept and module naming conventions.
    /// Provides state and some additional common module informations. It also handles the KeepAlive feedback.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlcModuleInterface : ModuleEquipment, IHavePlcInformation, IDisposable
    {
        public event Action<short> StateChanged;
        public event Action<string> StateNameChanged;
        public event Action<string> SubStateNameChanged;
        public event Action PlcHeartbeatFast;
        public event EventHandler<PlcModuleInterfaceEventArgs> InterfaceChanged;
        public event Action<int> IsPortFullChanged;

        public int FastPollingRate { get; set; }

        public int SlowPollingRate { get; set; }

        protected const int MaxPortListCount = 11;
        public readonly IList<bool> IsPortFull = new List<bool>(MaxPortListCount);

        private string _name;
        private readonly ILogger _logger;
        private readonly List<Tuple<int, Action<IController>>> _subscribers;

        private const double KeepAliveTimeout = 5000;
        private Tag _keepAliveTag;
        private DateTime _lastKeepAliveToggleTime;
        private readonly object _lockSubscribers = new object();

        private Tag _stateNameTag;
        private Tag _subStateNameTag;
        
        private string _stateName;
        private string _subStateName;
        private string _oldStateName;
        private string _adsPath;
        private int _adsPort;
        private string _pathRootController;

        private IGenericDataChannelListener<PlcSaberLoggerDataStruct> _plcSaberLoggerDchRecv;
        private bool _enableSubStateLogging;
        private readonly object _isPortFullLock = new object();

        protected Tag CommandChannelTag { get; private set; }

        [Import]
        public ITagListener FastTagListener { get; internal set; }

        [Import]
        public ITagListener SlowTagListener { get; internal set; }

        [Import]
        public ITagController TagController { get; internal set; }

        /// <summary>
        /// Gets the <see cref="IControllerTreeImporter"/> for the plc associated with this instance.
        /// </summary>
        [Import]
        public IControllerTreeImporter Importer { get; internal set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _logger.Init(GetType() + "-" + value);
            }
        }

        public IControllerTree ControllerTree
        {
            get
            {
                if (Importer == null)
                    return null;
                return Importer.ControllerTree;
            }
        }

        public string Scope
        {
            get { return "MAIN"; }
        }

        [ImportingConstructor]
        public PlcModuleInterface(ILogger logger)
        {
            _logger = logger;
            _subscribers = new List<Tuple<int, Action<IController>>>(1);
            InnerControllerTreeUpdated += c => { };
            StateChanged += c => { };
            StateNameChanged += c => { };
            SubStateNameChanged += c => { };
            PlcHeartbeatFast += () => { };
            InterfaceChanged += (sender, args) => { };
            IsPortFullChanged += i => { };

            FastPollingRate = 50;
            SlowPollingRate = 200;

            _stateName = "NONE";
            _oldStateName = "NONE";

            EquipmentState = "NONE";
            EquipmentName = GetType().Name;

            for (int i = 0; i < MaxPortListCount; i++)
                IsPortFull.Add(false);
        }

        private event Action<IControllerTree> InnerControllerTreeUpdated;

        public void Initialize(CancellationToken token, string name, string adsPath, int adsPort, string rootController, bool enableStateLogging = false)
        {
            _adsPath = adsPath;
            _adsPort = adsPort;
            _pathRootController = rootController;
            _enableSubStateLogging = enableStateLogging;

            Name = name;
            FastTagListener.Name = name;
            FastTagListener.AddressAndPath = adsPath + ":" + adsPort;
            SlowTagListener.Name = name;
            SlowTagListener.AddressAndPath = adsPath + ":" + adsPort;
            TagController.StartConnection(adsPath, adsPort);
            if (!TagController.IsConnected)
                throw new Exception("Can't initialize " + name + " because connection to PLC failed! (Address: " + adsPath + ":" + adsPort);

            FastTagListener.CommunicationProblemOccured += TagListenerCommunicationProblemOccurred;
            SlowTagListener.CommunicationProblemOccured += TagListenerCommunicationProblemOccurred;

            try
            {
                _plcSaberLoggerDchRecv = new GenericDataChannelListener<PlcSaberLoggerDataStruct>(FastTagListener, TagController);
                _plcSaberLoggerDchRecv.SetChannel("_pSaberLogger^.fbDataChannel", _pathRootController, "T_Logger_DCH");
                _plcSaberLoggerDchRecv.DataReceived += PlcSaberLoggerDataReceived;
                _plcSaberLoggerDchRecv.CommunicationProblemOccured += PlcSaberLoggerCommunicationProblemOccured;
            }
            catch (Exception e)
            {
                _logger.Error("Initialization of Logger Datachannel failed:", e);
            }

            InitControllerTags();
            ListenToPlcModuleActualValues();
            ListenToPlcModuleState(enableStateLogging);

            Importer.Initialize(SlowTagListener, _adsPath, _adsPort, _pathRootController);

            _lastKeepAliveToggleTime = DateTime.Now;

            FastTagListener.RefreshAll();
            SlowTagListener.RefreshAll();
        }

        public void Activate()
        {
            SlowTagListener.StartListening(_adsPath, _adsPort);
            FastTagListener.StartListening(_adsPath, _adsPort);
        }

        public void Dispose()
        {
            
        }

        public void Start()
        {
            TagController.WriteTag(CommandChannelTag, (short)StandardCommands.Run);
        }

        public void Standby()
        {
            TagController.WriteTag(CommandChannelTag, (short)StandardCommands.Standby);
        }

        public void Stop()
        {
            TagController.WriteTag(CommandChannelTag, (short)StandardCommands.Off);
        }

        /// <summary>
        /// The <see cref="ControllerTreeUpdated"/> event is raised whenever the overall information of the controller
        /// tree has changed.
        /// </summary>
        public event Action<IControllerTree> ControllerTreeUpdated
        {
            add
            {
                IControllerTree controllerTree = ControllerTree;
                if (controllerTree == null)
                    return;
                InnerControllerTreeUpdated += value;
                controllerTree.OverallInformationChanged += NotifyControllerTreeSubscribers;
                foreach (Tag overallInformationTag in controllerTree.GetOverallInformationTags())
                    SlowTagListener.AddTag(overallInformationTag);
            }
            remove
            {
                InnerControllerTreeUpdated -= value;
                IControllerTree controllerTree = ControllerTree;
                if (controllerTree == null)
                    return;
                controllerTree.OverallInformationChanged -= NotifyControllerTreeSubscribers;
                foreach (Tag overallInformationTag in controllerTree.GetOverallInformationTags())
                    SlowTagListener.RemoveTag(overallInformationTag);
            }
        }

        public void UpdateControllerTree()
        {
            Importer.UpdateImportedControllers();
        }

        /// <summary>
        /// Subscribes the specified <paramref name="handler"/> on the controller with the specified <paramref name="controllerId"/>.
        /// The handler gets executed whenever the controller changed a value.
        /// </summary>
        /// <param name="controllerId">The id of the controller to observe.</param>
        /// <param name="handler">The handler to get executed on a controller change.</param>
        public void SubscribeForControllerChanges(int controllerId, Action<IController> handler)
        {
            IController controller = ControllerTree.GetController(controllerId);
            lock (_lockSubscribers)
            {
                _subscribers.Add(new Tuple<int, Action<IController>>(controllerId, handler));
            }
            foreach (Tag controllerTag in controller.GetAllAssociatedTags())
            {
                SlowTagListener.AddTag(controllerTag);
                controllerTag.ValueChanged += NotifySubscribers;
            }
            SlowTagListener.RefreshAll();
        }

        /// <summary>
        /// Unsubscribes the specified <paramref name="handler"/> from the controller with the specified <paramref name="controllerId"/>.
        /// The handler won't get executed on a change of the controller.
        /// </summary>
        /// <param name="controllerId">The id of the controller to not anymore observe.</param>
        /// <param name="handler">The handler to unsubscribe.</param>
        public void UnsubsribeFromControllerChanges(int controllerId, Action<IController> handler)
        {
            IController controller = ControllerTree.GetController(controllerId);
            foreach (Tag controllerTag in controller.GetAllAssociatedTags())
            {
                controllerTag.ValueChanged -= NotifySubscribers;
                SlowTagListener.RemoveTag(controllerTag);
            }

            lock (_lockSubscribers)
            {
                _subscribers.RemoveAll(t => Equals(t.Item2, handler));
            }
        }

        private void NotifySubscribers(Tag tag, TagValueChangedEventArgs args)
        {
            List<Tuple<int, Action<IController>>> subscribers;

            lock (_lockSubscribers)
            {
                subscribers = new List<Tuple<int, Action<IController>>>(_subscribers);
            }

            foreach (Tuple<int, Action<IController>> subscriber in subscribers)
            {
                IController controller = ControllerTree.TryGetController(subscriber.Item1);
                if (controller != null)
                    subscriber.Item2(controller);
            }
        }

        private void NotifyControllerTreeSubscribers()
        {
            IControllerTree controllerTree = ControllerTree;
            if (controllerTree == null)
                return;
            InnerControllerTreeUpdated(controllerTree);
        }

        private void TagListenerCommunicationProblemOccurred(object sender, Exception e)
        {
            _logger.Error("PLC Communication (TagListener) Problem", e);
            StateChanged((short) StandardStates.Unknown);
        }

        private void InitControllerTags()
        {
            CommandChannelTag = new Tag(
                NamingConventions.CommonInterfaceAutoCmdChannel,
                _pathRootController,
                "INT");

            _keepAliveTag = new Tag(
                            NamingConventions.ModuleInterfaceActualValues + ".bolKeepAliveHost",
                            _pathRootController,
                            "BOOL");
        }

        private void HandlePlcKeepAlive(bool keepAliveHost, bool keepAlivePlc)
        {
            if (keepAliveHost == keepAlivePlc)
            {
                Task.Run(() =>
                {
                    try
                    {
                        TagController.WriteTag(_keepAliveTag, !keepAliveHost);
                    }
                    catch (PlcCommunicationException)
                    {
                        // ignore, timeout will occur later if connection lost
                    }

                    _lastKeepAliveToggleTime = DateTime.Now;
                });
            }

            if (DateTime.Now - _lastKeepAliveToggleTime > TimeSpan.FromMilliseconds(KeepAliveTimeout))
            {
                //_logger.Error("No KeepAlive to PLC " + FastTagListener.AddressAndPath + " !");

                try
                {
                    //Stop();
                    //State = PlatformModuleState.Error;
                    //_logger.Info("Module stopped because no KeepAlive to PLC");
                }
                catch (Exception)
                {
                    _logger.Error("Can't stop module after no KeepAlive to PLC");
                }

                _lastKeepAliveToggleTime = DateTime.Now;
            }
        }

        private void ListenToPlcModuleActualValues()
        {
            var tag = new Tag(
                            NamingConventions.ModuleInterfaceActualValues,
                            _pathRootController,
                            NamingConventions.ModuleInterfaceActualValuesType,
                            _adsPort);
            tag.ValueChanged += ModuleInterfaceTagChanged;

            FastTagListener.AddUdtHandler<ModuleActualValues>(NamingConventions.ModuleInterfaceActualValuesType);
            FastTagListener.AddTag(tag);
        }

        private void ListenToPlcModuleState(bool enableStateLogging)
        {
            var stateTag = new Tag(NamingConventions.CommonInterfaceState, _pathRootController, "INT");
            _stateNameTag = new Tag(NamingConventions.CommonInterfaceStateName, _pathRootController, "STRING");
            _subStateNameTag = new Tag(NamingConventions.CommonInterfaceSubStateName, _pathRootController, "STRING");

            stateTag.ValueChanged += StateTagValueChanged;
            _stateNameTag.ValueChanged += StateNameTagValueChanged;
            _subStateNameTag.ValueChanged += SubStateNameTagValueChanged;

            FastTagListener.AddTag(stateTag);
            FastTagListener.AddTag(_subStateNameTag);
            FastTagListener.AddTag(_stateNameTag);

            _stateNameTag.IsActive = enableStateLogging;
        }

        private void StateTagValueChanged(Tag sender, TagValueChangedEventArgs eventArgs)
        {
            if (eventArgs.Value == null)
            {
                StateChanged((short) StandardStates.Off);
                SubStateNameChanged("<unknown>");
                return;
            }

            StateChanged((short) eventArgs.Value);
        }

        private void StateNameTagValueChanged(Tag sender, TagValueChangedEventArgs eventArgs)
        {
            try
            {
                _oldStateName = _stateName;
                _stateName = (string)_stateNameTag.Value;
                EquipmentState = _stateName;
                _logger.InfoFormat("MainStateChanged: '{0}' to '{1}'", _oldStateName, _stateName);
            }
            catch (PlcCommunicationException exception)
            {
                _logger.Error("Can't read state name from PLC", exception);
            }

            StateNameChanged(_stateName);
        }

        private void SubStateNameTagValueChanged(Tag sender, TagValueChangedEventArgs eventArgs)
        {
            try
            {
                var oldSubStateName = _subStateName;
                _subStateName = (string)_subStateNameTag.Value;

                if (_enableSubStateLogging)
                {
                    _logger.InfoFormat("SubStateChanged: '{0}' to '{1}'", oldSubStateName, _subStateName);
                }
            }
            catch (PlcCommunicationException exception)
            {
                _subStateName = "<unknown comm err>";
                _logger.Error("Can't read sub state name from PLC", exception);
            }

            SubStateNameChanged(_subStateName);
        }

        private void ModuleInterfaceTagChanged(Tag sender, TagValueChangedEventArgs e)
        {
            var plcActualValues = (ModuleActualValues)e.Value;
            if (plcActualValues == null)
                return;

            HandlePlcKeepAlive(plcActualValues.KeepAliveHost, plcActualValues.KeepAlivePlc);

            InterfaceChanged(this, new PlcModuleInterfaceEventArgs(
                plcActualValues.MaxCapacity,
                plcActualValues.NumberOfItems));

            lock (_isPortFullLock)
            {
                for (int i = 0; i < plcActualValues.IsFull.Length; i++)
                {
                    if (IsPortFull[i] != plcActualValues.IsFull[i])
                    {
                        IsPortFull[i] = plcActualValues.IsFull[i];
                        IsPortFullChanged(i);
                    }
                }
            }

            PlcHeartbeatFast();
        }

        private void PlcSaberLoggerDataReceived(object sender, PlcSaberLoggerDataStruct data)
        {
            if (data != null)
            {
                try
                {
                    string message = string.Format("PlcLogger: Controller={0}, TimeStamp={1}, Message={2}", data.controller, data.timeStamp, data.message);

                    switch (data.level)
                    {
                        case EPlcSaberLoggerLevel.Info:
                            _logger.InfoFormat(message);
                            break;
                        case EPlcSaberLoggerLevel.Warning:
                            _logger.WarnFormat(message);
                            break;
                        case EPlcSaberLoggerLevel.Error:
                            _logger.ErrorFormat(message);
                            break;
                        default:
                            _logger.ErrorFormat("Unknown Level for message: {0}", message);
                            break;
                    }
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("PlcSaberLoggerDataReceived Exception {0}", e);
                }
            }
            else
            {
                _logger.Error("PlcSaberLoggerDataReceived has invalid data");
            }
        }

        private void PlcSaberLoggerCommunicationProblemOccured(object sender, Exception e)
        {
            _logger.Error("Communication problem with the Plc Saber Logger occured:", e);
        }
    }
}
