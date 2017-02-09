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
using System.Diagnostics;
using VP.FF.PT.Common.Infrastructure;

namespace VP.FF.PT.Common.Simulation
{
    [Serializable]
    [DebuggerDisplay("ItemId:{ItemId}")]
    public class SimulatedItem : ISimulatedItem
    {
        private static ulong _lastItemId;
        private bool _isFrozen;

        public SimulatedItem()
        {
            ItemId = ++_lastItemId;
            Metadata = new Dictionary<string, object>();
        }

        public string LogHistory { get; private set; }

        public ulong ItemId { get; set; }

        public void AddLog(string message, string moduleName = "")
        {
            LogHistory += DateTime.Now + " - " + moduleName + " - " + message + Environment.NewLine;
        }

        public bool IsFrozen 
        {
            get
            {
                return _isFrozen;
            }
            set
            {
                if (value != _isFrozen)
                {
                    _isFrozen = value;

                    if (_isFrozen)
                    {
                        AddLog("item is now frozen and will not move");
                    }
                    else
                    {
                        AddLog("item is not frozen anymore");
                    }
                }
            }
        }

        public IDictionary<string, object> Metadata { get; private set; }

        public bool Equals(ISimulatedItem other)
        {
            if (other == null)
            {
                return false;
            }

            return ItemId == other.ItemId;
        }
    }
}
