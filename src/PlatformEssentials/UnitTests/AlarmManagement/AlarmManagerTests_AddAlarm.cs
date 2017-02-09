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
    public class AlarmManagerTests_AddAlarm : AlarmManagerTestsBase
    {
        [Test]
        public void WithAlarm_ShouldExposeAddedAlarmAsCurrent()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.CurrentAlarms.Should().OnlyContain(a => Equals(a, FirstAlarm));
        }

        [Test]
        public void WithAlarm_ShouldRaiseAlarmsChangedEvent()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.MonitorEvents();
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.ShouldRaise("AlarmsChanged");
        }

        [Test]
        public void Twice_ShouldAddBothAlarms()
        {
            Alarm secondAlarm = CreateRandomAlarm();
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.AddAlarm(secondAlarm);
            alarmManager.CurrentAlarms.Should().ContainInOrder(FirstAlarm, secondAlarm);
        }

        [Test]
        public void WithAlreadyAddedAlarm_ShouldNotAddAgain()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.CurrentAlarms.Should().HaveCount(1);
        }

        [Test]
        public void WithAlreadyAddedAlarm_ShouldNotRaiseChangedEventOnSecondTry()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.MonitorEvents();
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.ShouldNotRaise("AlarmsChanged");
        }

        [Test]
        public void OnManagerWithoutPlugin_ShouldExposeEmptyEnumerable()
        {
            IAlarmManager alarmManager = CreateAlarmManager();
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.CurrentAlarms.Should().BeEmpty();
        }

        [Test]
        public void OnManagerWithoutPlugin_ShouldNotRaiseAlarmsChangedEvent()
        {
            IAlarmManager alarmManager = CreateAlarmManager();
            alarmManager.MonitorEvents();
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.ShouldNotRaise("AlarmsChanged");
        }
    }
}
