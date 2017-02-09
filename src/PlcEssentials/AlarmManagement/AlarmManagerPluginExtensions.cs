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
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlcEssentials.AlarmManagement
{
    internal static class AlarmManagerPluginExtensions
    {
        public static Alarm CopyPlcAlarmToAlarm(this IManageCurrentAlarmsPlugin alarmPlugin, IAlarm plcAlarm, string source)
        {
            DateTime timeStamp = plcAlarm.Timestamp;

            var alarm = new Alarm
            {
                AlarmId = (int)plcAlarm.Id,
                Type = plcAlarm.AlarmType.ToEntity(),
                Message = plcAlarm.Text,
                Source = source,
                Timestamp = timeStamp,
                SourceType = AlarmSourceType.Plc
            };

            return alarm;
        }
    }
}
