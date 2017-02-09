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
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    class AlarmTests
    {
        [Test]
        public void GivenAlarm_ComparedWithItself_ShouldBeEqual()
        {
            var alarm = new Alarm();
            var result = alarm == alarm;
            alarm.ShouldBeEquivalentTo(alarm);
            result.Should().BeTrue();
        }

        [Test]
        public void GivenDuplicateAlarms_Compare_ShouldBeEqual()
        {
            var alarm1 = new Alarm { Source = "alarmSource", AlarmId = 777 };
            var alarm2 = new Alarm { Source = "alarmSource", AlarmId = 777 };

            var result = alarm1 == alarm2;

            alarm1.ShouldBeEquivalentTo(alarm2);
            result.Should().BeTrue();
        }

        [Test]
        public void GivenDifferentAlarms_Compare_ShouldNotBeEqual()
        {
            var alarm1 = new Alarm { Source = "alarmSource", AlarmId = 666 };
            var alarm2 = new Alarm { Source = "alarmSource", AlarmId = 777 };

            var result = alarm1 != alarm2;

            alarm1.Should().NotBe(alarm2);
            result.Should().BeTrue();
        }

        [Test]
        public void GivenAlarm_WhenComparedWithNull_ShouldNotBeEqual()
        {
            var alarm = new Alarm { };

            var result = alarm == null;

            alarm.Should().NotBe(null);
            result.Should().BeFalse();
        }

        [Test]
        public void GivenNullObject_WhenComparedWithNull_ShouldBeEqual()
        {
            Alarm alarm = null;

            var result = alarm == null;

            result.Should().BeTrue();
        }

        [Test]
        public void WhenCreateAlarm_PropertiesShouldNotBeNull()
        {
            var alarm = new Alarm { };

            alarm.Source.Should().NotBeNull();
            alarm.Message.Should().NotBeNull();
        }
    }
}
