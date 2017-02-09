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
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction
{
    public interface IJunction : IModuleEquipment
    {
        /// <summary>
        /// Occurs when an item got routed to specific lane.
        /// </summary>
        event EventHandler<long> ItemRoutedEvent;

        /// <summary>
        /// Instructs the junction to route an upcoming item to a specific lane.
        /// </summary>
        /// <param name="itemId">The unique item identifier.</param>
        /// <param name="laneIndex">Index of the lane.</param>
        void RouteItem(long itemId, int laneIndex);

        /// <summary>
        /// Instructs the junction to route an upcoming item to a specific lane.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="laneIndex">Index of the lane.</param>
        void RouteItem(PlatformItem item, int laneIndex);
    }
}
