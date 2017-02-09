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


using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    public class DownstreamUnavailableAlarmsTests
    {
        [Test]
        public void WhenGetAlarmTwice_ShouldReturnSameAlarmObject()
        {
            var module = new Mock<IPlatformModule>().Object;
            var moduleBusManagerMock = new Mock<IModuleBusManager>();
            var testee = new DownstreamUnavailableAlarms(moduleBusManagerMock.Object);

            var alarm = testee.GetAlarm(module);
            var alarm2 = testee.GetAlarm(module);

            alarm2.Should().BeSameAs(alarm);
        }

        [Test]
        public void WhenProvidingCustomMessage_AlarmShouldHaveCustomMessage()
        {
            const string message = "custom test message";
            var module = new Mock<IPlatformModule>().Object;
            var moduleBusManagerMock = new Mock<IModuleBusManager>();
            var testee = new DownstreamUnavailableAlarms(moduleBusManagerMock.Object);

            var alarm = testee.GetAlarm(module, () => message);

            alarm.Message.Should().Be(message);
        }
    }
}
