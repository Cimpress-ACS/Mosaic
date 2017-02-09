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
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;
using Command = VP.FF.PT.Common.PlcEssentials.Impl.Command;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class CommandTests
    {
        [Test]
        public void EqualTest()
        {
            Command command = SetupCommand(1, true, "command");

            Command sameCommandDifferentValue = SetupCommand(1, false, "other name does not matter, its no primary key");

            command.Equals(sameCommandDifferentValue).Should().BeTrue();
        }

        [Test]
        public void CompareTest()
        {
            Command command = SetupCommand(99, true, "command");

            Command lowerCommand = SetupCommand(11, false, "command");
            
            command.CompareTo(lowerCommand).Should().BePositive();
        }

        [Test]
        public void FireTest()
        {
            var controllerMock = new Mock<IController>();
            var command = new Command(controllerMock.Object);

            command.Fire();

            controllerMock.Verify(
                controller => controller.SendCommand(command), 
                Times.Once(), "command.Fire() must trigger the associated controller to send this command to PLC");
        }

        private Command SetupCommand(short id, bool isAvailable, string name)
        {
            return new Command(null,
                new PlcEssentials.Command
                {
                    Available = isAvailable,
                    CommandId = id,
                    Name = name
                },
                new Tag[0].ToReadOnly());
        }
    }
}
