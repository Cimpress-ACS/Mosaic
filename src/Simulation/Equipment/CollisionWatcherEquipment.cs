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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using VP.FF.PT.Common.Simulation.Alarms;

namespace VP.FF.PT.Common.Simulation.Equipment
{
    /// <summary>
    /// Whenever an items passes this equipment it will log it to the history and raise an Alarm.
    /// </summary>
    [Export]
    [Export(typeof(IAlarmSource))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("CollisionWatcherEquipment Position:{Position} DetectedItems¨:{DetectedItems.Count}")]
    public class CollisionWatcherEquipment : ISimulatedEquipment, IAlarmSource
    {
        private readonly Subject<Alarm> _alarmOccurredSubject = new Subject<Alarm>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CollisionWatcherEquipment"/> class.
        /// </summary>
        [ImportingConstructor]
        public CollisionWatcherEquipment()
        {
            IsActive = true;
            DetectedItems = new List<ISimulatedItem>();
        }

        /// <summary>
        /// Gets the detected items which had a collision.
        /// </summary>
        public IList<ISimulatedItem> DetectedItems { get; private set; }

        public bool IsActive { get; set; }
        public int Position { get; private set; }
        public ulong ItemPassedCount { get; private set; }

        /// <summary>
        /// Initializes the equipment.
        /// </summary>
        /// <param name="position">The slot position of the equipment.</param>
        public void Initialize(int position)
        {
            Position = position;
        }

        public void ItemPassed(ISimulatedItem item)
        {
            if (!IsActive)
            {
                return;
            }

            ItemPassedCount++;

            DetectedItems.Add(item);

            item.AddLog("collision detected", "CollisionWatcherEquipment");

            _alarmOccurredSubject.OnNext(new Alarm
            {
                Source = this,
                Message = "Collision detected of item " + item.ItemId,
                Type = AlarmType.Warning
            });
        }

        public IObservable<Alarm> Alarms
        {
            get
            {
                return _alarmOccurredSubject.AsObservable();
            }
        }
    }
}
