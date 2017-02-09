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


using System.Collections.Generic;
using System.ComponentModel.Composition;
using VP.FF.PT.Common.Infrastructure.ItemTracking;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    /// <summary>
    /// Simple PlatformModule mock provides access to internal via "Simulate"-methods.
    /// Is should behave like a real module so that we can run a realistic test for ModuleBusManager.
    /// </summary>
    [Export]
    public class ModuleMock : PlatformModule
    {
        private readonly HashSet<int> _isFull = new HashSet<int>();

        private readonly MockItemRouter _itemRouter;

        public ModuleMock()
        {
            MaxCapacity = 10;
            StreamType = 1;
            _itemRouter = new MockItemRouter();
        }

        public override void Construct()
        {
            base.Construct();
            State = PlatformModuleState.NotInitialized;
        }

        public override void Start()
        {
            State = PlatformModuleState.Run;
            RaiseModuleStateChangedEvent();
        }

        public override void Stop()
        {
            State = PlatformModuleState.Off;
            RaiseModuleStateChangedEvent();
        }

        public override bool IsFull(int inputPortIndex)
        {
            if (_isFull.Contains(inputPortIndex))
                return true;

            return false;
        }

        public void SimulateIsFull(int port)
        {
            _isFull.Add(port);
            RaiseModuleStateChangedEvent();
        }

        public void SimulateIsNotFull(int port)
        {
            _isFull.Remove(port);
            RaiseModuleStateChangedEvent();
        }

        public void SimulateEmergencyOff()
        {
            State = PlatformModuleState.Error;
        }

        public void SimulateNewItemCreated(PlatformItem item)
        {
            EventAggregator.Publish(new PlatformItemEvent(item.ItemId, this, PlatformItemEventType.NewItemCreated) { NewItem = item });
        }

        public void SimulateItemDetected(PlatformItem item)
        {
            RaisePlatformItemDetected(item.ItemId);
        }

        public void SimulateItemReleased(PlatformItem item, int outputPort)
        {
            RaisePlatformItemReleased(item.ItemId, outputPort);
        }

        protected override void MoveItem(PlatformItem item, IPlatformModule targetModule)
        {
            base.MoveItem(item, targetModule);
            _itemRouter.Cancel(item);
        }

        protected override void RemoveItem(PlatformItem item)
        {
            base.RemoveItem(item);
            _itemRouter.Cancel(item);
        }

        public override void AddItemRouting(PlatformItem item, int outputPortIndex)
        {
            base.AddItemRouting(item, outputPortIndex);
            _itemRouter.Route(item, outputPortIndex);
        }

        public override void RemoveItemRouting(PlatformItem item)
        {
            base.RemoveItemRouting(item);
            _itemRouter.Cancel(item);
        }

        /// <summary>
        /// Make current routings public for testing
        /// </summary>
        public HashSet<int> TestCurrentAllPortRoutings
        {
            get
            {
                return PortRoutings;
            }
        }

        public IDictionary<PlatformItem, int> TestCurrentItemRoutings
        {
            get
            {
                return _itemRouter.Items;
            }
        }

        public class MockItemRouter : UniqueItemRouter<PlatformItem>
        {
            public IDictionary<PlatformItem, int> Items
            {
                get { return CurrentItemRoutings; }
            }
        }
    }
}
