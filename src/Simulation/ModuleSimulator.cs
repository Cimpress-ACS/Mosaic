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
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.Simulation.Equipment;
using Alarm = VP.FF.PT.Common.Simulation.Alarms.Alarm;
using AlarmType = VP.FF.PT.Common.Simulation.Alarms.AlarmType;

namespace VP.FF.PT.Common.Simulation
{
    [DebuggerDisplay("{Name}  ItemCount:{ItemCount} Length:{Length} Equipments:{Equipments.Count}")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModuleSimulator : IModuleSimulator
    {
        private readonly ILogger _logger;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IDictionary<ISimulatedItem, int> _itemPositions = new Dictionary<ISimulatedItem, int>();
        private readonly Subject<SimulatedItemLeftModuleData> _itemLeftSubject = new Subject<SimulatedItemLeftModuleData>();
        private readonly Subject<Alarm> _alarmOccurredSubject = new Subject<Alarm>();
        private readonly IList<ISimulatedEquipment> _simulatedEquipments = new List<ISimulatedEquipment>();

        [Import] 
        internal ITaktPartsRepository TaktPartsRepository;

        [ImportingConstructor]
        public ModuleSimulator(ILogger logger, IEquipmentRepository equipmentRepository)
        {
            _logger = logger;
            _equipmentRepository = equipmentRepository;
            _logger.Init(GetType());
        }

        public void Initialize(int length, string name)
        {
            Length = length;
            Name = name;
            IsActive = true;
        }

        public void ReactOnPlatformModuleState(IPlatformModule platformModule)
        {
            platformModule.ModuleStateChangedEvent += (sender, state) =>
            {
                if (state == PlatformModuleState.Run)
                    IsActive = true;
                else
                    IsActive = false;
            };
        }

        public void AddEquipment(ISimulatedEquipment equipment)
        {
            if (equipment.Position < 0 || equipment.Position > Length - 1)
            {
                throw new SimulationException("The equipment position " + equipment.Position + " exceeds the ModuleSimulator range of " + Name);
            }

            _equipmentRepository.AddEquipment(equipment, this);

            _simulatedEquipments.Add(equipment);

            var taktPart = equipment as ITakt;
            if (taktPart != null)
            {
                TaktPartsRepository.AddTaktPart(taktPart);
            }
        }

        public void RemoveEquipment(ISimulatedEquipment equipment)
        {
            equipment.IsActive = false;

            _simulatedEquipments.Remove(equipment);
        }

        public IEnumerable<ISimulatedEquipment> Equipments
        {
            get { return _simulatedEquipments; }
        }

        public string Name { get; private set; }

        public int Length { get; private set; }

        public ISimulatedItem GetItemById(ulong itemId)
        {
            return _itemPositions.FirstOrDefault(i => i.Key.ItemId == itemId).Key;
        }

        public int GetPositionOfItem(ulong itemId)
        {
            var item = _itemPositions.Keys.FirstOrDefault(i => i.ItemId == itemId);

            if (item == null)
            {
                return -1;
            }
            
            return _itemPositions[item];
        }

        public ISimulatedItem GetItemByPosition(int position)
        {
            return _itemPositions.FirstOrDefault(i => i.Value == position).Key;
        }

        public IObservable<SimulatedItemLeftModuleData> ItemLeft
        {
            get { return _itemLeftSubject.AsObservable(); }
        }

        public IObservable<Alarm> Alarms
        {
            get { return _alarmOccurredSubject.AsObservable(); }
        }

        public int ItemCount
        {
            get { return _itemPositions.Count; }
        }

        public bool IsActive { get; set; }

        public void AddItem(ISimulatedItem item)
        {
            AddItem(item, 0);
        }

        public void AddItem(ISimulatedItem item, int position)
        {
            if (position >= Length || position < 0)
            {
                throw new ArgumentOutOfRangeException("position", "index position " + position + " does not exist in " + Name);
            }

            if (_itemPositions.Any(i => i.Value == position))
            {
                _alarmOccurredSubject.OnNext(new Alarm
                {
                    Source = this,
                    Type = AlarmType.Warning,
                    Message = "Possible collision: Can't add item " + item.ItemId + " to index " + position + 
                    " because it's aleady occupied by item " + _itemPositions.First(i => i.Value == position).Key.ItemId
                });
            }

            _itemPositions.Add(item, position);

            // process single equipment at position if existing
            var equipment = _simulatedEquipments.FirstOrDefault(e => e.Position == position);
            if (equipment != null)
            {
                equipment.ItemPassed(item);
            }
        }

        public void RemoveItem(ISimulatedItem item)
        {
            _itemPositions.Remove(item);
        }

        public void Takt()
        {
            if (!IsActive)
            {
                return;
            }

            MoveItems();

            ProcessEquipments();
            
            HandleItemLeft();

            // todo: detect collisions
        }

        private void MoveItems()
        {
            foreach (var simulatedItem in _itemPositions.Keys.ToArray())
            {
                if (!simulatedItem.IsFrozen)
                {
                    _itemPositions[simulatedItem]++;
                }
            }
        }

        private void ProcessEquipments()
        {
            var itemEvents = (from se in _simulatedEquipments
                                    join itemPosition in _itemPositions on se.Position equals itemPosition.Value
                                    select new { Equipment = se, Item = itemPosition.Key }).ToArray();

            foreach (var itemEvent in itemEvents)
            {
                try
                {
                    itemEvent.Equipment.ItemPassed(itemEvent.Item);
                    itemEvent.Item.AddLog("passed equipment " + itemEvent.Equipment.GetType().Name + " at position " + itemEvent.Equipment.Position, Name);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Error while simulating equipment '{0}' in module simulator '{1}'.", itemEvent.Equipment, Name);
                    _logger.Error(msg, e);
                }
            }
        }

        private void HandleItemLeft()
        {
            var removedItems = _itemPositions.Where(i => i.Value >= Length).ToArray();
            foreach (var simulatedItem in removedItems)
            {
                _itemLeftSubject.OnNext(new SimulatedItemLeftModuleData { Item = simulatedItem.Key, OriginModuleSimulator = this, OriginPort = 0 });
            }
        }

    }
}
