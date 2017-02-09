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
using VP.FF.PT.Common.PlatformEssentials;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction;

namespace VP.FF.PT.Common.Simulation.HardwareAbstraction
{
    /// <summary>
    /// This fakes a IJunction interface for mosaic, don't mismatch it with the simulated equipment.
    /// </summary>
    [Export("simulation", typeof(IJunction))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimulatedJunction : ModuleEquipment, IJunction
    {
        private readonly ILogger _logger;
        internal readonly Dictionary<long, int> Routings = new Dictionary<long, int>();

        public event EventHandler<long> ItemRoutedEvent;

        [ImportingConstructor]
        public SimulatedJunction(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            EquipmentName = GetType().Name;
        }

        public void RouteItem(long itemId, int laneIndex)
        {
            Routings.Add(itemId, laneIndex);
        }

        public void RouteItem(PlatformItem item, int laneIndex)
        {
            RouteItem(item.ItemId, laneIndex);
        }

        public void SimulateRouting(ISimulatedItem item)
        {
            long itemId = (long) item.ItemId;

            if (Routings.ContainsKey(itemId))
            {
                Routings.Remove(itemId);

                if (ItemRoutedEvent != null)
                    ItemRoutedEvent(this, itemId);
            }
            else
            {
                string moduleName = string.Empty;
                if (AssociatedModule != null)
                    moduleName = AssociatedModule.Name;

                _logger.WarnFormat("junction in module " + moduleName + " could not find routing for item-id={0}, default route port 0 will be used", itemId);

                if (ItemRoutedEvent != null)
                    ItemRoutedEvent.Invoke(this, itemId);
            }
        }
    }
}
