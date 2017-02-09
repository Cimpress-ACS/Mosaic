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


using System.ComponentModel.Composition;
using System.Linq;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    /// <summary>
    /// Simple PlatformModule mock provides access to internal via "Simulate"-methods.
    /// Is should behave like a real module so that we can run a realistic test for ModuleBusManager.
    /// </summary>
    [Export]
    public class ModuleMockWithFlowBehavior : ModuleMock
    {
        public ModuleMockWithFlowBehavior()
        {
            SimulateRouteImmediately = true;
            StreamType = 1;
        }

        public void SetUp(int maxCapacity, int currentItemCount, int limit = -1)
        {
            Assert.That(currentItemCount <= maxCapacity);

            LimitItemCount = limit;
            MaxCapacity = maxCapacity;

            for (int i = 1; i <= currentItemCount; i++)
            {
                Entities.PlatformItems.Add(new PlatformItem
                {
                    Id = i * -1,
                    ItemId = i * -1
                });
            }
        }

        public bool SimulateRouteImmediately { get; set; }

        public override void AddItemRouting(PlatformItem item, int outputPortIndex)
        {
            base.AddItemRouting(item, outputPortIndex);

            if (SimulateRouteImmediately)
            {
                if (Entities.PlatformItems.Contains(item))
                {
                    RaisePlatformItemReleased(item.ItemId, outputPortIndex);
                }
            }
        }

        public override void AddItem(PlatformItem item)
        {
            base.AddItem(item);

            if (SimulateRouteImmediately)
            {
                if (PortRoutings.Any())
                    RaisePlatformItemReleased(item.ItemId, PortRoutings.First());
            }
        }
    }
}
