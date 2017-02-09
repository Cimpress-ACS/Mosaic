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
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    [TestFixture]
    public class LogixTagExtensionsTests
    {
        [Test]
        public void FullName_WhenTagIsInNoScope_JustTagNameMustBeReturned()
        {
            var tag = new Tag { Name = "SensorA" };

            var result = tag.FullName();

            result.Should().Be("SensorA", "when no scope is given the global controller tags can be accessed with simple tag name");
        }

        [Test]
        public void FullName_WhenTagIsInProgramScope_FullAddressMustBeReturned()
        {
            var tag = new Tag { Name = "SensorB", Scope = "ConveyorA" };

            var result = tag.FullName();

            result.Should().Be("PROGRAM:ConveyorA.SensorB", "when scope is given the full access path to tag is expected");
        }
    }
}
