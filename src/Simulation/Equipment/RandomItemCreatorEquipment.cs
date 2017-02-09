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
    /// <summary>
    /// This equipment is an item source and creates new items in a random takt intervall.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("RandomItemCreatorEquipment Position:{Position} CreatedItemCount:{CreatedItemCount}")]
    public class RandomItemCreatorEquipment : ISimulatedEquipment, ITakt
    {
        private IModuleSimulator _module;
        private int _numberOfTaktsBeforeCreateItem;
        private int _taktCount;

        public event EventHandler<ItemCreatorEventArgs> ItemCreated;

        public RandomItemCreatorEquipment()
        {
            IsActive = true;
        }

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        public void Initialize(int position, IModuleSimulator module, int numberOfTaktsBeforeCreateItem)
        {
            _module = module;
            Position = position;
            NumberOfTaktsBeforeCreateItem = numberOfTaktsBeforeCreateItem;
        }

        public int NumberOfTaktsBeforeCreateItem 
        {
            get
            {
                return _numberOfTaktsBeforeCreateItem;
            } 
            
            set
            {
                if (value <= 0)
                {
                    throw new Exception("NumberOfTaktsBeforeCreateItem must be >= 1");
                }

                _numberOfTaktsBeforeCreateItem = value;
            } 
        }

        public void ItemPassed(ISimulatedItem item)
        {
        }

        public int CreatedItemCount { get; private set; }

        public void Takt()
        {
            if (_module == null || !_module.IsActive || !IsActive)
            {
                return;
            }

            ItemPassedCount++;

            if (++_taktCount >= NumberOfTaktsBeforeCreateItem)
            {
                _taktCount = 0;

                var item = new SimulatedItem();
                item.Metadata.Add("barcode", "123-456-" + CreatedItemCount);

                item.AddLog("item was created by an RandomItemCreatorEquipment after " + NumberOfTaktsBeforeCreateItem + " takts", _module.Name);

                _module.AddItem(item, Position);

                CreatedItemCount++;

                if (ItemCreated != null)
                    ItemCreated(this, new ItemCreatorEventArgs(item));
            }
        }
    }

    public class ItemCreatorEventArgs : EventArgs
    {
        public ItemCreatorEventArgs(ISimulatedItem simulatedItem)
        {
            CreatedItem = simulatedItem;
        }

        public ISimulatedItem CreatedItem { get; private set; }
    }
}
