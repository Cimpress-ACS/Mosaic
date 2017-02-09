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
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    public class AlarmManagerAddTests : AlarmManagerTestsBase
    {
        [Test]
        public void HasWarnings_OnManagerWithWarningAlarm_ShouldReturnTrue()
        {
            FirstAlarm.Type = AlarmType.Warning;
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.HasWarnings.Should().BeTrue();
        }

        [Test]
        public void HasWarnings_OnManagerWithErrorAlarm_ShouldReturnFalse()
        {
            FirstAlarm.Type = AlarmType.Error;
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.HasWarnings.Should().BeFalse();
        }

        [Test]
        public void HasErrors_OnManagerWithErrorAlarm_ShouldReturnTrue()
        {
            FirstAlarm.Type = AlarmType.Error;
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.HasErrors.Should().BeTrue();
        }

        [Test]
        public void HasErrors_OnManagerWithWarningAlarm_ShouldReturnFalse()
        {
            FirstAlarm.Type = AlarmType.Warning;
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.HasErrors.Should().BeFalse();
        }
    }
}
