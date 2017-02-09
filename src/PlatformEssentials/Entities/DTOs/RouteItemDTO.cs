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
    public class RouteItemDTO
    {
        [DataMember]
        public Int64 Id { get; set; }

        [DataMember]
        public Int32 ModuleType { get; set; }

        [DataMember]
        public Nullable<Int32> OverrideModuleType { get; set; }

        [DataMember]
        public Int32 Index { get; set; }

        [DataMember]
        public String ForceModuleInstance { get; set; }

        [DataMember]
        public Int32 ForbiddenModuleType { get; set; }

        [DataMember]
        public List<ValueDTO> ProcessSettings { get; set; }

        [DataMember]
        public List<ValueDTO> ProcessData { get; set; }

        public RouteItemDTO()
        {
        }

        public RouteItemDTO(Int64 id, Int32 moduleType, Nullable<Int32> overrideModuleType, Int32 index, String forceModuleInstance, Int32 forbiddenModuleType, List<ValueDTO> processSettings, List<ValueDTO> processData)
        {
			Id = id;
			ModuleType = moduleType;
			OverrideModuleType = overrideModuleType;
			Index = index;
			ForceModuleInstance = forceModuleInstance;
			ForbiddenModuleType = forbiddenModuleType;
			ProcessSettings = processSettings;
			ProcessData = processData;
        }
    }
}
