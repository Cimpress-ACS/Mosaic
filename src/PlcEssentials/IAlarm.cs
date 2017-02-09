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

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Holds informations of a PLC state controller alarm.
    /// </summary>
    public interface IAlarm
    {
        /// <summary>
        /// Gets or sets the Alarm Id which is unique.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        uint Id { get; }

        /// <summary>
        /// Gets or sets the alarm message text (not localized, use Id instead).
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        string Text { get; }

        /// <summary>
        /// Gets or sets the type of the alarm.
        /// </summary>
        /// <value>
        /// The type of the alarm.
        /// </value>
        AlarmType AlarmType { get; }

        /// <summary>
        /// Gets the info number about this alarm.
        /// </summary>
        int Info { get; }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the controller id of the alarm source.
        /// </summary>
        short SourceControllerId { get; }
    }
}
