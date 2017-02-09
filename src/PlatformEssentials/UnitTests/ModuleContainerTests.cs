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
using NUnit.Framework;
using VP.FF.PT.Common.TestInfrastructure;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    [TestFixture]
    public class ModuleContainerTests
    {
        private const string SingleOvenModule = "Oven"; // contains no instance id information
        
        private int _loa1Port;
        private IPlatformModule _loa1Module;
        private IPlatformModule _lab1Module;
        private IPlatformModule _ovenModule;

        [SetUp]
        public void Setup()
        {
            _loa1Port = CreateRandom.Int();

            _loa1Module = new VirtualPlatformModule { Name = Loa1(), AdsPort = _loa1Port, StreamType = 1 };
            _lab1Module = new VirtualPlatformModule { Name = Lab1(), AdsPort = 113, StreamType = 1 };
            _ovenModule = new VirtualPlatformModule { Name = SingleOvenModule, AdsPort = 841, StreamType = 2 };
        }

        [Test]
        public void FindPlatformModule_WithValidModuleName_ShouldReturnModule()
        {
            IPlatformModuleRepository finder = ModuleContainer(_loa1Module, _lab1Module);
            IPlatformModule platformModule = finder.GetModule(Loa1());
            platformModule.Should().BeSameAs(_loa1Module);
        }

        [Test]
        public void FindPlatformModule_WithDifferentModuleName_ShouldReturnModule()
        {
            IPlatformModuleRepository finder = ModuleContainer(_loa1Module, _lab1Module);
            IPlatformModule platformModule = finder.GetModule(Lab1());
            platformModule.Should().BeSameAs(_lab1Module);
        }

        [Test]
        public void FindPlatformModule_WithNotExistingModuleName_ShouldThrow()
        {
            IPlatformModuleRepository finder = ModuleContainer(_loa1Module, _lab1Module);
            finder.Invoking(f => f.GetModule("NotExistingName")).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void FindModuleByType()
        {
            IPlatformModuleRepository moduleRepository = ModuleContainer(_loa1Module);

            var result = moduleRepository.GetModulesByType<IPlatformModule>();

            result.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GivenInstance_FindModuleByTypeAndInstance()
        {
            IPlatformModuleRepository moduleRepository = ModuleContainer(_loa1Module);

            var result = moduleRepository.GetModuleByType<IPlatformModule>(_loa1Module.ModuleNbr);

            result.Should().Be(_loa1Module);
        }

        [Test]
        public void GivenSingleInstance_GivenInstance_FindModuleByTypeAndInstance()
        {
            IPlatformModuleRepository moduleRepository = ModuleContainer(_ovenModule);

            var result = moduleRepository.GetModuleByType<IPlatformModule>(_ovenModule.ModuleNbr);

            result.Should().Be(_ovenModule);
        }

        [Test]
        public void GivenNoInstance_FindModuleByTypeAndInstance()
        {
            IPlatformModuleRepository moduleRepository = ModuleContainer(_loa1Module);

            Action action = () => moduleRepository.GetModuleByType<IPlatformModule>(2);

            action.ShouldThrow<InvalidOperationException>("There is no instance available with instance id 2");
        }

        private ModuleContainer ModuleContainer(params IPlatformModule[] modules)
        {
            return new ModuleContainer(modules);
        }

        private static string Lab1() { return "LAB_1"; }
        private static string Loa1() { return "LOA_1"; }
    }
}
