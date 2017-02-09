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
using System.Linq;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// A log representing PLC performance data, summarized based on a larger set of individual performance data.
    /// </summary>
    internal class PlcPerformanceData
    {
        public string Module { get; private set; }
        public long Max { get; private set; }
        public long Min { get; private set; }
        public long Count { get; private set; }
        public double Average { get; private set; }
        public double StandardDeviation { get; private set; }

        public PlcPerformanceData(string module, params long[] performanceData)
        {
            Module = module;
            Max = performanceData.Max();
            Min = performanceData.Min();
            Count = performanceData.Count();
            Average = performanceData.Average();
            StandardDeviation = Count > 1 ? Math.Sqrt(performanceData.Sum(d => (d - Average) * (d - Average)) / Count) : 0;
        }
    }
}
