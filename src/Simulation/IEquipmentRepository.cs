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
using VP.FF.PT.Common.Simulation.Equipment;

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// Container which allows to browse for all Equipments of the entire simulation.
    /// </summary>
    public interface IEquipmentRepository
    {
        /// <summary>
        /// Adds the equipment.
        /// </summary>
        /// <param name="equipment">The equipment.</param>
        /// <param name="module">The module.</param>
        /// <exception cref="SimulationException">An equipment can only be assigned to one module at a time.</exception>
        void AddEquipment(ISimulatedEquipment equipment, IModuleSimulator module);

        /// <summary>
        /// Gets the equipments of module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <returns>List of equipments. Can be empty if no equipment exists.</returns>
        IList<ISimulatedEquipment> GetEquipmentsOfModule(IModuleSimulator module);

        /// <summary>
        /// Gets the equipments of module.
        /// </summary>
        /// <param name="moduleName">Name of the module which is should be unique because it's used as a primary key.</param>
        /// <returns>List of equipments. Can be empty if no equipment exists.</returns>
        IList<ISimulatedEquipment> GetEquipmentsOfModule(string moduleName);

        /// <summary>
        /// Gets the module where the equipment was assigned to.
        /// </summary>
        /// <param name="equipment">The equipment.</param>
        /// <returns>The ModuleSimulator or null if the equipment was never assigned to any module.</returns>
        IModuleSimulator GetModuleOfEquipment(ISimulatedEquipment equipment);
    }
}
