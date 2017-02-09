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
using System.ComponentModel.Composition;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction
{
    /// <summary>
    /// Dummy implementation which can be replaced by some real hardware junctions, AGV, etc. in future or by a screen telling an operator where to route it to.
    /// </summary>
    [Export(typeof(IJunction))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GenericJunction : ModuleEquipment, IJunction
    {
        private readonly ILogger _logger;

        public event EventHandler<long> ItemRoutedEvent;

        [ImportingConstructor]
        public GenericJunction(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            EquipmentName = GetType().Name;
        }

        public readonly Dictionary<long, int> Routings = new Dictionary<long, int>();

        public void RouteItem(long itemId, int laneIndex)
        {
            Routings.Add(itemId, laneIndex);
        }

        public void RouteItem(PlatformItem item, int laneIndex)
        {
            RouteItem(item.ItemId, laneIndex);
        }
    }
}
