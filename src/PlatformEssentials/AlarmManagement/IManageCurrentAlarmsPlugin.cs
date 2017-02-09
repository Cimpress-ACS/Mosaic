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
    /// An implementer of <see cref="IManageCurrentAlarmsPlugin"/> provides 
    /// an a specific way to handle current alarms.
    /// </summary>
    public interface IManageCurrentAlarmsPlugin
    {
        /// <summary>
        /// Notifies subscribers about one or more added alarms to the current alarms.
        /// </summary>
        event Action<Alarm> AlarmAdded;

        /// <summary>
        /// Gets the current alarms.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        IEnumerable<Alarm> GetCurrentAlarms();

        /// <summary>
        /// Tries to remove the specified <paramref name="alarmsToRemove"/> from the current alarms.
        /// </summary>
        /// <param name="alarmsToRemove">An <see cref="IEnumerable{T}"/> of alarm instances.</param>
        void TryRemoveAlarms(IEnumerable<Alarm> alarmsToRemove);

        /// <summary>
        /// Tries to remove the specified <paramref name="alarmsToRemove"/> from the current alarms, thereby ignoring the "IsResettable" attribute.
        /// </summary>
        /// <param name="alarmsToRemove">An <see cref="IEnumerable{T}"/> of alarm instances.</param>
        void ForceRemoveAlarms(IEnumerable<Alarm> alarmsToRemove);
        
        /// <summary>
        /// Tries to find an alarm that matches all the attributes and removes it when found, thereby ignoring the "IsResettable" attribute.
        /// </summary>
        /// <param name="source">source of the alarm to remove</param>
        /// <param name="alarmId">alarmId of the alarm to remove (not to be confused with id)</param>
        /// <returns>number of removed alarms</returns>
        int ForceRemoveAlarms(string source, int alarmId);
    }
}
