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

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    public enum PlatformItemEventType
    {
        NewItemCreated,
        ItemLeft,
        ItemDetected
    }

    public class PlatformItemEvent
    {
        public PlatformItemEvent(long itemId, IPlatformModule assosiatedModule, PlatformItemEventType eventType, int releasePort = -1)
        {
            ItemId = itemId;
            AssosiatedModule = assosiatedModule;
            EventType = eventType;
            Timestamp = DateTime.Now;
            ReleasePort = releasePort;
        }

        public long ItemId { get; private set; }
        public IPlatformModule AssosiatedModule { get; internal set; }
        public PlatformItemEventType EventType { get; private set; }
        public DateTime Timestamp { get; private set; }
        public int ReleasePort { get; private set; }


        /// <summary>
        /// Optional. If new item was created, otherwise null.
        /// </summary>
        public PlatformItem NewItem { get; set; }
    }
}
