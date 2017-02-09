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
    public class PlatformItemDTO
    {
        [DataMember]
        public Int64 Id { get; set; }

        [DataMember]
        public Int64 ItemId { get; set; }

        [DataMember]
        public Int64 DetectedCount { get; set; }

        [DataMember]
        public Int64 DetectedInModuleCount { get; set; }

        [DataMember]
        public DateTime LastDetectionTime { get; set; }

        [DataMember]
        public String LogHistory { get; set; }

        [DataMember]
        public RouteDTO Route { get; set; }

        [DataMember]
        public PlatformItemDTO ItemBehind { get; set; }

        [DataMember]
        public PlatformItemDTO ItemInFront { get; set; }

        [DataMember]
        public PlatformModuleEntityDTO AssociatedPlatformModuleEntity { get; set; }

        public PlatformItemDTO()
        {
        }

        public PlatformItemDTO(Int64 id, Int64 itemId, Int64 detectedCount, Int64 detectedInModuleCount, DateTime lastDetectionTime, String logHistory, RouteDTO route, PlatformItemDTO itemBehind, PlatformItemDTO itemInFront, PlatformModuleEntityDTO associatedPlatformModuleEntity)
        {
			Id = id;
			ItemId = itemId;
			DetectedCount = detectedCount;
			DetectedInModuleCount = detectedInModuleCount;
			LastDetectionTime = lastDetectionTime;
			LogHistory = logHistory;
			Route = route;
			ItemBehind = itemBehind;
			ItemInFront = itemInFront;
			AssociatedPlatformModuleEntity = associatedPlatformModuleEntity;
        }
    }
}
