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

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// An implementer of <see cref="IPlatformModuleRepository"/> is capable of finding
    /// <see cref="IPlatformModule"/> instances.
    /// </summary>
    public interface IPlatformModuleRepository
    {
        /// <summary>
        /// Adds a new module.
        /// </summary>
        /// <param name="module">The module.</param>
        void AddNewModule(IPlatformModule module);

        /// <summary>
        /// Finds a <see cref="IPlatformModule"/> instance with the specified <paramref name="moduleName"/>.
        /// </summary>
        /// <param name="moduleName">The name of the desired module.</param>
        /// <returns>An <see cref="IPlatformModule"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Throws exception if the module was not found.</exception>
        IPlatformModule GetModule(string moduleName);

        /// <summary>
        /// Finds all <see cref="IPlatformModule"/> instances of a specified type/>.
        /// </summary>
        /// <typeparam name="T">Type of searched module.</typeparam>
        /// <returns>Module instances of type T or empty collection if such a module does not exist.</returns>
        IList<T> GetModulesByType<T>() where T : IPlatformModule;

        /// <summary>
        /// Finds a <see cref="IPlatformModule"/> instance of a specified type and instance number.
        /// </summary>
        /// <typeparam name="T">Type of searched module.</typeparam>
        /// <param name="moduleInstance">Instance id, starting with 1.</param>
        /// <returns>Module instance of type T and instance number.</returns>
        T GetModuleByType<T>(int moduleInstance) where T : IPlatformModule;

        /// <summary>
        /// Contains all <see cref="IPlatformModule"/> instances. 
        /// It is a list because the order matters: The index reflects the module instance number.
        /// </summary>
        IList<IPlatformModule> Modules { get; }
    }
}
