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
using VP.FF.PT.Common.Simulation.Equipment;

namespace VP.FF.PT.Common.Simulation.UnitTests
{
    [TestFixture]
    public class EquipmentRepositoryTests
    {
        private EquipmentRepository _testee;

        [SetUp]
        public void SetUp()
        {
            _testee = new EquipmentRepository();
        }

        [Test]
        public void WhenAddEquipment_ShoundReturnRightObjects()
        {
            var module = new Mock<IModuleSimulator>().Object;
            var equipment = new Mock<ISimulatedEquipment>().Object;

            _testee.AddEquipment(equipment, module);

            _testee.GetEquipmentsOfModule(module).Should().Contain(equipment);
            _testee.GetModuleOfEquipment(equipment).Should().BeSameAs(module);
        }

        [Test]
        public void GivenEquipmentAlreadyAdded_WhenAddEquipment_ShouldThrowException()
        {
            var module = new Mock<IModuleSimulator>().Object;
            var anotherModule = new Mock<IModuleSimulator>().Object;
            var equipment = new Mock<ISimulatedEquipment>().Object;
            _testee.AddEquipment(equipment, module);

            var action = new Action(() => _testee.AddEquipment(equipment, anotherModule));

            action.ShouldThrow<SimulationException>();
        }

        [Test]
        public void GivenModulesWithEquipment_WhenSearchForModuleNameNotExisting_ShouldReturnEmpty()
        {
            SetupTwoModulesWithOneEquipmentEach("module1", "module2");

            var equipments = _testee.GetEquipmentsOfModule("notexistingmoduletest");

            equipments.Should().BeEmpty();
        }

        [Test]
        public void GivenModulesWithEquipment_WhenSearchForModuleName_ShouldReturnEquipmentsForIt()
        {
            var originalEquipments = SetupTwoModulesWithOneEquipmentEach("module1", "module2");

            var equipments = _testee.GetEquipmentsOfModule("module1");

            equipments.Should().Contain(originalEquipments.Item1);
            equipments.Should().NotContain(originalEquipments.Item2);
        }

        private Tuple<ISimulatedEquipment, ISimulatedEquipment> SetupTwoModulesWithOneEquipmentEach(string moduleName1, string moduleName2)
        {
            var moduleMock1 = new Mock<IModuleSimulator>();
            moduleMock1.Setup(m => m.Name).Returns(moduleName1);
            var moduleMock2 = new Mock<IModuleSimulator>();
            moduleMock2.Setup(m => m.Name).Returns(moduleName2);
            var equipment1 = new Mock<ISimulatedEquipment>().Object;
            var equipment2 = new Mock<ISimulatedEquipment>().Object;
            _testee.AddEquipment(equipment1, moduleMock1.Object);
            _testee.AddEquipment(equipment2, moduleMock2.Object);

            return new Tuple<ISimulatedEquipment, ISimulatedEquipment>(equipment1, equipment2);
        }
    }
}
