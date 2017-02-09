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
    public class MetricsDTO
    {
        public MetricsDTO()
        {
        }

        public MetricsDTO(int overallItemCount, TimeSpan upTime, TimeSpan downTime, double throughput, double throughputMin = 0, double throughputMax = 1)
        {
            OverallItemCount = overallItemCount;
            UpTime = upTime;
            DownTime = downTime;
            Throughput = throughput;
            ThroughputMin = throughputMin;
            ThroughputMax = throughputMax;
        }

        [DataMember]
        public int OverallItemCount { get; set; }

        [DataMember]
        public TimeSpan UpTime { get; set; }

        [DataMember]
        public TimeSpan DownTime { get; set; }

        [DataMember]
        public double Throughput { get; set; }

        [DataMember]
        public double ThroughputMin { get; set; }

        [DataMember]
        public double ThroughputMax { get; set; }
    }
}
