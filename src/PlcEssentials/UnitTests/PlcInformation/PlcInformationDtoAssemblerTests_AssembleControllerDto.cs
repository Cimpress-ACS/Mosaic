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
using System.Collections.Generic;
using System.Linq;
using ApprovalTests.Reporters;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials.PlcInformation;
using VP.FF.PT.Common.PlcEssentials.PlcInformation.DTOs;
using VP.FF.PT.Common.TestInfrastructure.ApprovalTests;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.PlcInformation
{
    [UseReporter(typeof(DiffReporter))]
    [TestFixture]
    public class PlcInformationDtoAssemblerTests_AssembleControllerDto : PlcInformationDtoAssemblerTests_Base
    {
        [Test]
        public void WithTreeAndRecursive_ShouldAssembleChilds()
        {
            IController controller = Controller(
                activeAlarm: Alarm("Root Alarm", AlarmType.Info),
                actualValues: new [] {VelocityTag()},
                children: new[] { ChildController(
                                        parameters:new[]{ Tag(
                                                            dataType:"INT",
                                                              name:"AnyTag",
                                                            value:13,
                                                            unit:"...",
                                                            comment:"Whatever")}).Object },
                commands: new [] { Command("Start", "Starts the system") },
                configurations: new Tag[0],
                controllerMode: ControllerMode.Auto,
                currentState: "Running",
                currentSubState: "Sub-Running",
                enableForcing: true,
                id: 1,
                isEnabled: true,
                isSimulation: true,
                name: "Root",
                fullname:"Root",
                parameters: new Tag[0],
                type: "RootController").Object;
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(controller, recursive:true);
            ApprovalTests.Approvals.Verify(dto.ToJson());
        }

        [Test]
        public void WithControllerRoot_ShouldReturnDtoRoot()
        {
            IController controller = Controller(
                activeAlarm: Alarm("Root Alarm", AlarmType.Info),
                actualValues: new Tag[0],
                children: new IController[0],
                commands: new ICommand[0],
                configurations: new Tag[0],
                controllerMode: ControllerMode.Auto,
                currentState: "Running",
                currentSubState: "Sub-Running",
                enableForcing: true,
                id: 1,
                isEnabled: true,
                isSimulation: true,
                name: "Root",
                fullname:"Root",
                parameters: new Tag[0],
                type: "RootController").Object;
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(controller);
            dto.ShouldHaveValues(
                activeAlarm: "Root Alarm",
                alarmType: PlcEssentials.PlcInformation.DTOs.AlarmType.Normal,
                currentState: "Running",
                currentSubState: "Sub-Running",
                enableForcing: true,
                id: 1,
                isEnabled: true,
                isSimulated: true,
                mode: PlcEssentials.PlcInformation.DTOs.ControllerMode.Auto,
                name: "Root",
                fullname:"Root",
                type: "RootController");
        }

        [Test]
        public void WithControllerChild_ShouldReturnDtoChild()
        {
            IController controller = ChildController(
                                        activeAlarm: Alarm("Special Child Alarm", AlarmType.Warning)).Object;
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(controller);
            dto.ShouldHaveValues(
                activeAlarm: "Special Child Alarm",
                alarmType: PlcEssentials.PlcInformation.DTOs.AlarmType.Warning,
                currentState: "Ready",
                currentSubState: "Sub-Ready",
                enableForcing: false,
                id: 2,
                isEnabled: false,
                isSimulated: false,
                mode: PlcEssentials.PlcInformation.DTOs.ControllerMode.Manual,
                name: "Child",
                fullname:"Child",
                type: "ChildType");
        }

        [Test]
        public void WithControllerHavingNoAlarm_ShouldReturnDtoWithEmptyAlarm()
        {
            IController controller = ChildController().Object;
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(controller);
            dto.ActiveAlarm.Should().BeEmpty();
        }

        [Test]
        public void WithControllerHavingChildAndRecurcive_ShouldReturnBothDtos()
        {
            IController controller = ChildController(children: new[] { ChildController(name: "Subchild").Object }).Object;
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(controller, recursive:true);
            dto.Children.Should().HaveCount(1);
            dto.Children.First().Name.Should().Be("Subchild");
        }

        [Test]
        public void WithControllerHavingChildAndNonRecursive_ShouldReturnNoChild()
        {
            IController controller = ChildController(children: new[] { ChildController(name: "Subchild").Object }).Object;
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(controller, recursive: false);
            dto.Children.Should().BeEmpty();
        }

        [Test]
        public void WithNull_ShouldReturnNull()
        {
            PlcInformationDtoAssembler assembler = CreateAssembler();
            ControllerDTO dto = assembler.AssembleControllerDto(null);
            dto.Should().BeNull();
        }

        [Test]
        public void WithEnumerableOfTwoControllers_ShouldReturnBothAsDtos()
        {
            IEnumerable<IController> controllers = new[] { ChildController().Object,
                                                            ChildController(name: "Root").Object };
            PlcInformationDtoAssembler assembler = CreateAssembler();
            IEnumerable<ControllerDTO> dtos = assembler.AssembleControllerDtos(controllers).ToList();
            dtos.Should().HaveCount(2);
            dtos.First().Name.Should().Be("Child");
            dtos.Last().Name.Should().Be("Root");
        }

        [Test]
        public void WithEnumerableHavingNullValue_ShouldIgnoreNullValues()
        {
            IEnumerable<IController> controllers = new[] { null, ChildController().Object };
            PlcInformationDtoAssembler assembler = CreateAssembler();
            IEnumerable<ControllerDTO> dtos = assembler.AssembleControllerDtos(controllers).ToList();
            dtos.Should().HaveCount(1);
            dtos.First().Name.Should().Be("Child");
        }

        [Test]
        public void WithEnumerableNull_ShouldReturnEmptyList()
        {
            PlcInformationDtoAssembler assembler = CreateAssembler();
            IEnumerable<ControllerDTO> dtos = assembler.AssembleControllerDtos(null).ToList();
            dtos.Should().BeEmpty();
        }

        private static Mock<IController> ChildController(
            Mock<IAlarm> activeAlarm = null,
            IEnumerable<Tag> actualValues = null,
            IEnumerable<IController> children = null,
            IEnumerable<ICommand> commands = null,
            IEnumerable<Tag> configurations = null,
            ControllerMode controllerMode = ControllerMode.Manual,
            string currentState = "Ready",
            string currentSubState = "Sub-Ready",
            bool enableForcing = false,
            int id = 2,
            bool isEnabled = false,
            bool isSimulation = false,
            string name = "Child",
            string fullname = "Child",
            IEnumerable<Tag> parameters = null,
            string type = "ChildType")
        {
            return Controller(activeAlarm, actualValues, children, commands, configurations, controllerMode, currentState,
                currentSubState, enableForcing, id, isEnabled, isSimulation, name, fullname,parameters, type);
        }

        private static Mock<IController> Controller(
            Mock<IAlarm> activeAlarm,
            IEnumerable<Tag> actualValues,
            IEnumerable<IController> children,
            IEnumerable<ICommand> commands,
            IEnumerable<Tag> configurations,
            ControllerMode controllerMode,
            string currentState,
            string currentSubState,
            bool enableForcing,
            int id,
            bool isEnabled,
            bool isSimulation,
            string name,
            string fullname,
            IEnumerable<Tag> parameters,
            string type)
        {
            var controller = new Mock<IController>();
            if (activeAlarm != null)
                controller.Setup(c => c.ActiveAlarm).Returns(activeAlarm.Object);
            controller.Setup(c => c.ActualValues).Returns(actualValues);
            controller.Setup(c => c.Childs).Returns(children);
            controller.Setup(c => c.Commands).Returns(commands);
            controller.Setup(c => c.Configurations).Returns(configurations);
            controller.Setup(c => c.ControllerMode).Returns(controllerMode);
            controller.Setup(c => c.CurrentState).Returns(currentState);
            controller.Setup(c => c.CurrentSubState).Returns(currentSubState);
            controller.Setup(c => c.EnableForcing).Returns(enableForcing);
            controller.Setup(c => c.Id).Returns(id);
            controller.Setup(c => c.IsEnabled).Returns(isEnabled);
            controller.Setup(c => c.IsSimulation).Returns(isSimulation);
            controller.Setup(c => c.Name).Returns(name);
            controller.Setup(c => c.FullName).Returns(fullname);
            controller.Setup(c => c.Parameters).Returns(parameters);
            controller.Setup(c => c.Type).Returns(type);
            return controller;
        }

        private static Mock<IAlarm> Alarm(string text, AlarmType type)
        {
            var alarm = new Mock<IAlarm>();
            alarm.Setup(a => a.Text).Returns(text);
            alarm.Setup(a => a.AlarmType).Returns(type);
            return alarm;
        }
    }

    public static class ControllerDtoAssertionExtensions
    {
        public static void ShouldHaveValues(this ControllerDTO dto,
            string activeAlarm,
            PlcEssentials.PlcInformation.DTOs.AlarmType alarmType,
            string currentState,
            string currentSubState,
            bool enableForcing,
            int id,
            bool isEnabled,
            bool isSimulated,
            PlcEssentials.PlcInformation.DTOs.ControllerMode mode,
            string name,
            string fullname,
            string type)
        {
            dto.ActiveAlarm.Should().Be(activeAlarm);
            dto.ActualValues.Should().NotBeNull();
            dto.Children.Should().NotBeNull();
            dto.ControllerState.Should().Be(alarmType);
            dto.Commands.Should().NotBeNull();
            dto.Configurations.Should().NotBeNull();
            dto.CurrentState.Should().Be(currentState);
            dto.CurrentSubState.Should().Be(currentSubState);
            dto.EnableForcing.Should().Be(enableForcing);
            dto.Id.Should().Be(id);
            dto.Inputs.Should().NotBeNull();
            dto.IsEnabled.Should().Be(isEnabled);
            dto.IsSimulation.Should().Be(isSimulated);
            dto.Mode.Should().Be(mode);
            dto.Name.Should().Be(name);
            dto.FullName.Should().Be(fullname);
            dto.Outputs.Should().NotBeNull();
            dto.Parameters.Should().NotBeNull();
            dto.Type.Should().Be(type);
        }
    }
}
