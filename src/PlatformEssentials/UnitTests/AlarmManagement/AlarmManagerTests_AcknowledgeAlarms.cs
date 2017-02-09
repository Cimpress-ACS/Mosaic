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

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    public class AlarmManagerTests_AcknowledgeAlarms : AlarmManagerTestsBase
    {
        [Test]
        public void OnManagerWithCurrentAlarm_ShouldExposeAlarmAsHistoric()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.AcknowledgeAlarms();
            alarmManager.HistoricAlarms.Should().Contain(FirstAlarm);
        }

        [Test]
        public void OnManagerWithCurrentAlarm_ShouldRaiseAlarmsChangedEvent()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.MonitorEvents();
            alarmManager.AcknowledgeAlarms();
            alarmManager.ShouldRaise("AlarmsChanged");
        }

        [Test]
        public void OnManagerWithoutAlarm_ShouldNotRaiseAlarmsChangedEvent()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.MonitorEvents();
            alarmManager.AcknowledgeAlarms();
            alarmManager.ShouldNotRaise("AlarmsChanged");
        }

        [Test]
        public void OnManagerWithCurrentAlarm_ShouldNotExposeAlarmAsCurrentAnymore()
        {
            IAlarmManager alarmManager = CreateAlarmManager(new InMemoryManageCurrentAlarmsPlugin());
            alarmManager.AddAlarm(FirstAlarm);
            alarmManager.AcknowledgeAlarms();
            alarmManager.CurrentAlarms.Should().BeEmpty();
        }
    }
}
