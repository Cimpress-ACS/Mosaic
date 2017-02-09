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
    /// This equipment queues up items on a slot in a FIFO order.
    /// The items in the queue will not move forward when a "Takt" occures but it will be buffered in this equipment until it gets released.
    /// </summary>
    [Export]
    [Export(typeof(IAlarmSource))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("QueueEquipment Position:{Position} Capacity:{Capacity}")]
    public class QueueEquipment : ISimulatedEquipment, IAlarmSource
    {
        private readonly object _lock = new object();
        private IModuleSimulator _module;
        private readonly Subject<Alarm> _alarmOccurredSubject = new Subject<Alarm>();
        private readonly SimulatedItemList _queue = new SimulatedItemList();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueEquipment" /> class.
        /// </summary>
        [ImportingConstructor]
        public QueueEquipment()
        {
            IsActive = true;
            _queue.IsFrozen = true;
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
        /// Gets the list which is used like a queue. First item was last added to the queue.
        /// </summary>
        public SimulatedItemList QueueList
        {
            get { return _queue; }
        }

        public IObservable<Alarm> Alarms
        {
            get
            {
                return _alarmOccurredSubject.AsObservable();
            }
        }

        /// <summary>
        /// Initializes the <see cref="QueueEquipment" />.
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
        public void ReleaseItem()
        {
            lock (_lock)
            {
                if (_queue.Items.IsNullOrEmpty() || !IsActive)
                {
                    return;
                }

                var item = _queue.Items.Last();
                _queue.Items.Remove(item);

                // udate itemId if there is a remaining item in the queue
                if (_queue.Items.LastOrDefault() != null)
                {
                    _queue.ItemId = _queue.Items.Last().ItemId;
                }

                if (_queue.Items.IsNullOrEmpty())
                {
                    _module.RemoveItem(_queue);
                }

                _module.AddItem(item, Position + 1);
            }
        }

        /// <summary>
        /// The equipment will remove the single item and put it into a fifo list which is assigned to the slot.
        /// </summary>
        /// <param name="item"></param>
        public void ItemPassed(ISimulatedItem item)
        {
            lock (_lock)
            {
                if (!IsActive || item == _queue)
                {
                    return;
                }

                ItemPassedCount++;

                _module.RemoveItem(item);

                // create and add new queue if not done
                if (_module.GetItemByPosition(Position) == null)
                {
                    _module.AddItem(_queue, Position);
                }

                _queue.Items.Insert(0, item);

                // itemId of the overall queue is the last added item
                _queue.ItemId = item.ItemId;

                CheckCapacity();
            }
        }

        private void CheckCapacity()
        {
            if (Capacity > 0 && _queue.Items.Count > Capacity)
            {
                _alarmOccurredSubject.OnNext(new Alarm
                {
                    Source = this,
                    Type = AlarmType.Warning,
                    Message = "QueueEqupment count exceeded max capacity of " + Capacity
                });
            }
        }
    }
}
