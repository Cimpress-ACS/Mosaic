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
using VP.FF.PT.Common.Infrastructure;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    [Serializable]
    public class PlatformItem : Item, ILinkedItem<PlatformItem>
    {
        public PlatformItem()
        {
            LastDetectionTime = DateTime.MinValue;
        }

        public bool Equals(PlatformItem other)
        {
            if (other == null)
                return false;

            return ItemId == other.ItemId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlatformItem);
        }

        public override int GetHashCode()
        {
            return (int)ItemId;
        }

        public void AddLog(string text, string moduleName = "")
        {
            LogHistory += String.Format("{0} - {1} - {2}\n", DateTime.Now, moduleName, text);
        }

        public long Id { get; set; }
        /// <summary>
        /// This is the hashed item id which is used for faster processing
        /// </summary>
        public long ItemId { get; set; }
        /// <summary>
        /// This is the real id as a string.
        /// </summary>
        public string RawItemId { get; set; }
        public long DetectedCount { get; set; }
        public long DetectedInModuleCount { get; set; }
        public DateTime LastDetectionTime { get; set; }
        public string LogHistory { get; set; }
    
        public virtual Route Route { get; set; }

        public virtual PlatformItem ItemBehind { get; set; }

        public virtual PlatformItem ItemInFront { get; set; }

        public virtual PlatformModuleEntity AssociatedPlatformModuleEntity { get; set; }
    }
}

