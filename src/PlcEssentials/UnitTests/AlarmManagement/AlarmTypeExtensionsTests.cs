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
using VP.FF.PT.Common.PlcEssentials.AlarmManagement;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    public class AlarmTypeExtensionsTests
    {
        [TestCase(Common.PlcEssentials.AlarmType.Info, PlatformEssentials.Entities.AlarmType.Info)]
        [TestCase(Common.PlcEssentials.AlarmType.Warning, PlatformEssentials.Entities.AlarmType.Warning)]
        [TestCase(Common.PlcEssentials.AlarmType.EmergencyOff, PlatformEssentials.Entities.AlarmType.Error)]
        [TestCase(Common.PlcEssentials.AlarmType.None, PlatformEssentials.Entities.AlarmType.Error)]
        [TestCase(Common.PlcEssentials.AlarmType.Off, PlatformEssentials.Entities.AlarmType.Error)]
        [TestCase(Common.PlcEssentials.AlarmType.Stop, PlatformEssentials.Entities.AlarmType.Error)]
        [TestCase(Common.PlcEssentials.AlarmType.TactStop, PlatformEssentials.Entities.AlarmType.Error)]
        public void GivenPlcAlarmType_WhenConverted_ConvertsToCorrectResult(Common.PlcEssentials.AlarmType plcAlarmType, PlatformEssentials.Entities.AlarmType entityAlarmType)
        {
            // execute
            var convertedAlarmType = plcAlarmType.ToEntity();

            // validate
            convertedAlarmType.Should().Be(entityAlarmType);
        }
    }
}
