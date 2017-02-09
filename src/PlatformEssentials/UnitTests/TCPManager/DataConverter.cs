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
using VP.FF.PT.Common.PlatformEssentials.TCPManager;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.TCPManager
{
    public class DataConverter : IDataConverter<int>
    {
        private readonly Queue<byte> _bytes = new Queue<byte>();

        public List<int> Parse(byte[] newRawData)
        {
            foreach (var x in newRawData)
            {
                _bytes.Enqueue(x);
            }

            var items = new List<int>();
            while (_bytes.Count >= 4)
            {
                var chunk = new[] {_bytes.Dequeue(), _bytes.Dequeue(), _bytes.Dequeue(), _bytes.Dequeue()};
                items.Add(BitConverter.ToInt32(chunk, 0));
            }
            return items;
        }

        public byte[] GetBytes(int item)
        {
            return BitConverter.GetBytes(item);
        }

        public void Init()
        {
            
        }
    }
}
