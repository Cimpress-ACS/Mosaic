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


namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Describes the alarm type.
    /// </summary>
    public enum AlarmType : short
    {
        /// <summary>
        /// Indicates that no alarm is active at the moment.
        /// </summary>
        None = 0,

        /// <summary>
        /// This alarm is used a an info message. Machine will not stop.
        /// </summary>
        Info = 1,

        /// <summary>
        /// Warning alarm. Machine will not stop.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Tact stop alarm.
        /// Complete cycle and stop afterwards.
        /// </summary>
        TactStop = 3,

        /// <summary>
        /// Stop alarm.
        /// Operator must confirm alarm to continue.
        /// </summary>
        /// <remarks>
        /// E.g. servos will shutdown normally (power is still available).
        /// </remarks>
        Stop = 4,

        /// <summary>
        /// Off alarm (similar to EmergencyOff).
        /// </summary>
        Off = 5,

        /// <summary>
        /// Emergency stop alarm
        /// </summary>
        EmergencyOff = 6
    }
}
