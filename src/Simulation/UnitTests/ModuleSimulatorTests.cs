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

namespace VP.FF.PT.Common.Simulation.UnitTests
{
    [TestFixture]
    public class ModuleSimulatorTests
    {
        private ModuleSimulator _testee;

        [SetUp]
        public void SetUp()
        {
            var equipmentRepository = new EquipmentRepository();

            _testee = new ModuleSimulator(new Mock<ILogger>().Object, equipmentRepository);
            _testee.Initialize(5, "test");
        }

        [Test]
        public void WhenAddItems_ShouldBeOnRightPosition()
        {
            _testee.AddItem(new SimulatedItem());
            _testee.AddItem(new SimulatedItem(), 2);

            _testee.GetItemByPosition(0).Should().NotBeNull();
            _testee.GetItemByPosition(1).Should().BeNull();
            _testee.GetItemByPosition(2).Should().NotBeNull();
        }

        [Test]
        public void WhenAddItem_ShouldBeAccessible()
        {
            var item1 = new SimulatedItem { ItemId = 1 };
            var item2 = new SimulatedItem { ItemId = 2 };

            _testee.AddItem(item1);
            _testee.AddItem(item2);

            _testee.GetItemById(1).Should().Be(item1);
            _testee.GetItemById(2).Should().Be(item2);
            _testee.GetItemById(99).Should().BeNull("item does not exist");
        }

        [Test]
        public void GivenItem_WhenSearchForPosition_ShouldReturn()
        {
            var item = new SimulatedItem { ItemId = 1 };
            _testee.AddItem(item, 3);

            _testee.GetPositionOfItem(item.ItemId).Should().Be(3);            
        }

        [Test]
        public void GivenNoItem_WhenSearchForPosition_ShouldReturnNegative()
        {
            _testee.GetPositionOfItem(999).Should().Be(-1);
        }

        [Test]
        public void WhenAddItemToInvalidIndex_ShouldThrow()
        {
            var action1 = new Action(() => _testee.AddItem(new SimulatedItem(), -1));
            var action2 = new Action(() => _testee.AddItem(new SimulatedItem(), 99));

            action1.ShouldThrow<ArgumentOutOfRangeException>();
            action2.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void GivenSlotOccupied_WhenAddItem_ShouldAddAlarm()
        {
            Alarm alarm = null;
            _testee.Alarms.Subscribe(a => alarm = a);

            _testee.AddItem(new SimulatedItem());
            _testee.AddItem(new SimulatedItem());

            alarm.Should().NotBeNull();
            alarm.Source.Should().Be(_testee);
            alarm.Type.Should().Be(AlarmType.Warning);
        }

        [Test]
        public void GivenEquipmentOnPosition1_WhenItemPassed_ShouldCallCustomAction()
        {
            var item = new SimulatedItem { ItemId = 1 };
            _testee.AddItem(item);

            bool customActionWasCalled = false;
            var customActionEquipment = new CustomActionEquipment();
            customActionEquipment.Initialize(1, i =>
            {
                customActionWasCalled = true;
            });
            _testee.AddEquipment(customActionEquipment);

            customActionWasCalled.Should().BeFalse("item is still on position 0");

            _testee.Takt();

            customActionWasCalled.Should().BeTrue("item just passed equipment on position 1");
        }

        [Test]
        public void WhenItemPassesEquipment_ShouldAddLogHistory()
        {
            var item = new SimulatedItem();
            _testee.AddItem(item);
            var customActionEquipment = new CustomActionEquipment();
            customActionEquipment.Initialize(1, i => {});
            _testee.AddEquipment(customActionEquipment);

            _testee.Takt();

            item.LogHistory.Should().Contain("passed equipment CustomActionEquipment");
        }

        [Test]
        public void GivenModuleStopped_WhenTakt_ShouldNotMoveItems()
        {
            var item = new SimulatedItem { ItemId = 1 };
            _testee.AddItem(item);
            _testee.IsActive = false;

            _testee.Takt();

            _testee.GetItemByPosition(0).Should().NotBeNull("item is still on first slot because module is not active");
        }

        [Test]
        public void GivenItemFrozen_WhenTakt_ShouldNotMove()
        {
            var item = new SimulatedItem { ItemId = 1 };
            item.IsFrozen = true;
            _testee.AddItem(item);

            _testee.Takt();

            _testee.GetItemByPosition(0).Should().NotBeNull("item is still on first slot because module is not active");
        }

        [Test]
        public void GivenItemAndItemFrozen_WhenTakt_ShouldNotMoveFrozenItemOnly()
        {
            var item1 = new SimulatedItem { ItemId = 1 };
            var item2 = new SimulatedItem { ItemId = 2 };
            item1.IsFrozen = true;
            _testee.AddItem(item1, 0);
            _testee.AddItem(item2, 1);

            _testee.Takt();

            _testee.GetPositionOfItem(item1.ItemId).Should().Be(0, "this item must not move because it's frozen");
            _testee.GetPositionOfItem(item2.ItemId).Should().Be(2, "this item must move because it's not frozen");
        }

        [Test]
        public void WhenAddEquipmentToWrongSlot_ShouldThrowException()
        {
            var equipment1 = new CustomActionEquipment();
            var equipment2 = new CustomActionEquipment();
            equipment1.Initialize(-1, a => {});
            equipment2.Initialize(999999, a => { });

            var action1 = new Action(() => _testee.AddEquipment(equipment1));
            var action2 = new Action(() => _testee.AddEquipment(equipment2));

            action1.ShouldThrow<SimulationException>("equipment position must be >= 0");
            action2.ShouldThrow<SimulationException>("equipment position must not exceed length of ModuleSimulator");
        }

        [Test]
        public void GivenEquipmentAlreadyAtModule_WhenAddEquipment_ShouldThrowException()
        {
            var equipment = new CustomActionEquipment();
            equipment.Initialize(0, a => {});
            _testee.AddEquipment(equipment);

            var action = new Action(() => _testee.AddEquipment(equipment));

            action.ShouldThrow<SimulationException>();
        }
    }
}
