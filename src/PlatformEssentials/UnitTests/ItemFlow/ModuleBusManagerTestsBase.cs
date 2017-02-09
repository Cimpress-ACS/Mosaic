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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Moq;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Credentials;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;
using VP.FF.PT.Common.PlatformEssentials.JobManagement;
using VP.FF.PT.Common.PlatformEssentials.Statistic;
using VP.FF.PT.Common.TestInfrastructure;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow
{
    public class ItemFlowTestBase<TModule> where TModule : IPlatformModule, new()
    {
        protected IPlatformModuleInitializer ModuleBusInitializer;
        protected CompositionContainer Container;
        protected IPlatformModuleRepository ModuleRepository;
        protected TModule ModuleA;
        protected TModule ModuleB;
        protected TModule ModuleC;
        protected TModule ModuleD;
        protected TModule ModuleE;
        protected TModule ModuleF;
        protected TModule ModuleG;
        protected TModule ModuleX;

        protected Mock<ILogger> Logger;
        protected IEventAggregator EventAggregator;
        protected Mock<ISafeEventRaiser> SafeEventRaiser;
        protected Mock<IEntityContextFactory> EntityContextFactory;
        protected Mock<IJobManager> JobManager;
        protected Mock<IAlarmManager> AlarmManager;
        protected Mock<IPlatformModuleEntities> PlatformEntities;

        protected IModuleBusManager ModuleBusManager;

        public virtual void SetUp()
        {
            Logger = new Mock<ILogger>();
            SafeEventRaiser = new Mock<ISafeEventRaiser>();
            AlarmManager = new Mock<IAlarmManager>();
            PlatformEntities = new Mock<IPlatformModuleEntities>();

            EntityContextFactory = new Mock<IEntityContextFactory>();
            EntityContextFactory
                .Setup(x => x.CreateContext())
                .Returns(new NoDbEntityContext());

            PlatformEntities.Setup(a => a.GetPlatformModuleEntity(It.IsAny<PlatformModuleEntity>())).Returns<PlatformModuleEntity>(a => a);
            PlatformEntities.Setup(a => a.GetAll()).Returns(new List<PlatformModuleEntity>());

            JobManager = new Mock<IJobManager>();

            ComposeMefContainer();

            ModuleBusManager.Construct();
            ModuleBusManager.Initialize();
            ModuleBusManager.Activate();

            ModuleA = (TModule)ModuleRepository.GetModule("ModuleA");
            ModuleB = (TModule)ModuleRepository.GetModule("ModuleB");
            ModuleC = (TModule)ModuleRepository.GetModule("ModuleC");
            ModuleD = (TModule)ModuleRepository.GetModule("ModuleD");
            ModuleE = (TModule)ModuleRepository.GetModule("ModuleE");
            ModuleF = (TModule)ModuleRepository.GetModule("ModuleF");
            ModuleG = (TModule)ModuleRepository.GetModule("ModuleG");
            ModuleX = (TModule)ModuleRepository.GetModule("ModuleX");
        }

        protected void TearDown()
        {
            ModuleBusManager = null;
            ModuleA = default(TModule);
            ModuleB = default(TModule);
            ModuleC = default(TModule);
            ModuleD = default(TModule);
            ModuleE = default(TModule);
            ModuleF = default(TModule);
            ModuleG = default(TModule);
            ModuleX = default(TModule);
        }

        private void ComposeMefContainer()
        {
            AssemblyUtilities.SetEntryAssembly();

            var types = new TypeCatalog(
                typeof(PlatformModuleActivator),
                typeof(PlatformModuleCreator),
                typeof(PlatformModuleInitializer),
                typeof(ModuleContainer),
                typeof(ModuleMock),
                typeof(ModuleFactoryMock),
                typeof(PlatformDependencyManager),
                typeof(ConfigurationAccessor),
                typeof(EventAggregator),
                typeof(ModuleMetricMeasurement));

            var aggregateCatalog = new AggregateCatalog(types);

            Container = new CompositionContainer(aggregateCatalog);

            Container.ComposeExportedValue(Container);
            Container.ComposeExportedValue(Logger.Object);
            Container.ComposeExportedValue(SafeEventRaiser.Object);
            Container.ComposeExportedValue(EntityContextFactory.Object);
            Container.ComposeExportedValue(JobManager.Object);
            Container.ComposeExportedValue(PlatformEntities.Object);
            Container.ComposeExportedValue(new CompositeAlarmManager(Logger.Object));
            Container.ComposeExportedValue(new ModuleBusManager() as IModuleBusManager);

            Container.ComposeParts();

            EventAggregator = Container.GetExportedValue<IEventAggregator>();

            ModuleRepository = Container.GetExportedValue<IPlatformModuleRepository>();

            ModuleBusInitializer = Container.GetExportedValue<IPlatformModuleInitializer>();

            ModuleBusManager = Container.GetExportedValue<IModuleBusManager>();
            Container.SatisfyImportsOnce(ModuleBusManager);
        }

        protected void StartAllModules()
        {
            ModuleA.Start();
            ModuleB.Start();
            ModuleC.Start();
            ModuleD.Start();
            ModuleE.Start();
            ModuleF.Start();
            ModuleG.Start();
            ModuleX.Start();
        }
    }
}
