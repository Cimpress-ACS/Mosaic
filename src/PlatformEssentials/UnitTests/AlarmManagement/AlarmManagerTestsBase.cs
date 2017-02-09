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


using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.TestInfrastructure;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement
{
    public class AlarmManagerTestsBase
    {
        protected Alarm FirstAlarm { get; private set; }

        [SetUp]
        public void Setup()
        {
            FirstAlarm = CreateRandomAlarm();
        }

        protected IAlarmManager CreateAlarmManager(params IManageCurrentAlarmsPlugin[] plugins)
        {
            return new CompositeAlarmManager(plugins, new Mock<ILogger>().Object);
        }

        protected static Alarm CreateRandomAlarm()
        {
            return new Alarm {AlarmId = CreateRandom.Int(), Source = CreateRandom.String()};
        }
    }
}
