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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Credentials;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    [TestFixture]
    public class PlatformModuleCreatorTests
    {
        private PlatformModuleCreator _testee;
        private Mock<IPlatformModuleRepository> _moduleRepository;
        private PlatformDependencyManager _dependencyManager;

        [SetUp]
        public void SetUp()
        {
            _moduleRepository = new Mock<IPlatformModuleRepository>();
            _dependencyManager = new PlatformDependencyManager();

            _testee = new PlatformModuleCreator(_moduleRepository.Object, new ConfigurationAccessor(), new Mock<ILogger>().Object, _dependencyManager);
            _testee.EventAggregator = new Mock<IEventAggregator>().Object;

            var privateTestee = new PrivateObject(_testee);
            var containers = new List<CompositionContainer> { new CompositionContainer() };

            var moduleFactory = new Mock<IPlatformModuleFactory>();
            moduleFactory
                .Setup(f => f.CreateModule())
                .Returns(new ModuleMock());

            var container = containers.First();
            container.ComposeExportedValue(new Mock<ILogger>().Object);
            container.ComposeExportedValue(moduleFactory.Object);
            container.ComposeParts();

            privateTestee.SetFieldOrProperty("_container", containers);
        }

        [Test]
        public void DependencyTest()
        {
            _moduleRepository
                .Setup(r => r.Modules)
                .Returns(new List<IPlatformModule>
                {
                    new ModuleMockWithPrintDone {Name = "JEM"},
                    new ModuleMock {Name = "GRM"}
                });

            _testee.ConstructModules(2);

            _dependencyManager.Dependencies.Should().HaveCount(2);
        }

        private class ModuleMockWithPrintDone : ModuleMock
        {
            public event EventHandler<EventArgs> PrintDone;
        }
    }
}
