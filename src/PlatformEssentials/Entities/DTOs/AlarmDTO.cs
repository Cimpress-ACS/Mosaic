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
using System.Runtime.Serialization;

namespace VP.FF.PT.Common.PlatformEssentials.Entities.DTOs
{
    [DataContract]
    public class AlarmDTO
    {
        [DataMember]
        public Int64 Id { get; set; }

        [DataMember]
        public AlarmType Type { get; set; }

        [DataMember]
        public String Message { get; set; }

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public String Source { get; set; }

        public AlarmDTO()
        {
        }

        public AlarmDTO(Int64 id, AlarmType type, String message, DateTime timestamp, String source)
        {
			Id = id;
			Type = type;
			Message = message;
			Timestamp = timestamp;
			Source = source;
        }
    }
}
