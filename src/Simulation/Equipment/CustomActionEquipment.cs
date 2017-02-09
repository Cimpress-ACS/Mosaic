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
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("CustomActionEquipment Position:{Position}")]
    public class CustomActionEquipment : ISimulatedEquipment
    {
        private Action<ISimulatedItem> _action;
        
        public CustomActionEquipment()
        {
            IsActive = true;
        }

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomActionEquipment"/> class.
        /// </summary>
        /// <param name="position">The slot position of the equipment.</param>
        /// <param name="action">The action.</param>
        public void Initialize(int position, Action<ISimulatedItem> action)
        {
            _action = action;
            Position = position;
        }

        public void ItemPassed(ISimulatedItem item)
        {
            if (IsActive)
            {
                _action(item);
                ItemPassedCount++;
            }
        }
    }
}
