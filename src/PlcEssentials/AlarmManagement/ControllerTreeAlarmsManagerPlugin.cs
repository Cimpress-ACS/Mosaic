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
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlcEssentials.AlarmManagement
{
    /// <summary>
    /// The <see cref="ControllerTreeAlarmsManagerPlugin"/> manages the current alarms
    /// which can be found on a given controller tree.
    /// </summary>
    public class ControllerTreeAlarmsManagerPlugin : IManageCurrentAlarmsPlugin
    {
        private readonly IControllerTree _controllerTree;
        private readonly object _lock = new object();
        private readonly List<Alarm> _alarmsCache = new List<Alarm>();

        /// <summary>
        /// Initializes a new <see cref="ControllerTreeAlarmsManagerPlugin"/> instance.
        /// </summary>
        /// <param name="controllerTree">The controller tree.</param>
        public ControllerTreeAlarmsManagerPlugin(IControllerTree controllerTree)
        {
            _controllerTree = controllerTree;
            AlarmAdded += _ => { };
            _controllerTree.AlarmsAdded += RaiseAlarmAddedEvent;
            _controllerTree.AlarmsRemoved += RaiseAlarmAddedEvent;
        }

        /// <summary>
        /// Notifies subscribers about changes in the current alarms.
        /// </summary>
        public event Action<Alarm> AlarmAdded;

        private void RaiseAlarmAddedEvent(IEnumerable<Impl.Alarm> alarms)
        {
            var entityAlarms = from alarm in alarms
                               let controller = _controllerTree.TryGetController(alarm.SourceControllerId)
                               let entitiesAlarm = this.CopyPlcAlarmToAlarm(alarm, controller != null ? controller.PlcControllerPath : string.Empty)
                               select entitiesAlarm;
            foreach (var a in entityAlarms)
            {
                AlarmAdded(a);
            }
        }

        /// <summary>
        /// Gets the current alarms.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        public IEnumerable<Alarm> GetCurrentAlarms()
        {
            lock (_lock)
            {
                _alarmsCache.Clear();
                foreach (IAlarm alarm in _controllerTree.GetAllAlarms())
                {
                    IController controller = _controllerTree.TryGetController(alarm.SourceControllerId);
                    Alarm entitiesAlarm = this.CopyPlcAlarmToAlarm(alarm,
                        controller != null ? controller.PlcControllerPath : string.Empty);
                    _alarmsCache.Add(entitiesAlarm);
                }
                return _alarmsCache.ToList();
            }
        }

        /// <summary>
        /// This AlarmsManagerPlugin implementation just looks if there is any specified alarm and acknowledge in this case all alarms.
        /// For this plugin it is technically not possible to acknowlegde single alarms.
        /// </summary>
        /// <param name="alarmsToRemove">An <see cref="IEnumerable{T}"/> of alarm instances.</param>
        public void TryRemoveAlarms(IEnumerable<Alarm> alarmsToRemove)
        {
            RemoveAlarmsHelper(alarmsToRemove);
        }

        public void ForceRemoveAlarms(IEnumerable<Alarm> alarmsToRemove)
        {
            RemoveAlarmsHelper(alarmsToRemove);
        }

        private void RemoveAlarmsHelper(IEnumerable<Alarm> alarmsToRemove)
        {
            lock (_lock)
            {
                if (alarmsToRemove.Any(_alarmsCache.Contains))
                {
                    _controllerTree.AcknowledgeAlarms();
                }
            }
        }

        public int ForceRemoveAlarms(string source, int alarmId)
        {
            lock (_lock)
            {
                _controllerTree.AcknowledgeAlarms();
            }
            return 0;
        }
    }
}
