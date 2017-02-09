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
using VP.FF.PT.Common.Simulation.Equipment;

namespace VP.FF.PT.Common.Simulation
{
    [Export(typeof(IEquipmentRepository))]
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly Dictionary<IModuleSimulator, List<ISimulatedEquipment>> _equipments = new Dictionary<IModuleSimulator, List<ISimulatedEquipment>>();
        private readonly Dictionary<ISimulatedEquipment, IModuleSimulator> _equipmentToModuleMapping = new Dictionary<ISimulatedEquipment, IModuleSimulator>();

        public void AddEquipment(ISimulatedEquipment equipment, IModuleSimulator module)
        {
            if (!_equipments.ContainsKey(module))
                _equipments.Add(module, new List<ISimulatedEquipment>());

            var alreadyAssignedToModule = GetModuleOfEquipment(equipment);
            if (alreadyAssignedToModule != null)
            {
                throw new SimulationException(string.Format("The equipment {0} is already assigned to module {1}", 
                    equipment.GetType().Name, alreadyAssignedToModule.Name));
            }

            _equipments[module].Add(equipment);
            _equipmentToModuleMapping.Add(equipment, module);
        }

        public IList<ISimulatedEquipment> GetEquipmentsOfModule(IModuleSimulator module)
        {
            if (!_equipments.ContainsKey(module))
                return new List<ISimulatedEquipment>();

            return _equipments[module];
        }

        public IList<ISimulatedEquipment> GetEquipmentsOfModule(string moduleName)
        {
            var equipments = from e in _equipments
                             where e.Key.Name == moduleName
                             select e.Value;

            if (equipments.IsNullOrEmpty())
                return new List<ISimulatedEquipment>();

            return equipments.Single();
        }

        /// <summary>
        /// Gets the module where the equipment was assigned to.
        /// </summary>
        /// <param name="equipment">The equipment.</param>
        /// <returns>The ModuleSimulator or null if the equipment was never assigned to any module.</returns>
        public IModuleSimulator GetModuleOfEquipment(ISimulatedEquipment equipment)
        {
            IModuleSimulator ret;
            _equipmentToModuleMapping.TryGetValue(equipment, out ret);
            return ret;
        }
    }
}
