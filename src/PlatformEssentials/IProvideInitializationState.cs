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
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// An object implementing this interface provides the functionality
    /// to request and subscribe for information about the initalization state of Mosaic.
    /// </summary>
    public interface IProvideInitializationState
    {
        /// <summary>
        /// Subscribes specified <paramref name="handleModuleInInitialization"/> method to 
        /// get notified when a module is initialized.
        /// </summary>
        /// <param name="handleModuleInInitialization">The method to handle the event.</param>
        /// <returns>A <see cref="Task"/> for awaiting the return of this method.</returns>
        Task SubscribeForInitializationEvents(Action<string> handleModuleInInitialization);

        /// <summary>
        /// Requests information about the initialization state of all modules.
        /// </summary>
        /// <returns>true if all modules are initialized, else false.</returns>
        Task<bool> AreAllModulesInitialized();

        /// <summary>
        /// Requests the paper stream module graph.
        /// </summary>
        /// <returns>A <see cref="ModuleGraphDTO"/> instance.</returns>
        Task<ModuleGraphDTO> GetModuleGraph();

        /// <summary>
        /// Unsubscribe specified <paramref name="handleModuleInInitialization"/> method from 
        /// getting notified when a module is initialized.
        /// </summary>
        /// <param name="handleModuleInInitialization">The method to handle the event.</param>
        /// <returns>A <see cref="Task"/> for awaiting the return of this method.</returns>
        Task UnsubscribeFromInitializationEvents(Action<string> handleModuleInInitialization);

        /// <summary>
        /// Unsubscribe all common services events.
        /// </summary>
        void UnsubscribeEvents();
    }
}
