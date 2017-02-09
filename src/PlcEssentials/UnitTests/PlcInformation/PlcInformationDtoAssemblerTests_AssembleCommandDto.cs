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


using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlcEssentials.PlcInformation;
using VP.FF.PT.Common.PlcEssentials.PlcInformation.DTOs;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.PlcInformation
{
    [TestFixture]
    public class PlcInformationDtoAssemblerTests_AssembleCommandDto : PlcInformationDtoAssemblerTests_Base
    {
        [Test]
        public void WithCommandStart_ShouldReturnDtoStart()
        {
            ICommand command = Command(Start(), StartsIt());
            PlcInformationDtoAssembler assembler = CreateAssembler();
            CommandDTO dto = assembler.AssembleCommandDto(command);
            dto.ShouldHaveValues(Start(), StartsIt());
        }

        [Test]
        public void WithCommandStop_ShouldReturnDtoStop()
        {
            ICommand command = Command(Stop(), StopsIt());
            PlcInformationDtoAssembler assembler = CreateAssembler();
            CommandDTO dto = assembler.AssembleCommandDto(command);
            dto.ShouldHaveValues(Stop(), StopsIt());
        }

        [Test]
        public void WithNull_ShouldReturnNull()
        {
            PlcInformationDtoAssembler assembler = CreateAssembler();
            CommandDTO dto = assembler.AssembleCommandDto(null);
            dto.Should().BeNull();
        }

        [Test]
        public void WithEnumerableHavingTwoCommands_ShouldReturnBothAsDtos()
        {
            IEnumerable<ICommand> commands = new[] { Command(Start(), StartsIt()), 
                                                     Command(Stop(), StopsIt())   };
            PlcInformationDtoAssembler assembler = CreateAssembler();
            IEnumerable<CommandDTO> dtos = assembler.AssembleCommandDtos(commands).ToList();
            dtos.Should().HaveCount(2);
            dtos.First().ShouldHaveValues(Start(), StartsIt());
            dtos.Last().ShouldHaveValues(Stop(), StopsIt());
        }

        [Test]
        public void WithNullEnumerable_ShouldReturnEmptyList()
        {
            PlcInformationDtoAssembler assembler = CreateAssembler();
            IEnumerable<CommandDTO> dtos = assembler.AssembleCommandDtos(null);
            dtos.Should().BeEmpty();
        }

        [Test]
        public void WithEnumerableHavingNullValues_ShouldIgnoreNullValues()
        {
            IEnumerable<ICommand> commands = new[] { Command(Start(), StartsIt()), null };
            PlcInformationDtoAssembler assembler = CreateAssembler();
            IEnumerable<CommandDTO> dtos = assembler.AssembleCommandDtos(commands).ToList();
            dtos.Should().HaveCount(1);
            dtos.First().ShouldHaveValues(Start(), StartsIt());
        }

        private static string Start() { return "Start"; }
        private static string StartsIt() { return "Starts it."; }
        private static string Stop() { return "Stop"; }
        private static string StopsIt() { return "Stops it."; }
    }

    public static class CommandDtoAssertionExtensions
    {
        public static void ShouldHaveValues(this CommandDTO dto, string name, string optionalText)
        {
            dto.Name.Should().Be(name);
            dto.OptionalText.Should().Be(optionalText);
        }
    }
}
