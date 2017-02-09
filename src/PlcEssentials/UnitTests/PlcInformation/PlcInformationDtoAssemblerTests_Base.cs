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


using ApprovalUtilities.Utilities;
using Moq;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcEssentials.PlcInformation;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.PlcInformation
{
    public class PlcInformationDtoAssemblerTests_Base
    {
        protected PlcInformationDtoAssembler CreateAssembler()
        {
            return new PlcInformationDtoAssembler();
        }

        protected static Tag VelocityTag(
            string dataType = IEC61131_3_DataTypes.USInt,
            string name = "Velocity",
            string unit = "m/s",
            string comment = "Ruuuun!",
            object value = null,
            params Tag[] children)
        {
            return Tag(dataType, name, value, unit, comment, children);
        }

        protected static Tag Tag(string dataType, string name, object value, string unit, string comment, params Tag[] children)
        {
            var tag = new Tag
            {
                DataType = dataType,
                Name = name,
                Value = value,
                MetaData =
                {
                    UnitForUser = unit,
                    Comment = comment
                }
            };
            tag.Childs.AddAll(children);
            return tag;
        }

        protected static ICommand Command(string name, string comment)
        {
            var commandMock = new Mock<ICommand>();
            commandMock.Setup(x => x.Name).Returns(name);
            commandMock.Setup(x => x.Comment).Returns(comment);
            return commandMock.Object;
        }
    }
}
