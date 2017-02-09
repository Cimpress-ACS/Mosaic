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
using System.ComponentModel.Composition;
using System.Diagnostics;
using VP.FF.PT.Common.Simulation.HardwareAbstraction;

namespace VP.FF.PT.Common.Simulation.Equipment
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("JunctionEquipment Position:{Position} OutputLanes:{_targetModules.Length}")]
    public class JunctionEquipment : ISimulatedEquipment
    {
        private SimulatedJunction _mosaicControlledJunction;
        private IModuleSimulator _originModule;
        private IModuleSimulator[] _targetModules;

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        public JunctionEquipment()
        {
            IsActive = true;
        }

        public void Initialize(int position, IModuleSimulator originModule, SimulatedJunction mosaicControlledJunction, params IModuleSimulator[] targetModules)
        {
            Position = position;
            _originModule = originModule;
            _targetModules = targetModules;
            _mosaicControlledJunction = mosaicControlledJunction;
        }

        public void ItemPassed(ISimulatedItem item)
        {
            if (IsActive)
            {
                ItemPassedCount++;

                long itemId = (long)item.ItemId;

                if (_mosaicControlledJunction.Routings.ContainsKey(itemId))
                {
                    int lane = _mosaicControlledJunction.Routings[itemId];

                    if (lane >= _targetModules.Length)
                        throw new IndexOutOfRangeException("mosaic routes to port " + lane +
                                                           " which does not exist in the simulation. origin module was " +
                                                           _originModule.Name);

                    _originModule.RemoveItem(item);
                    _targetModules[lane].AddItem(item);
                }

                _mosaicControlledJunction.SimulateRouting(item);
            }
        }
    }
}
