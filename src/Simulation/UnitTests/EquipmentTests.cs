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
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.Simulation.Alarms;
using VP.FF.PT.Common.Simulation.Equipment;
using VP.FF.PT.Common.Simulation.HardwareAbstraction;

namespace VP.FF.PT.Common.Simulation.UnitTests
{
    [TestFixture]
    public class EquipmentTests
    {
        private ModuleSimulator _module;

        [SetUp]
        public void SetUp()
        {
            _module = new ModuleSimulator(new Mock<ILogger>().Object, new Mock<IEquipmentRepository>().Object);
            _module.TaktPartsRepository = new Mock<ITaktPartsRepository>().Object;
            _module.Initialize(2, "test");
        }

        [Test]
        public void CollisionWatcherTest()
        {
            var item = new SimulatedItem();
            var testee = new CollisionWatcherEquipment();
            Alarm alarm = null;
            testee.Alarms.Subscribe(a => alarm = a);

            testee.ItemPassed(item);

            testee.ItemPassedCount.Should().Be(1);
            alarm.Should().NotBeNull("each collision must raise an alarm");
            testee.DetectedItems.Should().Contain(item);
            item.LogHistory.Should().Contain("collision detected");
        }

        [Test]
        public void QueueEquipmentTests()
        {
            var testee = new QueueEquipment();
            testee.Initialize(0, _module);
            testee.Capacity = 5;
            _module.AddEquipment(testee);
            var item1 = new SimulatedItem {ItemId = 1};
            var item2 = new SimulatedItem {ItemId = 2};
            var item3 = new SimulatedItem {ItemId = 3};
            
            // add 3 items to slot 0
            _module.AddItem(item1);
            _module.AddItem(item2);
            _module.AddItem(item3);

            testee.ItemPassedCount.Should().Be(3);
            testee.QueueList.Items.Should().HaveCount(3);
            _module.GetItemByPosition(0).Should().BeOfType(typeof (SimulatedItemList));

            // release 1 item (move to slot 1)
            testee.ReleaseItem();

            testee.QueueList.Items.Should().HaveCount(2);
            _module.GetItemByPosition(1).Should().Be(item1, "item1 was added to the queue first and now released");

            // relase all other items (slot 0 is empty now)
            testee.ReleaseItem();
            testee.ReleaseItem();

            testee.QueueList.Items.Should().HaveCount(0);
            _module.GetItemByPosition(0).Should().BeNull("there is not item in the queue anymore");

            // further release must do nothing
            var action = new Action(testee.ReleaseItem);
            action.ShouldNotThrow("there is no more item to release");
        }

        [Test]
        public void QueueOfQueueEquipmentMustNotMove()
        {
            var testee = new QueueEquipment();
            testee.Initialize(0, _module);
            _module.AddEquipment(testee);

            _module.AddItem(new SimulatedItem());
            _module.Takt();

            testee.ItemPassedCount.Should().Be(1);
            _module.GetItemByPosition(0).Should().NotBeNull();
            _module.GetItemByPosition(1).Should().BeNull();
        }

        [Test]
        public void GivenNotActive_QueueMustNotRelease()
        {
            var testee = new QueueEquipment();
            testee.Initialize(0, _module);
            _module.AddEquipment(testee);

            _module.AddItem(new SimulatedItem());
            _module.Takt();

            testee.IsActive = false;

            testee.ReleaseItem();

            testee.QueueList.Items.Should().HaveCount(1);
        }

        [Test]
        public void GivenNotActive_StackMustNotRelease()
        {
            var testee = new StackerEquipment();
            testee.Initialize(0, _module);
            _module.AddEquipment(testee);

            _module.AddItem(new SimulatedItem());
            _module.Takt();

            testee.IsActive = false;

            testee.ReleaseStack();

            testee.StackList.Items.Should().HaveCount(1);
        }

        [Test]
        public void StackerEquipmentTests()
        {
            var testee = new StackerEquipment();
            testee.Initialize(0, _module);
            testee.Capacity = 5;
            _module.AddEquipment(testee);
            var item1 = new SimulatedItem { ItemId = 1 };
            var item2 = new SimulatedItem { ItemId = 2 };

            // add 2 items to slot 0
            _module.AddItem(item1);
            _module.AddItem(item2);

            testee.ItemPassedCount.Should().Be(2);
            testee.StackList.Items.Should().HaveCount(2);

            // release full stack (move to slot 1)
            testee.ReleaseStack();

            testee.StackList.Items.Should().HaveCount(0, "stack was released");
            _module.GetItemByPosition(0).Should().BeNull("there is no item in the stacker anymore");
            var stack = _module.GetItemByPosition(1);
            stack.Should().BeOfType<SimulatedItemList>("the stack is not on slot 1");
            stack.ItemId.Should().Be(item2.ItemId, "item2 was added last on top of the stack");
            ((SimulatedItemList) stack).Items.Should().HaveCount(2);
            ((SimulatedItemList) stack).Items.Should().Contain(item1);
            ((SimulatedItemList) stack).Items.Should().Contain(item2);

            // further release must do nothing
            var action = new Action(testee.ReleaseStack);
            action.ShouldNotThrow("there is no more item to release");
        }

        [Test]
        public void RandomItemCreatorEquipmentTests()
        {
            var testee = new RandomItemCreatorEquipment();
            testee.Initialize(0, _module, 1);
            testee.MonitorEvents();
            _module.AddEquipment(testee);

            // if module is not active it must not create items
            _module.IsActive = false;
            testee.Takt();
            testee.ItemPassedCount.Should().Be(0);
            _module.ItemCount.Should().Be(0);

            _module.IsActive = true;
            testee.Takt();
            testee.ItemPassedCount.Should().Be(1);
            testee.ShouldRaise("ItemCreated")
                .WithSender(testee)
                .WithArgs<ItemCreatorEventArgs>(a => a.CreatedItem.Metadata.ContainsKey("barcode"));
            _module.ItemCount.Should().Be(1);
            _module.GetItemByPosition(0).Metadata.Should().ContainKey("barcode");
        }

        [Test]
        public void RemoveItemCreatorEquipmentTests()
        {
            var testee = new RandomItemCreatorEquipment();
            testee.Initialize(0, _module, 1);
            _module.AddEquipment(testee);

            _module.RemoveEquipment(testee);
            testee.Takt();

            _module.ItemCount.Should().Be(0);
            testee.CreatedItemCount.Should().Be(0);
        }

        [Test]
        public void DistributionJunctionEquipment_SortOutEverySecondTests()
        {
            var targetModule = new ModuleSimulator(new Mock<ILogger>().Object, new Mock<IEquipmentRepository>().Object);
            targetModule.Initialize(10, "test2");
            var testee = new DistributionJunctionEquipment();
            testee.Initialize(0, _module, targetModule, JunctionDistributionMode.SortOutEverySecond);
            _module.Initialize(10, "test");
            _module.AddEquipment(testee);
            
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());

            _module.ItemCount.Should().Be(2);
            targetModule.ItemCount.Should().Be(2);
        }

        [Test]
        public void DistributionJunctionEquipment_SortOutEverythingTests()
        {
            var targetModule = new ModuleSimulator(new Mock<ILogger>().Object, new Mock<IEquipmentRepository>().Object);
            targetModule.Initialize(10, "test2");
            var testee = new DistributionJunctionEquipment();
            testee.Initialize(0, _module, targetModule, JunctionDistributionMode.SortOutEverything);
            _module.Initialize(10, "test");
            _module.AddEquipment(testee);

            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());

            _module.ItemCount.Should().Be(0);
            targetModule.ItemCount.Should().Be(4);
        }

        [Test]
        public void DistributionJunctionEquipment_SortOutNothingTests()
        {
            var targetModule = new ModuleSimulator(new Mock<ILogger>().Object, new Mock<IEquipmentRepository>().Object);
            targetModule.Initialize(10, "test2");
            var testee = new DistributionJunctionEquipment();
            testee.Initialize(0, _module, targetModule, JunctionDistributionMode.SortOutNothing);
            _module.Initialize(10, "test");
            _module.AddEquipment(testee);

            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());
            _module.AddItem(new SimulatedItem());

            _module.ItemCount.Should().Be(4);
            targetModule.ItemCount.Should().Be(0);
        }

        [Test]
        public void MosaicControlledJunctionEquipmentTests()
        {
            var targetModule1 = new ModuleSimulator(new Mock<ILogger>().Object, new Mock<IEquipmentRepository>().Object);
            var targetModule2 = new ModuleSimulator(new Mock<ILogger>().Object, new Mock<IEquipmentRepository>().Object);
            targetModule1.Initialize(10, "test1");
            targetModule2.Initialize(10, "test2");

            var mosaicControlledJunction = new SimulatedJunction(new Mock<ILogger>().Object);

            var testee = new JunctionEquipment();
            testee.Initialize(0, _module, mosaicControlledJunction, targetModule1, targetModule2);

            _module.Initialize(10, "test");
            _module.AddEquipment(testee);

            // route item-100 to lane 0
            mosaicControlledJunction.RouteItem(100, 0);
            _module.AddItem(new SimulatedItem { ItemId = 100 });
            _module.ItemCount.Should().Be(0);
            targetModule1.ItemCount.Should().Be(1);

            // route item-200 to lane 1
            mosaicControlledJunction.RouteItem(200, 1);
            _module.AddItem(new SimulatedItem { ItemId = 200 });
            _module.ItemCount.Should().Be(0);
            targetModule2.ItemCount.Should().Be(1);

            // dont specify a route, should go to next module
            _module.AddItem(new SimulatedItem { ItemId = 999 });
            _module.ItemCount.Should().Be(1);
            _module.Takt();
            _module.ItemCount.Should().Be(1);
        }

        [Test]
        public void MosaicBarcodeReaderEquipmentTests()
        {
            int barcodeReadCount = 0;
            var item = new SimulatedItem();
            var mosaicBarcodeReader = new SimulatedBarcodeReader(new Mock<ILogger>().Object);
            mosaicBarcodeReader.BarcodeReceivedEvent += (sender, args) => barcodeReadCount++;
            var testee = new BarcodeReaderEquipment();
            testee.Initialize(0, mosaicBarcodeReader);
            
            testee.ItemPassed(item);

            testee.ItemPassedCount.Should().Be(1);
            barcodeReadCount.Should().Be(1);
        }
    }
}
