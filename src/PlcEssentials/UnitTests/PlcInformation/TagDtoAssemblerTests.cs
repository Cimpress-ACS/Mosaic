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
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcEssentials.PlcInformation;
using VP.FF.PT.Common.PlcEssentials.PlcInformation.DTOs;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.PlcInformation
{
    [TestFixture]
    public class TagDtoAssemblerTests
    {
        [Test]
        public void WithTag_ShouldReturnDto()
        {
            Tag tag = Tag(
                dataType: IEC61131_3_DataTypes.Int,
                name: "Throughput",
                value: 2100,
                unit: "Shirts",
                comment: "Shirts, baby");

            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);

            dto.ShouldHaveValues(
                type: typeof(short).ToString(),
                name: "Throughput",
                value: "2100",
                unit: "Shirts",
                comment: "Shirts, baby",
                key: "Throughput");
        }

        [Test]
        public void WithTagVelocity_ShouldReturnDtoVelocity()
        {
            Tag tag = VelocityTag(value: 12);

            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);

            dto.ShouldHaveValues(
                type: typeof(byte).ToString(),
                name: "Velocity",
                value: "12",
                unit: "m/s",
                comment: "Ruuuun!",
                key: "Velocity");
        }

        [Test]
        public void WithTagVelocityHavingTypeStatisticalData_ShouldReturnDtoWithTypeNull()
        {
            Tag velocityTag = VelocityTag(value: 12, dataType: "T_StatisticalData");

            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(velocityTag);

            dto.Type.Should().BeNull();
        }

        [Test]
        public void WithTagVelocityHavingValueNull_ShouldReturnDtoWithEmptyValue()
        {
            Tag tag = VelocityTag(value: null);

            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);

            dto.Value.Should().Be(string.Empty);
        }

        [Test]
        public void WithTagHavingListValue_ShouldReturnDtoWithListValue()
        {
            Tag tag = VelocityTag(value: null,
                                  dataType: "ARRAY (..) of INT",
                                  children: new[] { VelocityTag(value: 1, dataType: "INT"), VelocityTag(value: 2, dataType: "INT") });

            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);

            dto.Type.Should().Be("System.Array");
            dto.Children.Should().Contain(child => child.Type == "System.Int16" && child.Value == "1")
                             .And.Contain(child => child.Type == "System.Int16" && child.Value == "2");
        }

        [Test]
        public void WithTagHavingEnumValue_ShouldReturnDtoWithShortType()
        {
            Tag tag = VelocityTag(dataType: "E_DJM_PRINT_MODULE");
            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);
            dto.Type.Should().Be("System.Int16");
        }

        [Test]
        public void GivenNestedTypeName_WithTagHavingEnumValue_ShouldReturnDtoWithShortType()
        {
            Tag tag = VelocityTag(dataType: "BasePPT.E_DJM_PRINT_MODULE");
            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);
            dto.Type.Should().Be("System.Int16");
        }

        [Test]
        public void WithTagHavingStringValue_ShouldReturnDtoWithStringType()
        {
            Tag tag = VelocityTag("STRING(80)");
            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(tag);
            dto.Type.Should().Be("System.String");
        }

        [Test]
        public void WithNull_ShouldReturnNull()
        {
            TagDtoAssembler assembler = CreateAssembler();
            TagDTO dto = assembler.Assemble(null);

            dto.Should().BeNull();
        }

        private TagDtoAssembler CreateAssembler()
        {
            return new TagDtoAssembler();
        }

        private static Tag Tag(string dataType, string name, object value, string unit, string comment, params Tag[] children)
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

        private static Tag VelocityTag(
            string dataType = IEC61131_3_DataTypes.USInt,
            string name = "Velocity",
            string unit = "m/s",
            string comment = "Ruuuun!",
            object value = null,
            params Tag[] children)
        {
            return Tag(dataType, name, value, unit, comment, children);
        }
    }

    public static class TagDtoAssertionExtensions
    {
        public static void ShouldHaveValues(this TagDTO dto, string comment, string key, string name, string type, string unit, string value)
        {
            dto.Comment.Should().Be(comment);
            dto.Key.Should().Be(key);
            dto.Name.Should().Be(name);
            dto.Type.Should().Be(type);
            dto.Unit.Should().Be(unit);
            dto.Value.Should().Be(value);
        }
    }
}
