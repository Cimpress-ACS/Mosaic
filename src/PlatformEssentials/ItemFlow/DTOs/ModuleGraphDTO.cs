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


using System.Collections.Generic;
using System.Runtime.Serialization;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs
{
    [DataContract]
    public class ModuleGraphDTO
    {
        public ModuleGraphDTO()
        {
            Vertices = new List<PlatformModuleDTO>();
            Edges = new List<ModuleGraphEdgeDTO>();
        }

        [DataMember]
        public IList<PlatformModuleDTO> Vertices { get; set; }

        [DataMember]
        public IList<ModuleGraphEdgeDTO> Edges { get; set; }
    }
}
