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
using System.Linq;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    /// <summary>
    /// The <see cref="InMemoryManageCurrentAlarmsPlugin"/> plugin
    /// is capable of storing alarms and categorize them in current alarms.
    /// </summary>
    public class InMemoryManageCurrentAlarmsPlugin : IManageCurrentAlarmsPlugin, IAlarmAddingPlugin
    {
        private readonly List<Alarm> _currentAlarms;
        private IReadOnlyCollection<Alarm> _snapshot;

        /// <summary>
        /// Initializes a new <see cref="InMemoryManageCurrentAlarmsPlugin"/> instance.
        /// </summary>
        public InMemoryManageCurrentAlarmsPlugin()
        {
            _currentAlarms = new List<Alarm>();
            _snapshot = _currentAlarms.ToReadOnly();
            AlarmAdded += _ => { };
        }

        /// <summary>
        /// Notifies subscribers about changes in the current alarms.
        /// </summary>
        public event Action<Alarm> AlarmAdded;

        /// <summary>
        /// Gets the current alarms.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        public IEnumerable<Alarm> GetCurrentAlarms()
        {
            return _snapshot;
        }

        /// <summary>
        /// Tries to add the specified <paramref name="alarm"/> to the current alarms.
        /// </summary>
        /// <param name="alarm">The <see cref="Alarm"/> instance to add.</param>
        public void TryAddAlarm(Alarm alarm)
        {
            lock (_currentAlarms)
            {
                _currentAlarms.RemoveAll(a => a.Equals(alarm) && a.Timestamp < alarm.Timestamp);
                if (_currentAlarms.Any(a => a.Equals(alarm)))
                {
                    return;
                }
                _currentAlarms.Add(alarm);
                _snapshot = _currentAlarms.ToReadOnly();
            }
            AlarmAdded(alarm);
        }

        public void TryRemoveAlarms(IEnumerable<Alarm> alarmsToRemove)
        {
            RemoveAlarmsHelper(alarmsToRemove, true);
        }

        public void ForceRemoveAlarms(IEnumerable<Alarm> alarmsToRemove)
        {
            RemoveAlarmsHelper(alarmsToRemove, false);
        }

        private void RemoveAlarmsHelper(IEnumerable<Alarm> alarmsToRemove, bool onlyRemoveIsResettableAlarms)
        {
            lock (_currentAlarms)
            {
                foreach (Alarm alarm in alarmsToRemove)
                    if (!onlyRemoveIsResettableAlarms || alarm.IsResettable == true)
                        _currentAlarms.Remove(alarm);
                _snapshot = _currentAlarms.ToReadOnly();
            }
        }

        public int ForceRemoveAlarms(string source, int alarmId)
        {
            int numOfRemovedAlarms = 0;

            lock (_currentAlarms)
            {
                var alarmsToRemove = new List<Alarm>();
                foreach (Alarm alarm in _currentAlarms)
                {
                    if (alarm.Source == source && alarm.AlarmId == alarmId)
                    {
                        alarmsToRemove.Add(alarm);
                    }
                }

                numOfRemovedAlarms = alarmsToRemove.Count();
                foreach (Alarm alarm in alarmsToRemove)
                {
                    _currentAlarms.Remove(alarm);
                }

                _snapshot = _currentAlarms.ToReadOnly();
            }

            return numOfRemovedAlarms;
        }
    }
}
