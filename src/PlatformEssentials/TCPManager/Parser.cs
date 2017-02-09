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
using System.Collections.Concurrent;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.TCPManager
{
    public class Parser<TData>
    {
        private readonly ConcurrentQueue<byte> _queueReceiveRawData;
        private readonly ConcurrentQueue<TData> _queueReceive;
        private readonly IDataConverter<TData> _converter;
        private readonly ILogger _logger;

        public Parser(ILogger logger, IDataConverter<TData> converter)
        {
            _logger = logger;
            _converter = converter;
            _queueReceiveRawData = new ConcurrentQueue<byte>();
            _queueReceive = new ConcurrentQueue<TData>();
            _converter.Init();
        }

        /// <summary>
        /// This method creates a byte-Array of all received raw data
        /// and passes this byte-Array to the "Parse"-Method.
        /// </summary>
        public void Parse()
        {
            if (_queueReceiveRawData.Count == 0)
            {
                return;
            }

            try
            {
                byte[] newRawData = _queueReceiveRawData.ToArray();

                _queueReceiveRawData.Clear();

                var items = _converter.Parse(newRawData);
                foreach (var item in items)
                {
                    _queueReceive.Enqueue(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Parse failed", ex);
            }
        }

        public void AddData(byte data)
        {
            _queueReceiveRawData.Enqueue(data);
        }

        public bool Dequeue(out TData item)
        {
            return _queueReceive.TryDequeue(out item);
        }
    }
}
