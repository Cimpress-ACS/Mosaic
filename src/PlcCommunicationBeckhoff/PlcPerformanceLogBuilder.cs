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
using VP.FF.PT.Common.Infrastructure.Logging.Structured;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// Responsible for buffering logs, and eventually logging a single performance log with summary information.
    /// </summary>
    internal class PlcPerformanceLogBuilder
    {
        private readonly string _identifier;
        private const int Buffer = 100;
        private readonly PerformanceLogger<PlcPerformanceData> _logger;
        private List<long> _logs;

        public PlcPerformanceLogBuilder(string identifier)
        {
            _identifier = identifier;

            _logger = new PerformanceLogger<PlcPerformanceData>("PlcPerformance");

            _logs = new List<long>(Buffer);
        }

        public void AddPerformanceData(long elapsedMilliseconds)
        {
            _logs.Add(elapsedMilliseconds);

            LogWhenBufferFull();
        }

        private void LogWhenBufferFull()
        {
            if (_logs.Count >= Buffer)
            {
                _logger.Log(new PlcPerformanceData(_identifier, _logs.ToArray()));
                _logs = new List<long>(Buffer);
            }
        }
    }
}
