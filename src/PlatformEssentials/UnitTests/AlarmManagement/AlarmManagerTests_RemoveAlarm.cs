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
    public class AlarmManagerRemoveTests : AlarmManagerTestsBase
    {
        [Test]
        public void HasAlarms_WhenRemoveAlarm_ShouldRemoveTheRightOne()
        {
            var firstAlarm = new Alarm {AlarmId = 1};
            var secondAlarm = new Alarm {AlarmId = 2};
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(firstAlarm);
            alarmManager.AddAlarm(secondAlarm);

            alarmManager.RemoveAlarm(secondAlarm);

            alarmManager.CurrentAlarms.Should().Contain(firstAlarm);
            alarmManager.CurrentAlarms.Should().NotContain(secondAlarm);
        }

        [Test]
        public void HasAlarms_WhenRemoveAlarmUsingId_ShouldRemoveAlarm()
        {
            var alarm = new Alarm { AlarmId = 777, Source = "Test"};
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(alarm);

            alarmManager.RemoveAlarm("Test", 777);

            alarmManager.CurrentAlarms.Should().BeEmpty();
        }

        [Test]
        public void NonResettableAlarm_AlarmsAcknowledged_NonResettableAlarmShouldStay()
        {
            var alarm = new Alarm { IsResettable = false };
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(alarm);

            alarmManager.AcknowledgeAlarms();

            alarmManager.CurrentAlarms.Should().NotBeEmpty();
        }

        [Test]
        public void NonResettableAlarm_WhenRemoveAlarm_AlarmShouldBeGone()
        {
            var alarm = new Alarm { IsResettable = false };
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(alarm);

            alarmManager.RemoveAlarm(alarm);

            alarmManager.CurrentAlarms.Should().BeEmpty();
        }
    }
}
