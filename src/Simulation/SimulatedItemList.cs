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

namespace VP.FF.PT.Common.Simulation
{
    [Serializable]
    [DebuggerDisplay("ItemId:{ItemId} Count:{Items.Count}")]
    public class SimulatedItemList : ISimulatedItem
    {
        private static ulong _lastItemId;
        private bool _isFrozen;

        public SimulatedItemList()
        {
            ItemId = ++_lastItemId;
            Items = new List<ISimulatedItem>();
            Metadata = new Dictionary<string, object>();
        }

        public IList<ISimulatedItem> Items { get; private set; }
        
        public bool Equals(ISimulatedItem other)
        {
            var otherItemList = other as SimulatedItemList;
            if (otherItemList == null)
            {
                return false;
            }

            if (otherItemList.Items.Count != Items.Count)
            {
                return false;
            }

            for(int i = 0; i < Items.Count; i++)
            {
                if (!otherItemList.Items[i].Equals(Items[i]))
                {
                    return false;
                }
            }

            return true;
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
                _isFrozen = value;

                if (value != _isFrozen)
                {
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
    }
}
