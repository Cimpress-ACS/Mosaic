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
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement.Wcf;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement.Wcf
{
    public class AlarmManagementServiceTests_Base
    {
        protected PlatformModuleStub _loadingStation1;
        protected PlatformModuleStub _labelModule1;
        protected SimpleAlarmManagementServiceCallback _serviceCallback;

        [SetUp]
        public void Setup()
        {
            _loadingStation1 = new PlatformModuleStub(LOA_1());
            _labelModule1 = new PlatformModuleStub(LAB_1());
            _serviceCallback = new SimpleAlarmManagementServiceCallback();
            SetupConcreteContext();
        }

        protected virtual void SetupConcreteContext()
        {
        }

        protected IAlarmManagementService CreateService()
        {
            var consoleOutLogger = new ConsoleOutLogger();
            var alarmManagementService = new AlarmManagementService(
                new ModuleContainer(new[] { _loadingStation1, _labelModule1 }),
                new RemoteSubscriberProvider(_serviceCallback),
                new SafeEventRaiser(consoleOutLogger),
                consoleOutLogger);
            return alarmManagementService;
        }

        protected static Alarm LoadingStationAlarm(int id)
        {
            return new Alarm
            {
                Id = id,
                AlarmId = 1337,
                Message = "Alert Alert",
                Source = LOA_1(),
                SourceType = AlarmSourceType.Plc,
                Timestamp = new DateTime(2014, 05, 09),
                Type = AlarmType.Warning
            };
        }

        protected static Alarm LabelModuleAlarm(int id)
        {
            return new Alarm
            {
                Id = id,
                AlarmId = 5446,
                Message = "Label Module crashed",
                Source = LAB_1(),
                SourceType = AlarmSourceType.LineControl,
                Timestamp = new DateTime(2014, 05, 20),
                Type = AlarmType.Error
            };
        }

        protected static string LOA_1()
        {
            return "LOA_1";
        }

        protected static string LAB_1()
        {
            return "LAB_1";
        }

        protected class PlatformModuleStub : VirtualPlatformModule
        {
            private readonly Mock<IAlarmManager> _alarmManager;
            private readonly string _name;

            public PlatformModuleStub(string name)
            {
                StreamType = 0;
                _alarmManager = new Mock<IAlarmManager>();
                _name = name;
            }

            public override string Name
            {
                get { return _name; }
            }

            public override IAlarmManager AlarmManager
            {
                get { return _alarmManager.Object; }
            }

            public override void ResetAlarms()
            {
                _alarmManager.Object.AcknowledgeAlarms();
            }

            public virtual void RaiseAlarmsChangedEvent()
            {
                _alarmManager.Raise(x => x.AlarmsChanged += null);
            }

            public virtual void SetupCurrentAlarms(params Alarm[] alarms)
            {
                _alarmManager.Setup(x => x.CurrentAlarms).Returns(alarms);
            }

            public virtual void SetupHistoricAlarms(params Alarm[] alarms)
            {
                _alarmManager.Setup(x => x.HistoricAlarms).Returns(alarms);
            }

            public virtual void VerifyResetAlarm()
            {
                _alarmManager.Verify(x => x.AcknowledgeAlarms());
            }

            public virtual void VerifyNoResetOfAlarms()
            {
                _alarmManager.Verify(x => x.AcknowledgeAlarms(), Times.Never);
            }
        }

        protected class SimpleAlarmManagementServiceCallback : IAlarmManagementServiceCallback
        {
            private string _moduleWithLastAlarmsChange;

            public virtual string ModuleWithLastAlarmsChange
            {
                get { return _moduleWithLastAlarmsChange; }
            }

            public virtual void AlarmsChanged(string module)
            {
                _moduleWithLastAlarmsChange = module;
            }

            public void Reset()
            {
                _moduleWithLastAlarmsChange = null;
            }
        }
    }
}
