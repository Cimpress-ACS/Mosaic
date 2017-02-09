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

namespace VP.FF.PT.Common.PlcCommunication.Infrastructure
{
    /// <summary>
    /// Calculates the average timespan in milliseconds.
    /// </summary>
    public class AverageTimespan
    {
        private readonly uint _maxProbeCount;
        private readonly Queue<int> _timespanProbes;
        private int _timespanProbeNotifyCount;

        public class AverageTimespanEventArgs : EventArgs
        {
            public AverageTimespanEventArgs(int average)
            {
                Average = average;
            }

            public int Average { get; private set; }
        }

        public AverageTimespan(uint maxProbeCount)
        {
            _maxProbeCount = maxProbeCount;
            _timespanProbes = new Queue<int>((int)maxProbeCount);
        }

        /// <summary>
        /// Notifies new average value every maxProbeCount.
        /// </summary>
        public event EventHandler<AverageTimespanEventArgs> AverageChanged;

        private DateTime _lastTimestamp;

        public void AddTimestampProbe(DateTime timestamp)
        {
            if (_lastTimestamp == DateTime.MinValue)
            {
                _lastTimestamp = timestamp;
                return;
            }

            _timespanProbes.Enqueue((timestamp - _lastTimestamp).Milliseconds);

            if (_timespanProbes.Count > _maxProbeCount)
                _timespanProbes.Dequeue();

            if (AverageChanged != null)
            {
                _timespanProbeNotifyCount++;
                if (_timespanProbeNotifyCount >= _maxProbeCount)
                {
                    AverageChanged(this, new AverageTimespanEventArgs((int) _timespanProbes.Average()));
                    _timespanProbeNotifyCount = 0;
                }
            }

            _lastTimestamp = timestamp;
        }

        /// <summary>
        /// Calculates average timespans.
        /// </summary>
        /// <returns>Average in milliseconds.</returns>
        public uint Average()
        {
            return (uint) _timespanProbes.Average();
        }
    }
}
