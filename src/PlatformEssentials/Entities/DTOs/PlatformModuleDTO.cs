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
using System.Runtime.Serialization;

namespace VP.FF.PT.Common.PlatformEssentials.Entities.DTOs
{
    [DataContract]
    public class PlatformModuleDTO
    {
        [DataMember]
        public Int64 Id { get; set; }

        [DataMember]
        public PlatformModuleState State { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public Boolean IsEnabled { get; set; }

        [DataMember]
        public Int32 AdsPort { get; set; }

        [DataMember]
        public String PathRootController { get; set; }

        [DataMember]
        public Int32 MaxCapacity { get; set; }

        [DataMember]
        public String SubState { get; set; }

        [DataMember]
        public Int32 StreamType { get; set; }

        [DataMember]
        public List<PlatformItemDTO> PlatformItems { get; set; }

        [DataMember]
        public Int32 Type { get; set; }

        [DataMember]
        public Boolean HasWarnings { get; set; }

        [DataMember]
        public Boolean HasErrors { get; set; }

        [DataMember]
        public String MostImportantAlarmText { get; set; }

        public PlatformModuleDTO()
        {
        }

        public PlatformModuleDTO(Int64 id, PlatformModuleState state, String name, Boolean isEnabled, Int32 adsPort, String pathRootController, Int32 maxCapacity, String subState, List<PlatformItemDTO> platformItems, Int32 type)
        {
            Id = id;
            State = state;
            Name = name;
            IsEnabled = isEnabled;
            AdsPort = adsPort;
            PathRootController = pathRootController;
            MaxCapacity = maxCapacity;
            SubState = subState;
            PlatformItems = platformItems;
            Type = type;
        }
    }
}
