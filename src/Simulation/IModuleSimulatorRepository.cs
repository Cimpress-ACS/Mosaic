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

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// Container for all modules.
    /// </summary>
    public interface IModuleSimulatorRepository
    {
        /// <summary>
        /// Adds a new simulated module.
        /// </summary>
        /// <param name="module">The module.</param>
        void AddModule(IModuleSimulator module);

        /// <summary>
        /// Finds a instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An instance.</returns>
        IModuleSimulator GetModule(string name);

        /// <summary>
        /// Gets the modules.
        /// </summary>
        IEnumerable<IModuleSimulator> Modules { get; }
    }
}
