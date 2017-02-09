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
using System.Threading;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    public class AlarmArrayList : List<PlcAlarmStruct>
    {
        /// <summary>
        /// Ensure that each instance of <see cref="AlarmArrayList"/> has a unique <see cref="_id"/> in order
        /// to force the extension method <see cref="ObjectExtensions.CompareObjectsOnlyFields"/> always to return FALSE
        /// </summary>
        private readonly long _id;

        /// <summary>
        /// Global counter that's used to assign a unique value for each array list.
        /// </summary>
        private static long _globalCount;

        internal long Id { get { return _id; }}

        public AlarmArrayList()
        {
            _id = Interlocked.Increment(ref _globalCount);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AlarmArrayList);
        }

        protected bool Equals(AlarmArrayList other)
        {
            if (other == null)
            {
                return false;
            }

            // sequence equals of converted items
            return this.Select(Alarm.CopyPlcAlarmToAlarm).SequenceEqual(other.Select(Alarm.CopyPlcAlarmToAlarm));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 19;
                foreach (var alarm in this)
                {
                    hash = hash*31 + Alarm.CopyPlcAlarmToAlarm(alarm).GetHashCode();
                }
                return hash;
            }
        }
    }
}
