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
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using VP.FF.PT.Common.Simulation.Alarms;

namespace VP.FF.PT.Common.Simulation.Equipment
{
    /// <summary>
    /// This equipment adds incoming items to a stack.
    /// The stacker adds SimulatedItem's to a single SimulatedItemList until it get's released.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(IAlarmSource))]
    [DebuggerDisplay("StackerEquipment Position:{Position} Capacity:{Capacity}")]
    public class StackerEquipment : ISimulatedEquipment, IAlarmSource
    {
        private readonly object _lock = new object();
        private IModuleSimulator _module;
        private readonly Subject<Alarm> _alarmOccurredSubject = new Subject<Alarm>();
        private readonly SimulatedItemList _stack = new SimulatedItemList();

        /// <summary>
        /// Initializes a new instance of the <see cref="StackerEquipment" /> class.
        /// </summary>
        [ImportingConstructor]
        public StackerEquipment()
        {
            IsActive = true;
            _stack.IsFrozen = true;
        }

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        /// <summary>
        /// Gets or sets the maximum queue capacity for this equpment. If the count exceeds this value an warning Alarm will be raised.
        /// A value of 0 means inifinite capacity.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets the list which is used like a stack.
        /// </summary>
        public SimulatedItemList StackList
        {
            get { return _stack; }
        }

        public IObservable<Alarm> Alarms
        {
            get
            {
                return _alarmOccurredSubject.AsObservable();
            }
        }

        /// <summary>
        /// Initializes the <see cref="StackerEquipment" />.
        /// </summary>
        /// <param name="position">The slot position of the equipment.</param>
        /// <param name="module">This equipment needs a reference to the module where it is attached.</param>
        public void Initialize(int position, IModuleSimulator module)
        {
            Position = position;
            _module = module;
        }

        /// <summary>
        /// Releases the first item of the queue (if existing) and adds it to the next module slot.
        /// </summary>
        public void ReleaseStack()
        {
            lock (_lock)
            {
                if (!_stack.Items.Any() || !IsActive)
                {
                    return;
                }

                // move whole stack by 1 slot
                _module.RemoveItem(_stack);

                var stackCopy = _stack.Clone();
                stackCopy.IsFrozen = false;

                _module.AddItem(stackCopy, Position + 1);
                _stack.Items.Clear();
            }
        }

        public void ItemPassed(ISimulatedItem item)
        {
            lock (_lock)
            {
                if (!IsActive || item == _stack)
                {
                    return;
                }

                ItemPassedCount++;

                _module.RemoveItem(item);

                // create and add new stack if not done
                if (_module.GetItemByPosition(Position) == null)
                {
                    _module.AddItem(_stack, Position);
                }

                _stack.Items.Add(item);

                // itemId of the overall stack is the last added item
                _stack.ItemId = item.ItemId;

                CheckCapacity();
            }
        }

        private void CheckCapacity()
        {
            if (_stack.Items.Count > Capacity)
            {
                _alarmOccurredSubject.OnNext(new Alarm
                {
                    Source = this,
                    Type = AlarmType.Warning,
                    Message = "StackerEquipment count exceeded max capacity of " + Capacity
                });
            }
        }
    }
}
