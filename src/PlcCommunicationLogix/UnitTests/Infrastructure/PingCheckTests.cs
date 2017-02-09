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
using VP.FF.PT.Common.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests.Infrastructure
{
    [TestFixture]
    public class PingCheckTests
    {
        [Test]
        public void WhenPinginLocalMachine_ResultMustBeTrue()
        {
            bool result = PingCheck.SynchrounouslyCheck("127.0.0.1");
            result.Should().BeTrue("Ping to local machine must be successful.");
        }

        [Test]
        public void WhenPingingNotExistingMachine_ResultMustBeFalse()
        {
            bool result = PingCheck.SynchrounouslyCheck("1.2.3.4");
            result.Should().BeFalse();
        }

        [Test]
        public void WhenPingingInvalidAddress_ResultMustBeFalse()
        {
            bool result = PingCheck.SynchrounouslyCheck("1.1.1.1.1");
            result.Should().BeFalse();
        }
    }
}
