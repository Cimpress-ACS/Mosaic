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


namespace VP.FF.PT.Common.PlcEssentials.AlarmManagement
{
    internal static class AlarmTypeExtensions
    {
        public static PlatformEssentials.Entities.AlarmType ToEntity(this AlarmType plcAlarmType)
        {
            switch (plcAlarmType)
            {
                case Common.PlcEssentials.AlarmType.Info:
                    return PlatformEssentials.Entities.AlarmType.Info;
                case Common.PlcEssentials.AlarmType.Warning:
                    return PlatformEssentials.Entities.AlarmType.Warning;
                default:
                    return PlatformEssentials.Entities.AlarmType.Error;
            }
        }
    }
}
