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
using System.Threading.Tasks;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public interface IModuleBusManager : IPlatformModuleRouteForcing
    {
        /// <summary>
        /// Gets a value indicating if this instance has finished the initialization
        /// of all dependent modules.
        /// </summary>
        /// <value>
        ///   true if the initialization routine was called at least once, false if not.
        /// </value>
        bool HasFinishedInitializationOfAllModules { get; }

        /// <summary>
        /// Notifies the subscribers that the module initialization has started.
        /// </summary>
        event Action<string> ModuleInitializationStarted;

        /// <summary>
        /// Construct the modules
        /// </summary>
        void Construct();

        /// <summary>
        /// Initializes the internal graph and the modules.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Activates the modules
        /// </summary>
        void Activate();

        /// <summary>
        /// Return true if the module stream is ready for production (all modules are ready and accept items).
        /// </summary>
        /// <returns></returns>
        AcceptResult ReadyForProduction();

        /// <summary>
        /// Add a platform item to the module stream
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<AcceptResult> AddItemAsync(PlatformItem item);

        /// <summary>
        /// Gets the graph representing routing of items.
        /// </summary>
        ModuleGraphDTO GraphDto { get; }

        /// <summary>
        /// Sets a flag whether a module ignores the downstream module or not.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="ignore">if set to <c>true</c> module ignores the state of the downstream modules.</param>
        void SetIgnoreDownstreamModule(string moduleName, bool ignore);

        /// <summary>
        /// Gets a flag indicating a module ignores the downstream modules.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        bool GetIgnoreDownstreamModule(string moduleName);
    }
}
