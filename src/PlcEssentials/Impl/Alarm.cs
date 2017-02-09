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
using System.Globalization;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    /// <summary>
    /// Holds informations of a PLC state controller alarm.
    /// </summary>
    public class Alarm : IAlarm
    {
        /// <summary>
        /// Creates a <see cref="Tag"/> instance usable for reading the maximum number of possible alarms on the plc.
        /// </summary>
        /// <returns>A <see cref="Tag"/> instance.</returns>
        public static Tag PlcMaximumAlarmsTag()
        {
            string tagName = string.Format("{0}.{1}", NamingConventions.PathAlarmManager, NamingConventions.AlarmManagerMaxAlarms);
            return new Tag(tagName, NamingConventions.Global, IEC61131_3_DataTypes.UInt);
        }

        /// <summary>
        /// Creates a <see cref="Tag"/> instance usable for reading the alarms information from a PLC.
        /// </summary>
        /// <param name="maxAlarms">The maximum number of alarms possible on the PLC.</param>
        /// <returns>A <see cref="Tag"/> instance.</returns>
        public static Tag PlcArrayTag(int maxAlarms)
        {
            string tagName = string.Format("{0}.{1}", NamingConventions.PathAlarmManager, NamingConventions.AlarmManagerAlarmArray);
            return new Tag(tagName, NamingConventions.Global, PlcArrayDataType(maxAlarms)) { ArrayCreator = () => new AlarmArrayList(), StopReadingArrayPositions = StopReadingMoreAlarms };
        }

        private static bool StopReadingMoreAlarms(object value)
        {
            var alarm = value as PlcAlarmStruct;

            // stop if it's an alarm, but it's ID is 0. Then all other alarms will also be 0/null and it's waste of resources to read all of them
            // until the max array length has been reached
            return alarm != null && alarm.Id == 0;
        }

        /// <summary>
        /// Creates a string instance defining the plc array data type of an alarm.
        /// </summary>
        /// <param name="maxAlarms">The maximum number of alarms possible on the PLC</param>
        /// <returns>A string instance.</returns>
        public static string PlcArrayDataType(int maxAlarms)
        {
            return string.Format("ARRAY[0..{0}] OF T_AlmElement", maxAlarms);
        }
        
        public static Alarm CopyPlcAlarmToAlarm(PlcAlarmStruct plcAlarm)
        {
            DateTime timeStamp;
            try
            {
                timeStamp = string.IsNullOrEmpty(plcAlarm.EntryTime)
                    ? DateTime.MinValue
                    : DateTime.ParseExact(plcAlarm.EntryTime, NamingConventions.DateTimeString, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                timeStamp = DateTime.MinValue;
            }

            var alarm = new Alarm(plcAlarm, timeStamp);

            return alarm;
        }

        private readonly PlcAlarmStruct _innerAlarmStruct;
        private readonly DateTime _timestamp;

        /// <summary>
        /// Initializes a new <see cref="Alarm"/> instance.
        /// </summary>
        /// <param name="alarmStruct">The alarm struct containing the data of the alarm.</param>
        /// <param name="timestamp">The timestamp when the alarm was detected.</param>
        public Alarm(PlcAlarmStruct alarmStruct, DateTime timestamp)
        {
            _innerAlarmStruct = alarmStruct;
            _timestamp = timestamp;
        }

        /// <summary>
        /// Gets the Alarm Id which is unique.
        /// </summary>
        public uint Id
        {
            get { return _innerAlarmStruct.Id; }
        }

        /// <summary>
        /// Gets the controller id of the alarm source.
        /// </summary>
        public short SourceControllerId
        {
            get { return _innerAlarmStruct.SourceId; }
        }

        /// <summary>
        /// Gets the alarm message text (not localized, use Id instead).
        /// </summary>
        public string Text
        {
            get { return _innerAlarmStruct.Text; }
        }

        /// <summary>
        /// Gets the type of the alarm.
        /// </summary>
        public AlarmType AlarmType
        {
            get { return (AlarmType)_innerAlarmStruct.AlarmClass; }
        }

        /// <summary>
        /// Gets the state of the alarm.
        /// </summary>
        public AlarmState AlarmState
        {
            get { return (AlarmState)_innerAlarmStruct.AlarmState; }
        }

        /// <summary>
        /// Gets the number of the alarm.
        /// </summary>
        public uint AlarmNumber
        {
            get { return _innerAlarmStruct.AlarmNumber; }
        }

        /// <summary>
        /// Gets the info number about this alarm.
        /// </summary>
        public int Info
        {
            get { return _innerAlarmStruct.Info; }
        }

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public static bool operator ==(Alarm alarmX, Alarm alarmY)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(alarmX, alarmY))
            {
                return true;
            }

            // if only one is null, return false
            // the cast to object ensures that this operator overload is not called in an infinite loop
            if ((object)alarmX == null || (object)alarmY == null)
            {
                return false;
            }

            if (alarmX.Id == alarmY.Id &&
                alarmX.Info == alarmY.Info &&
                alarmX.Timestamp == alarmY.Timestamp &&
                alarmX.AlarmState == alarmY.AlarmState &&
                alarmX.Text == alarmY.Text)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Alarm alarmX, Alarm alarmY)
        {
            return !(alarmX == alarmY);
        }

        public override bool Equals(object obj)
        {
            return this == (obj as Alarm);
        }

        public override int GetHashCode()
        {
            return new { Id, Info, Timestamp, AlarmState, Text }.GetHashCode();
        }
    }
}
