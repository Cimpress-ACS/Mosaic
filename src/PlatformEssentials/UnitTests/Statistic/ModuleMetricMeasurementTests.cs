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
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Statistic;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.Statistic
{
    [TestFixture]
    public class ModuleMetricMeasurementTests
    {
        private Mock<IPlatformModule> _module;
        private IModuleMetricMeasurement _testee;

        [SetUp]
        public void SetUp()
        {
            _module = new Mock<IPlatformModule>();

            _testee = new ModuleMetricMeasurement();
            _testee.Initialize(_module.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _testee.Dispose();
            _testee = null;
            _module = null;
        }

        [Test]
        public void WhenModuleItemChanges_ShouldRaiseNewMetricEvent()
        {
            _testee.MonitorEvents();

            _module.Raise(m => m.CurrentItemCountChangedEvent += (s, a) => {}, new ItemCountChangedEventArgs(1) );

            _testee.ShouldRaise("MetricsUpdatedEvent")
                .WithSender(_testee);
        }

        [Test]
        public void WhenModuleStateChanges_ShouldRaiseNewMetricEvent()
        {
            _testee.MonitorEvents();

            _module.Raise(m => m.ModuleStateChangedEvent += (s, a) => { }, _module.Object, PlatformModuleState.Run);

            _testee.ShouldRaise("MetricsUpdatedEvent")
                .WithSender(_testee);
        }

        [Test]
        public void ShouldRaiseNewMetricEventAfterTimeout()
        {
            bool raisedMetricEvent = false;
            _testee.MetricsUpdatedEvent += (sender, args) => raisedMetricEvent = true;
            _testee.RefreshRate = TimeSpan.FromMilliseconds(50);

            Assert.That(() => raisedMetricEvent, Is.True.After(5000));
        }

        [Test]
        public void GivenTarget10_WhenOneItemProduced_PerformanceShouldBe10Percent()
        {
            _module.SetupAllProperties();
            _module.Object.PlannedThroughput = 10;

            _module
                .Setup(m => m.CurrentItemCount)
                .Returns(1);
            _module.Raise(m => m.CurrentItemCountChangedEvent += (s, a) => { }, new ItemCountChangedEventArgs(1));

            _testee.Performance.Should().Be(0.1, "1 item out of 10 were produced the last hour");
        }

        [Test]
        public void GivenItemProduced_WhenReset_OeeShouldBeZero()
        {
            _module.SetupAllProperties();
            _module.Object.PlannedThroughput = 1;
            _module
                .Setup(m => m.CurrentItemCount)
                .Returns(1);
            _module.Raise(m => m.CurrentItemCountChangedEvent += (s, a) => { }, new ItemCountChangedEventArgs(1));

            _testee.Reset();

            _testee.OverallEquipmentEfficiency.Should().Be(0, "reset was called");
        }

        [Test]
        public void WhenProduceOverTheTop_PerformanceIs1Max()
        {
            _module.SetupAllProperties();
            _module.Object.PlannedThroughput = 10;

            _module
                .Setup(m => m.CurrentItemCount)
                .Returns(13);
            _module.Raise(m => m.CurrentItemCountChangedEvent += (s, a) => { }, new ItemCountChangedEventArgs(13));

            _testee.Performance.Should().Be(1, "performance of 1 is maxium possible");
        }

        [Test]
        public void GivenItemScanned_WhenTimePasses_ShouldHaveZeroPerformanceBecauseItsOutOfFloatingTimeWindow()
        {
            _module.SetupAllProperties();
            _module.Object.PlannedThroughput = 1;
            _testee.RefreshRate = TimeSpan.FromMilliseconds(50);
            _testee.FloatingTimeWindow = TimeSpan.FromMilliseconds(100);

            _module
                .Setup(m => m.CurrentItemCount)
                .Returns(1);
            _module.Raise(m => m.CurrentItemCountChangedEvent += (s, a) => { }, new ItemCountChangedEventArgs(1));

            Thread.Sleep(1000);

            _testee.Performance.Should().Be(0, "last tracked item out the floating time window");
        }
    }
}
