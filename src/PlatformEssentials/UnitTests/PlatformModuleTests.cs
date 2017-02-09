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
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    [TestFixture]
    [Ignore]
    public class PlatformModuleTests
    {
        [SetUp]
        public void Setup()
        {
            ILogger logger = new ConsoleOutLogger();
            var entityContextFactory = new Mock<IEntityContextFactory>();
            entityContextFactory
                .Setup(e => e.CreateContext())
                .Returns(new NoDbEntityContext());
            var dummyPlatformEntities = new Mock<IPlatformModuleEntities>();
            dummyPlatformEntities.Setup(a => a.GetAll()).Returns(new List<PlatformModuleEntity>());
            dummyPlatformEntities.Setup(a => a.GetPlatformModuleEntity(It.IsAny<PlatformModuleEntity>()))
                .Returns<PlatformModuleEntity>(b => b);

            _testee = new PlatformModule
            {
                EntityContextFactory = entityContextFactory.Object,
                PlatformModuleEntities = dummyPlatformEntities.Object,
                Logger = logger,
                CompositeAlarmManager = new CompositeAlarmManager(logger),
                Name = string.Empty
            };

            _testee.Construct();
        }

        private PlatformModule _testee;

        [Test]
        public void GivenItem_WhenRemove_ItemShouldBeRemoved()
        {
            var item = new PlatformItem();
            _testee.AddItem(item);

            _testee.RemoveItem(item.ItemId);

            _testee.ContainsItem(item.ItemId).Should().BeFalse("item has been removed");
            _testee.Entities.PlatformItems.Contains(item).Should().BeFalse("item has been removed");
            _testee.Entities.PlatformItems.Count.Should().Be(0, "there is no item in the module anymore");
        }

        [Test]
        public void GivenModuleIsAbleToRun_WhenSetStateRun_ShouldRun()
        {
            IPlatformModule module = new TestSubModule {ConditionCanGoToRun = true};
            module.State = PlatformModuleState.Off;

            module.State = PlatformModuleState.Run;

            module.State.Should().Be(PlatformModuleState.Run);
        }

        [Test]
        public void GivenModuleIsNotAbleToRun_WhenSetStateRun_ShouldNotRun()
        {
            IPlatformModule module = new TestSubModule {ConditionCanGoToRun = false};
            module.State = PlatformModuleState.Off;

            module.State = PlatformModuleState.Run;

            module.State.Should().Be(PlatformModuleState.Off);
        }

        [Test]
        public void GivenTwoLinkedItems_WhenMoveOne_ShouldReleaseLinks()
        {
            var logger = new Mock<ILogger>();
            var entityContextFactory = new EntityContextFactory(logger.Object);
            var dummy = new PlatformModule
            {
                EntityContextFactory = entityContextFactory,
                Logger = logger.Object,
                CompositeAlarmManager = new CompositeAlarmManager(logger.Object),
                Name = string.Empty
            };
            dummy.Construct();
            var item1 = new PlatformItem {ItemId = 1};
            var item2 = new PlatformItem {ItemId = 2};
            item1.ItemBehind = item2;
            item2.ItemInFront = item1;
            _testee.AddItem(item1);
            _testee.AddItem(item2);

            _testee.MoveItem(item1.ItemId, dummy);

            item1.ItemInFront.Should().BeNull();
            item1.ItemBehind.Should().BeNull();
            item2.ItemInFront.Should().BeNull();
            item2.ItemBehind.Should().BeNull();
        }

        [Test]
        public void WhenAddItem_MustContainItem()
        {
            var item = new PlatformItem();

            _testee.AddItem(item);

            _testee.ContainsItem(item.ItemId).Should().BeTrue("item was added");
            _testee.Entities.PlatformItems.Contains(item).Should().BeTrue("item was added");
        }

        [Test]
        public void WhenAddItem_MustRaiseEvent()
        {
            _testee.MonitorEvents();

            _testee.AddItem(new PlatformItem());

            _testee
                .ShouldRaise("CurrentItemCountChangedEvent")
                .WithSender(_testee);
        }

        [Test]
        public void WhenRemoveItem_MustRaiseEvent()
        {
            var item = new PlatformItem();
            _testee.AddItem(item);
            _testee.MonitorEvents();

            _testee.RemoveItem(item.ItemId);

            _testee
                .ShouldRaise("CurrentItemCountChangedEvent")
                .WithSender(_testee);
        }

        [Test]
        public void WhenStateChanges_MustRaiseEvent()
        {
            _testee.MonitorEvents();

            _testee.State = PlatformModuleState.Disabled;

            _testee.ShouldRaise("ModuleStateChangedEvent").WithSender(_testee);
        }
    }
}
