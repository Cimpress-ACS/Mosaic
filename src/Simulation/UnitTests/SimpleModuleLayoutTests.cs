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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.TestInfrastructure;

namespace VP.FF.PT.Common.Simulation.UnitTests
{
    /// <summary>
    /// This class does not use a given ModuleBusManager with its graph but a simple custom created platform layout.
    ///
    /// ModuleA(length=10) --> ModuleB(length=10)
    /// </summary>
    [TestFixture]
    public class CustomModuleLayoutTests
    {
        private CompositionContainer _container;
        private Mock<ILogger> _logger;
        private IModuleSimulator _moduleA;
        private IModuleSimulator _moduleB;
        private ITaktManager _taktManager;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger>();

            ComposeMefContainer();

            var factory = _container.GetExportedValue<IModuleSimulatorFactory>();
            _moduleA = factory.CreateModule();
            _moduleA.Initialize(10, "ModuleA");
            _moduleB = factory.CreateModule();
            _moduleB.Initialize(10, "ModuleB");           
            factory.TriggerContainerRecomposition();

            _taktManager = _container.GetExportedValue<ITaktManager>();
            _container.GetExportedValue<SimulatedItemFlowManager>();
        }

        [TearDown]
        public void TearDown()
        {
            _taktManager.Stop();
        }

        [Test]
        public void StartTaktManagerTest()
        {
            _moduleA.AddItem(new SimulatedItem {ItemId = 1});
            Action a = () =>
            {
                _taktManager.Start();
                Task.Delay(20);
                _taktManager.Stop();
                _taktManager.Start();
                _taktManager.Start();
            };
            
            a.ShouldNotThrow();
        }

        [Test]
        public void WhenItemPositionExceedsModuleLength_ShouldMoveToNextModule()
        {
            var item = new SimulatedItem {ItemId = 1};
            _moduleA.AddItem(item);

            for (int i = 0; i <= 10; i++)
                _taktManager.SingleStep();

            _moduleA.ItemCount.Should().Be(0);
            _moduleB.ItemCount.Should().Be(1);
        }

        [Test]
        public void WhenItemMovesToNextModule_ShouldAddLogHistory()
        {
            var item = new SimulatedItem { ItemId = 1 };
            _moduleA.AddItem(item, 9);

            _moduleA.Takt();

            item.LogHistory.Should().Contain("Moved item from " + _moduleA.Name + " to " + _moduleB.Name);
        }

        [Test]
        public void GivenAddedNewItem_WhenItemPositionExceedsModuleLength_ShouldRaiseCorrectRxEvent()
        {
            var item = new SimulatedItem { ItemId = 1 };
            _moduleA.AddItem(item);
            SimulatedItemLeftModuleData itemLeftData = null;
            _moduleA.ItemLeft.Subscribe(data => itemLeftData = data);

            for (int i = 0; i <= 10; i++)
                _taktManager.SingleStep();

            itemLeftData.Should().NotBeNull();
            itemLeftData.Item.Should().Be(item);
            itemLeftData.OriginModuleSimulator.Should().Be(_moduleA, "the origin module of the test item was ModuleA");
        }

        private void ComposeMefContainer()
        {
            AssemblyUtilities.SetEntryAssembly();

            var aggregateCatalog = new AggregateCatalog();
            var thisAssemblyCatalog = new AssemblyCatalog(Assembly.GetEntryAssembly());
            var simulationAssemblyCatalog = new AssemblyCatalog(typeof(TaktManager).Assembly);
            aggregateCatalog.Catalogs.Add(thisAssemblyCatalog);
            aggregateCatalog.Catalogs.Add(simulationAssemblyCatalog);

            _container = new CompositionContainer(aggregateCatalog);

            _container.ComposeExportedValue(_container);
            _container.ComposeExportedValue(_logger.Object);
        }
    }
}
