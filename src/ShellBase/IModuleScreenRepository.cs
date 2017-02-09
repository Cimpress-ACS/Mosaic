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
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;

namespace VP.FF.PT.Common.ShellBase
{
    public interface IModuleScreenRepository
    {
        /// <summary>
        /// Finds a <see cref="IModuleScreen"/> instance with the specified <paramref name="moduleKey"/>.
        /// </summary>
        /// <param name="moduleKey">The name of the desired module.</param>
        /// <returns>An <see cref="IModuleScreen"/> instance.</returns>
        IModuleScreen GetModule(string moduleKey);

        /// <summary>
        /// Finds all <see cref="IModuleScreen"/> instances of a specified type/>.
        /// </summary>
        /// <typeparam name="T">Type of searched module.</typeparam>
        /// <returns>Module instances of type T or empty collection if such a module does not exist.</returns>
        IList<T> GetModulesByType<T>();

        /// <summary>
        /// Finds a <see cref="IModuleScreen"/> instance of a specified type and instance number.
        /// </summary>
        /// <typeparam name="T">Type of searched module.</typeparam>
        /// <param name="moduleInstance">Instance id, starting with 1.</param>
        /// <returns>Module instance of type T and instance number.</returns>
        T GetModuleByType<T>(int moduleInstance) where T : IModuleScreen;

        /// <summary>
        /// Contains all <see cref="IModuleScreen"/> instances. 
        /// It is a list because the order matters: The index reflects the module instance number.
        /// </summary>
        IList<IModuleScreen> Modules { get; }
    }
}
