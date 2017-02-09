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
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    /// <summary>
    /// An implementer of <see cref="IAlarmManager"/> is capable of 
    /// managing current and historic alarms.
    /// </summary>
    public interface IAlarmManager
    {
        /// <summary>
        /// Notifies any subscriber when the <see cref="CurrentAlarms"/> or the <see cref="HistoricAlarms"/> changed.
        /// </summary>
        event Action AlarmsChanged;

        /// <summary>
        /// Gets the current alarms.
        /// </summary>
        IEnumerable<Alarm> CurrentAlarms { get; }

        /// <summary>
        /// Gets the historic alarms.
        /// </summary>
        IEnumerable<Alarm> HistoricAlarms { get; }

        /// <summary>
        /// Indicates whether there is any current error-level alarm.
        /// </summary>
        bool HasErrors { get; }

        /// <summary>
        /// Inidicates the module to which the AlarmManager belongs
        /// </summary>
        string ModuleName { get; set; }

        /// <summary>
        /// Indicates whether there is any current warning-level alarm.
        /// </summary>
        bool HasWarnings { get; }

        /// <summary>
        /// Current Alarms will be removed from current alarm list but be still available as historical alarms.
        /// </summary>
        void AcknowledgeAlarms();

        /// <summary>
        /// Adds the <paramref name="newAlarm"/> to the current alarms.
        /// Will do nothing if the alarm was already added before.
        /// </summary>
        /// <param name="newAlarm">A new <see cref="Alarm"/> instance.</param>
        void AddAlarm(Alarm newAlarm);

        /// <summary>
        /// Removes an alarm when existing (by Alarm-id).
        /// </summary>
        /// <param name="alarm">The alarm.</param>
        void RemoveAlarm(Alarm alarm);

        /// <summary>
        /// Tries to find an alarm that matches all the attributes and removes it when found.
        /// </summary>
        /// <param name="source">source of the alarm to remove</param>
        /// <param name="alarmId">alarmId of the alarm to remove (not to be confused with id)</param>
        /// <returns>number of removed alarms</returns>
        int RemoveAlarm(string source, int alarmId);
    }
}
