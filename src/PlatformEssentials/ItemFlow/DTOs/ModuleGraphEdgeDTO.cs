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


using System.Runtime.Serialization;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs
{
    [DataContract]
    public class ModuleGraphEdgeDTO
    {
        public ModuleGraphEdgeDTO(string id, string source, string target, int sourcePort, int targetPort, bool isForcingEnabled)
        {
            Id = id;
            Source = source;
            Target = target;
            OriginPort = sourcePort;
            TargetPort = targetPort;
            IsForcingEnabled = isForcingEnabled;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string Target { get; set; }

        [DataMember]
        public int OriginPort { get; set; }

        [DataMember]
        public int TargetPort { get; set; }

        [DataMember]
        public bool IsForcingEnabled { get; set; }
    }
}
