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

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    public class Alarm : IEquatable<Alarm>
    {
        public Alarm()
        {
            Message = string.Empty;
            Source = string.Empty;
            IsResettable = true;
        }

        public long Id { get; set; }
        public AlarmType Type { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public int AlarmId { get; set; }
        public AlarmSourceType SourceType { get; set; }
        public bool IsResettable { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(Alarm))
                return false;

            return Equals((Alarm) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode()*397) ^ (Source != null ? Source.GetHashCode() : 0);
            }
        }

        public bool Equals(Alarm other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return AlarmId == other.AlarmId && string.Equals(Source, other.Source);
        }

        public static bool operator ==(Alarm alarm1, Alarm alarm2)
        {
            if (ReferenceEquals(alarm1, alarm2))
                return true;
            if (ReferenceEquals(null, alarm1))
                return false;

            return alarm1.Equals(alarm2);
        }

        public static bool operator !=(Alarm alarm1, Alarm alarm2)
        {
            return !(alarm1 == alarm2);
        }
    }
}
