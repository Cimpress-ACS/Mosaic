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
using System.Linq;
using System.Threading;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;
using VP.FF.PT.Common.PlcEssentials.ControllerImporting;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    [Export(typeof(IControllerTreeImporter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BeckhoffOnlineControllerTreeImporter : IControllerTreeImporter
    {
        private const int NoControllerId = 0;
        private const int MaxChildImportDepth = 20;

        private readonly ITwinCatClient _twinCatClient;
        private readonly ITagController _tagController;
        private readonly IAlarmsImporter _alarmsImporter;
        private readonly IFindControllerTags _findControllerTags;
        private readonly ICreateController _createController;
        private readonly ILogger _logger;

        private ITagListener _tagListener;
        private string _adsAddress;
        private int _adsPort;
        private string _rootController;
        private ControllerTree _controllerTree;
        
        public BeckhoffOnlineControllerTreeImporter(ITagImporter tagImporter)
        {
            _twinCatClient = new TwinCatClientWrapper();
            _tagController = new BeckhoffTagController();
            _tagListener = new BeckhoffPollingTagListener();
            _alarmsImporter = new BeckhoffOnlineAlarmsImporter();
            _findControllerTags = new ControllerTagFinder(tagImporter, new Log4NetLogger());
            _createController = new ControllerFactory();
            _logger = new Log4NetLogger();
            _controllerTree = new ControllerTree();
        }

        [ImportingConstructor]
        public BeckhoffOnlineControllerTreeImporter(
            ITwinCatClient twinCatClient, 
            ITagController tagController, 
            ITagListener tagListener, 
            IFindControllerTags findControllerTags,
            ICreateController createController,
            IAlarmsImporter alarmsImporter, 
            ILogger logger)
        {            
            _twinCatClient = twinCatClient;
            _tagController = tagController;
            _tagListener = tagListener;
            _alarmsImporter = alarmsImporter;
            _findControllerTags = findControllerTags;
            _createController = createController;
            _logger = logger;
            _controllerTree = new ControllerTree();
        }

        /// <summary>
        /// Gets the alarms importer used by this instance.
        /// </summary>
        public IAlarmsImporter AlarmsImporter
        {
            get { return _alarmsImporter; }
        }


        /// <summary>
        /// Initializes this instance with all necessary information needed to import controller trees.
        /// </summary>
        /// <param name="tagListener">The tag listener used to register the tags of the imported controller on.</param>
        /// <param name="path">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress.</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        /// <param name="rootController">The plcAddress of the root controller</param>
        public virtual void Initialize(
            ITagListener tagListener,
            string path,
            int port = 0, 
            string rootController = "")
        {
            if (_controllerTree.IsInitialized)
                return;
            _tagListener = tagListener;
            _adsAddress = path;
            _adsPort = port;
            _rootController = rootController;
            _logger.Init(typeof(BeckhoffOnlineControllerTreeImporter));
            _twinCatClient.Initialize(path, port);
            _tagController.StartConnection(path, port);
            _alarmsImporter.Initialize(tagListener);
            _findControllerTags.Initialize(_tagListener, _adsAddress, _adsPort);
        }

        private Controller RootController
        {
            get
            {
                return _controllerTree.RootController;
            }
        }

        /// <summary>
        /// Gets the <see cref="ControllerTree"/>
        /// </summary>
        public IControllerTree ControllerTree 
        { 
            get 
            {
                return _controllerTree;   
            }
        }

        /// <summary>
        /// Imports all controllers.
        /// </summary>
        public void ImportAllControllers()
        {
            ImportAllControllers(null);
        }

        /// <summary>
        /// Imports all controllers from the specified path and port 
        /// and searchs for user defined interfaces for each controller.
        /// </summary>
        public void ImportAllControllers(IList<string> userDefinedInterfaces)
        {
            _logger.Debug(string.Format("Import controller tree skeleton on '{0}:{1}'", _adsAddress, _adsPort));
            _alarmsImporter.ImportAlarms();
            IReadOnlyCollection<IControllerTag> controllerTags = _findControllerTags.FindControllerTags();
            if (controllerTags.IsNullOrEmpty())
                return;
            IControllerTag anyControllerTag = controllerTags.First();
            _tagListener.AddUdtHandler<CtrlCommonInterface>(anyControllerTag.GetCommonInterfaceDataType());
            foreach (IControllerTag controllerTag in controllerTags)
            {
                Tag commonInterfaceTag = controllerTag.GetCommonInterfaceTag();
                if (commonInterfaceTag != null)
                    _tagListener.ReadTagSynchronously(commonInterfaceTag);
            }
            Controller controller = ImportControllerWithChildren(_rootController, controllerTags, userDefinedInterfaces, MaxChildImportDepth);
            _controllerTree.Initialize(controller);
            _alarmsImporter.AlarmsChanged += alarms => _controllerTree.UpdateAlarms(alarms);
            _controllerTree.UpdateAlarms(_alarmsImporter.GetAllImportedAlarms());
            _logger.Debug(string.Format("Finished import of controller skeleton tree on '{0}:{1}'", _adsAddress, _adsPort));
        }

        public void UpdateImportedControllers()
        {
            if (RootController == null)
                return;
            RootController.VisitAllNodes(c => c.ChildsCollection, ReadControllerTags);
        }

        private void ReadControllerTags(Controller controller)
        {
            foreach (Tag tag in controller.GetAllAssociatedTags())
                _tagListener.ReadTagSynchronously(tag);
            foreach (Tag tag in controller.Commands.SelectMany(c => c.CmdValues))
                _tagListener.ReadTagSynchronously(tag);
        }

        /// <summary>
        /// Imports the controller trees recursively. Returns the root controller.
        /// The first call will build up the whole hierarchical controller tree. A second
        /// call will use the cached tree and just update the controller fields and tags.
        /// </summary>
        /// <returns>
        /// The root controller.
        /// </returns>
        public IControllerTree ImportControllerTree(IList<string> userDefinedInterfaces)
        {
            ImportAllControllers(userDefinedInterfaces);
            UpdateImportedControllers();
            return ControllerTree;
        }

        private Controller ImportControllerWithChildren(
            string rootControllerPath,
            IReadOnlyCollection<IControllerTag> controllerTags,
            IList<string> userDefinedInterfaces, int depth)
        {
            CheckCurrentDepth(rootControllerPath, depth);

            IControllerTag rootControllerTag;
            if (string.IsNullOrEmpty(rootControllerPath))
            {
                rootControllerTag = controllerTags.FirstOrDefault(ct => ct.GetParentControllerId() == NoControllerId);
            }
            else
            {
                rootControllerTag =
                    controllerTags.FirstOrDefault(ct => string.Equals(ct.GetScopedPath(), rootControllerPath, StringComparison.InvariantCultureIgnoreCase));
            }

            // the tag wasn't found, exit recursive call
            if (rootControllerTag == null)
            {
                return null;
            }

            Controller controller = _createController.Create(_tagController, rootControllerTag, userDefinedInterfaces, _tagListener);
            IEnumerable<IControllerTag> children = controllerTags.Where(ct => ct.GetParentControllerId() == controller.Id);
            foreach (IControllerTag child in children)
            {
                Controller createdControllerChild = ImportControllerWithChildren(child.GetScopedPath(), controllerTags, userDefinedInterfaces, depth - 1);
                controller.AddChild(createdControllerChild);
            }
            return controller;
        }

        private void CheckCurrentDepth(string rootControllerPath, int depth)
        {
            if (depth <= 0)
            {
                var message =
                    string.Format(
                        "The maximum depth of {0} has been reached to import children. The current rootControllerTag was {1}. " +
                        "Exiting now with importing additional children. " + "To support trouble shooting: " +
                        "This is a developer error and needs escalation to the development team. " +
                        "Most likely it's related to an invalid PLC path provided or to an incompatibility of the controller tree of the PLC and the way the controller tree was imported.",
                        MaxChildImportDepth, rootControllerPath);
                _logger.Error(message);
                Environment.Exit(99);
            }
        }
    }
}
