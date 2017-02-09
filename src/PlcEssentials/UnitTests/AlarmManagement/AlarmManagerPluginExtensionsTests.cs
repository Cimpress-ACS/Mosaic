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
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlcEssentials.AlarmManagement;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    public class AlarmManagerPluginExtensionsTests
    {
        [Test]
        public void GivenAlarmPlugin_WhenConvertingAlarm_ProducesCorrectResult()
        {
            // setup
            var alarmManager = new Mock<IManageCurrentAlarmsPlugin>().Object;
            var plcAlarm = GetMockAlarm();
            var source = "Unit test " + Guid.NewGuid();

            // execute
            var convertedAlarm = alarmManager.CopyPlcAlarmToAlarm(plcAlarm, source);

            // validate
            ValidateConvertedAlarm(convertedAlarm, plcAlarm, source);
        }

        private static void ValidateConvertedAlarm(Alarm convertedAlarm, IAlarm plcAlarm, string source)
        {
            convertedAlarm.AlarmId.Should().Be((int)plcAlarm.Id);
            convertedAlarm.Type.Should().Be(plcAlarm.AlarmType.ToEntity());
            convertedAlarm.Source.Should().Be(source);
            convertedAlarm.SourceType.Should().Be(AlarmSourceType.Plc);
            convertedAlarm.Timestamp.Should().Be(plcAlarm.Timestamp);
            convertedAlarm.Message.Should().Be(plcAlarm.Text);
        }

        private static IAlarm GetMockAlarm()
        {
            var r = new Random();

            var mockAlarm = new Mock<IAlarm>(MockBehavior.Strict);
            mockAlarm.SetupGet(a => a.Id).Returns((uint)r.Next(0, 99999));
            mockAlarm.SetupGet(a => a.AlarmType).Returns(Common.PlcEssentials.AlarmType.Info);
            mockAlarm.SetupGet(a => a.Text).Returns("Mock message " + Guid.NewGuid());
            mockAlarm.SetupGet(a => a.Timestamp).Returns(DateTime.Now);
            var plcAlarm = mockAlarm.Object;
            return plcAlarm;
        }
    }
}
