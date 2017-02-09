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


using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    /// <summary>
    /// Extension to the <see cref="IManageCurrentAlarmsPlugin"/> to also allow to add alarms, which only a subset of plugins will be able to support.
    /// </summary>
    public interface IAlarmAddingPlugin
    {
        /// <summary>
        /// Tries to add the specified <paramref name="alarm"/> to the current alarms.
        /// </summary>
        /// <param name="alarm">The <see cref="Alarm"/> instance to add.</param>
        void TryAddAlarm(Alarm alarm);
    }
}
