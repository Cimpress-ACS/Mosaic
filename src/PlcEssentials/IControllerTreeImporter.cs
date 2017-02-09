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


using System.Collections.Generic;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Imports and creates hierarchical PLC state machine controller tree.
    /// </summary>
    public interface IControllerTreeImporter
    {
        /// <summary>
        /// Gets the alarms importer used by this instance.
        /// </summary>
        IAlarmsImporter AlarmsImporter { get; }

        /// <summary>
        /// Initializes this instance with all necessary information needed to import controller trees.
        /// </summary>
        /// <param name="tagListener">The tag listener used to register the tags of the imported controller on.</param>
        /// <param name="path">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress.</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        /// <param name="rootController">The plcAddress of the root controller</param>
        void Initialize(
            ITagListener tagListener,
            string path,
            int port = 0,
            string rootController = "");

        /// <summary>
        /// Gets the <see cref="ControllerTree"/>
        /// </summary>
        IControllerTree ControllerTree { get; }

        /// <summary>
        /// Imports all controllers from the specified path and port.
        /// </summary>
        void ImportAllControllers();

        /// <summary>
        /// Imports all controllers from the specified path and port 
        /// and searchs for user defined interfaces for each controller.
        /// </summary>
        void ImportAllControllers(IList<string> userDefinedInterfaces);

        /// <summary>
        /// Updates all values of the imported controllers.
        /// </summary>
        void UpdateImportedControllers();

        /// <summary>
        /// Imports the controller tree recursively. Returns the controller Tree.
        /// </summary>
        /// <returns>
        /// Controller Tree.
        /// </returns>
        IControllerTree ImportControllerTree(IList<string> userDefinedInterfaces);
    }
}
