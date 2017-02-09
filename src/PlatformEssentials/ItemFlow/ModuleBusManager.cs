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
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Credentials;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;
using VP.FF.PT.Common.Utils.Security;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public class ModuleBusManager : IModuleBusManager, IGraphProvider, IPartImportsSatisfiedNotification
    {
        private IDisposable _platformItemEvent;

        private bool _hasFinishedInitializationOfAllModules;

        [Import]
        protected IPlatformModuleInitializer ModuleInitializer;

        [Import]
        protected IPlatformModuleActivator ModuleActivator;

        [Import]
        protected IPlatformModuleCreator ModuleCreator;

        [Import]
        protected IProvideConfiguration _provideConfiguration;

        [Import]
        protected ILogger Logger;

        [Import]
        protected IEventAggregator EventAggregator;

        [Import]
        protected IPlatformModuleRepository ModuleRepository;

        protected int StreamType = 1;

        /// <summary>
        /// Gets a value indicating if this instance has finished the initialization
        /// of all dependent modules.
        /// </summary>
        /// <value>
        ///   true if the initialization routine was called at least once, false if not.
        /// </value>
        public bool HasFinishedInitializationOfAllModules
        {
            get { return _hasFinishedInitializationOfAllModules; }
        }

        protected IRouteCalculator<IPlatformModule> RouteCalculator { get; set; }

        /// <summary>
        /// Notifies the subscribers that the module initialization has started.
        /// </summary>
        public event Action<string> ModuleInitializationStarted
        {
            add { ModuleInitializer.ModuleInitializationStarted += value; }
            remove { ModuleInitializer.ModuleInitializationStarted -= value; }
        }

        public virtual void Construct()
        {
            ModuleCreator.ConstructModules(StreamType);
            AddCredentials();
        }

        //Add the Credentials from the App.config to the Windows Credential Manager
        private void AddCredentials()
        {
            var credentialToolBox = new WindowsCredentialToolBox();

            try
            {
                var credentialConfig = _provideConfiguration.GetConfiguration<CredentialConfigSection>("credentialSection");

                if (credentialConfig != null && credentialConfig.Credentials.Count > 0)
                {
                    foreach (CredentialConfig credential in credentialConfig.Credentials)
                    {
                        credentialToolBox.Add(credential.Target, credential.User, credential.Password.DecodePassword(),
                            credential.Type, credential.Persist);
                    }
                }
                else
                {
                    Logger.Info("No Credentials available");
                }
            }
            catch (Exception e)
            {
                Logger.Error("Something wrong with the credentialSection in the App.config", e);
            }

        }

        public virtual void Initialize()
        {
            ModuleInitializer.Initialize(StreamType);

            RouteCalculator = new ForcingRouteCalculator(ModuleRepository.Modules, ModuleCreator.Graph, Logger);

            foreach (var platformModule in ModuleRepository.Modules)
            {
                platformModule.CurrentItemCountChangedEvent += OnModuleItemCountChangedEvent;
                platformModule.ModuleStateChangedEvent += OnModuleStateChangedEvent;
                platformModule.ModulePortFullChangedEvent += OnModulePortFullChangedEvent;
            }

            _hasFinishedInitializationOfAllModules = true;
        }

        public virtual void Activate()
        {
            ModuleActivator.ActivateModules(StreamType);
        }

        public ModuleGraph InternalGraph
        {
            get { return ((RouteCalculator)RouteCalculator).Graph; }
        }

        public ModuleGraphDTO GraphDto
        {
            get { return RouteCalculator.GraphToDto(); }
        }

        public void SetIgnoreDownstreamModule(string moduleName, bool ignore)
        {
            Logger.InfoFormat("Changed ignore-downstream flag for module {0} to {1}", moduleName, ignore);
            RouteForcing.SetIgnoreDownstreamModule(moduleName, ignore);

            var module = ModuleRepository.GetModule(moduleName);
            if (ignore)
            {
                module.AlarmManager.AddAlarm(IgnoreDownstreamAlarms.GetAlarm(module));
            }
            else
            {
                module.AlarmManager.RemoveAlarm(IgnoreDownstreamAlarms.GetAlarm(module));
            }
        }

        public bool GetIgnoreDownstreamModule(string moduleName)
        {
            return RouteForcing.GetIgnoreDownstreamModule(moduleName);
        }

        protected IRouteForcing<IPlatformModule> RouteForcing { get { return (IRouteForcing<IPlatformModule>)RouteCalculator; } }

        private void OnModulePortFullChangedEvent(IPlatformModule sender, int inputPortIndex, bool isFull)
        {
            RouteForcing.RecalculateRoute();
        }

        private void OnModuleItemCountChangedEvent(object module, ItemCountChangedEventArgs countEventArgs)
        {
            RouteForcing.RecalculateRoute();
        }

        private void OnModuleStateChangedEvent(IPlatformModule module, PlatformModuleState newState)
        {
            RouteForcing.RecalculateRoute();
        }

        public void ForcePath(IPlatformModule sourceModule, IPlatformModule targetModule, int sourcePortIndex, int targetPortIndex)
        {
            RouteForcing.ForcePath(sourceModule, targetModule, sourcePortIndex, targetPortIndex);
        }

        public void ReleaseForcePath(IPlatformModule sourceModule, IPlatformModule targetModule, int sourcePortIndex, int targetPortIndex)
        {
            RouteForcing.ReleaseForcePath(sourceModule, targetModule, sourcePortIndex, targetPortIndex);
        }

        public bool IsRoutePossible(IPlatformModule sourceModule, IPlatformModule targetModule)
        {
            return RouteCalculator.IsRoutePossible(sourceModule, targetModule);
        }

        public virtual AcceptResult ReadyForProduction()
        {
            foreach (var module in ModuleRepository.Modules.OfType<IReadyForProduction>())
            {
                var result = module.IsReadyForProduction;
                if (!result.IsAccepted)
                {
                    return result;
                }
            }

            return AcceptResult.Accepted();
        }

        public virtual Task<AcceptResult> AddItemAsync(PlatformItem item)
        {
            return Task.FromResult(AcceptResult.Accepted());
        }

        private void OnPlatformItemEvent(PlatformItemEvent itemEvent)
        {
            if (!IsInitialized)
                return;

            var module = itemEvent.AssosiatedModule;
            var itemId = itemEvent.ItemId;

            if (!RouteCalculator.GraphContainsModule(module))
                return;

            if (itemEvent.EventType == PlatformItemEventType.NewItemCreated)
            {
                var item = itemEvent.NewItem;

                if (item == null)
                    throw new InvalidOperationException(module.Name + " created a new PlatformItem but was not set as a reference to event args!");

                var previousModule = (from m in ModuleRepository.Modules
                                      where m.ContainsItem(itemEvent.ItemId) &&
                                            m != module
                                      select m).FirstOrDefault();

                if (previousModule != null)
                {
                    Logger.Warn("Duplicate item-id detected.");
                    item.AddLog("Duplicate item-id detected.");
                    previousModule.RemoveItem(itemId);
                }

                module.AddItem(item);
                UpdateRouteIndex(item, module);
                RecalculateRoute(item, module);
            }
            else if (itemEvent.EventType == PlatformItemEventType.ItemDetected)
            {
                var previousModule = (from m in ModuleRepository.Modules
                                      where m.ContainsItem(itemId) &&
                                          m != module
                                      select m).FirstOrDefault();

                if (previousModule != null)
                {
                    var originalItem = previousModule.GetItem(itemId);
                    previousModule.MoveItem(itemId, module);
                    UpdateRouteIndex(originalItem, module);
                }
                else
                {
                    if (!module.ContainsItem(itemId))
                    {
                        var item = module.CreateNewPlatformItem();
                        item.ItemId = itemId;

                        module.AddItem(item);
                    }
                }

                RecalculateRoute(module.GetItem(itemId), module);
            }
            else if (itemEvent.EventType == PlatformItemEventType.ItemLeft)
            {
                var targetModule = RouteCalculator.GetTargetModule(itemEvent.ReleasePort, module);

                // sink, finished
                if (targetModule == null)
                {
                    module.RemoveItem(itemId);
                    Logger.Info("Item " + (ulong)itemId + " lost in " + module.Name + ". Is in nirvana now.");
                }
                // move to next
                else
                {
                    var item = module.GetItem(itemId);
                    module.MoveItem(itemId, targetModule);

                    UpdateRouteIndex(item, targetModule);
                    RecalculateRoute(item, targetModule);
                }
            }
        }

        private void RecalculateRoute(PlatformItem item, IPlatformModule module)
        {
            if (!RouteCalculator.CalculateSingleItemRouting(item, module))
            {
                module.RemoveItemRouting(item);
            }
        }

        private void UpdateRouteIndex(PlatformItem item, IPlatformModule module)
        {
            if (item == null || item.Route == null)
                return;

            int index = item.Route.CurrentIndex;

            if (index < (item.Route.RouteItems.Count - 1))
            {
                int nextModuleTypeId = item.Route.GetOrderedList()[index + 1].ModuleType;
                if (module.ModuleTypeId == nextModuleTypeId)
                {
                    item.Route.CurrentIndex++;

                    index = item.Route.CurrentIndex;
                    if (index + 1 < item.Route.RouteItems.Count)
                    {
                        nextModuleTypeId = item.Route.GetOrderedList()[index + 1].ModuleType;
                        Logger.DebugFormat("Updated Route. Next target is {0}", nextModuleTypeId);
                    }
                }
            }

            if (item.Route.CurrentIndex >= item.Route.RouteItems.Count - 1)
            {
                Logger.InfoFormat("Route completed for item {0}", item.ItemId);
            }
        }

        public void OnImportsSatisfied()
        {
            Logger.Init(GetType());

            if (_platformItemEvent != null)
                _platformItemEvent.Dispose();
            _platformItemEvent = EventAggregator.GetEvent<PlatformItemEvent>().Subscribe(OnPlatformItemEvent);
        }

        private bool IsInitialized
        {
            get
            {
                return ModuleInitializer.AllModulesInitialized;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0}#{1}", GetType().Name, GetHashCode());
        }

        private static class IgnoreDownstreamAlarms
        {
            private static readonly ConcurrentDictionary<IPlatformModule, Alarm> Alarms = new ConcurrentDictionary<IPlatformModule, Alarm>();

            public static Alarm GetAlarm(IPlatformModule module)
            {
                var message = module.Name + " ignores downstream module";
                var alarm = Alarms.GetOrAdd(module, CreateAlarm(module, message));
                return alarm;
            }

            private static Alarm CreateAlarm(IPlatformModule module, string message)
            {
                return new Alarm
                {
                    Message = message,
                    Type = AlarmType.Warning,
                    Source = module.Name,
                    SourceType = AlarmSourceType.LineControl,
                    Timestamp = DateTime.Now
                };
            }
        }
    }
}
