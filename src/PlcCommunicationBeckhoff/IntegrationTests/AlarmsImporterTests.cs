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


using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class AlarmsImporterTests
    {
        private BeckhoffOnlineAlarmsImporter _alarmsImporter;
        private ITagController _tagController;
        private ITagListener _tagListener;

        [SetUp]
        public void SetUp()
        {
            _tagController = new BeckhoffTagController(Global.AdsAddress, Global.AdsPort);
            _tagListener = new BeckhoffPollingTagListener(Global.AdsAddress, Global.AdsPort, new GlobalLock());
            _alarmsImporter = new BeckhoffOnlineAlarmsImporter(Global.AdsAddress, Global.AdsPort, new GlobalLock());
            _alarmsImporter.Initialize(_tagListener);

            _tagController.StartConnection();
            _tagListener.StartListening();

            // prepare test by clear out all alarms
            _tagController.WriteTag(WriteTags.GlobalAlarmsCommandTag, WriteTags.ClearAllAlarmsCommand).Wait();
            Thread.Sleep(500);
        }

        [TearDown]
        public void TearDown()
        {
            _tagListener.Dispose();
            _tagController.Dispose();
            _alarmsImporter = null;
        }

        [Test]
        public void GivenNoAlarms_WhenImport_ShouldBeEmpty()
        {
            _alarmsImporter.ImportAlarms();
            var alarms = _alarmsImporter.GetAllImportedAlarms();

            alarms.Should().BeEmpty();
        }

        [Test]
        public void GivenOneAlarm_WhenImport_ShouldReturnAlarm()
        {
            GenerateAlarm(5);

            _alarmsImporter.ImportAlarms();
            var alarms = _alarmsImporter.GetAllImportedAlarms();

            alarms.Should().HaveCount(1, "alarm with id 5 was added");
            alarms.First().AlarmNumber.Should().Be(5);
        }

        [Test]
        public void WhenAddAlarm_ShouldRaiseEvent()
        {
            _alarmsImporter.MonitorEvents();
            GenerateAlarm(10);

            _alarmsImporter.ShouldRaise("AlarmsChanged");
        }

        [Test]
        public void WhenRemoveAlarm_ShouldRaiseEvent()
        {
            GenerateAlarm(15);
            _alarmsImporter.MonitorEvents();
            RemoveAlarm(15);

            _alarmsImporter.ShouldRaise("AlarmsChanged");
        }

        [Test]
        public void WhenImportFromSpecificController_ShouldReturnRightAlarms()
        {
            GenerateAlarm(20);

            _alarmsImporter.GetAlarms(4).Should().HaveCount(1, "controller id 4 is AGS_1 which generates the alarm");
            _alarmsImporter.GetAlarms(5).Should().BeEmpty("controller id 5 is AGS_2 which did not generate any alarm");
        }

        // uses the AGS (Alarm Generator Simulator PLC controller) to generate alarms
        private void GenerateAlarm(int alarmId)
        {
            _tagController.WriteTag(WriteTags.AgsAlarmIdTag, alarmId).Wait();
            _tagController.WriteTag(WriteTags.AgsCommandTag, WriteTags.AddCommand).Wait();
            Thread.Sleep(500);
        }

        private void RemoveAlarm(int alarmId)
        {
            _tagController.WriteTag(WriteTags.AgsAlarmIdTag, alarmId).Wait();
            _tagController.WriteTag(WriteTags.AgsCommandTag, WriteTags.RemoveCommand).Wait();
            Thread.Sleep(500);
        }

        private static class WriteTags
        {
            public const short AddCommand = 202;
            public const short RemoveCommand = 204;
            public const short ClearAllAlarmsCommand = 2;


            public readonly static Tag AgsAlarmIdTag = new Tag("fbAGS_1.udiAlarmId", "MiddlePRG_1", "UDINT", Global.AdsPort);
            public readonly static Tag AgsCommandTag = new Tag("fbAGS_1." + NamingConventions.CommonInterfaceAutoCmdChannel, "MiddlePRG_1", "INT", Global.AdsPort);
            public readonly static Tag GlobalAlarmsCommandTag = new Tag(NamingConventions.PathAlarmManager + "." + NamingConventions.AlarmManagerCommandChannel, NamingConventions.Global, "INT", Global.AdsPort);
        }
    }
}
