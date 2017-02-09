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

namespace VP.FF.PT.Common.Simulation.Equipment
{
    public enum JunctionDistributionMode
    {
        SortOutEverything,
        SortOutNothing,
        SortOutEverySecond,
        SortOutRandomly
    }

    /// <summary>
    /// This is a simple junction equipment which distributes items depending on a mode.
    /// There is no decision table or smart logic behind it.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("DistributionJunctionEquipment Position:{Position}")]
    public class DistributionJunctionEquipment : ISimulatedEquipment
    {
        private IModuleSimulator _originModule;
        private IModuleSimulator _targetModule;
        private readonly Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributionJunctionEquipment"/> class.
        /// </summary>
        public DistributionJunctionEquipment()
        {
            Mode = JunctionDistributionMode.SortOutEverySecond;
            _random = new Random();
            IsActive = true;
        }

        public JunctionDistributionMode Mode { get; set; }

        public void Initialize(int position, IModuleSimulator originModule, IModuleSimulator targetModule,
            JunctionDistributionMode mode = JunctionDistributionMode.SortOutEverySecond)
        {
            Position = position;
            _originModule = originModule;
            _targetModule = targetModule;
            Mode = mode;
        }

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }
        
        public void ItemPassed(ISimulatedItem item)
        {
            if (IsActive)
            {
                ItemPassedCount++;

                switch (Mode)
                {
                    case JunctionDistributionMode.SortOutNothing:
                        return;
                    case JunctionDistributionMode.SortOutEverything:
                        _originModule.RemoveItem(item);
                        _targetModule.AddItem(item);
                        break;
                    case JunctionDistributionMode.SortOutEverySecond:
                        if (ItemPassedCount%2 == 0)
                        {
                            _originModule.RemoveItem(item);
                            _targetModule.AddItem(item);
                        }
                        break;
                    case JunctionDistributionMode.SortOutRandomly:
                        if (_random.Next(0, 1) == 1)
                        {
                            _originModule.RemoveItem(item);
                            _targetModule.AddItem(item);
                        }
                        break;
                    default:
                        throw new NotImplementedException("JunctionDistributionMode");
                }
            }
        }
    }
}
