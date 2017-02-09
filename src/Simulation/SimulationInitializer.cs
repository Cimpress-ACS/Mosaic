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
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;
using VP.FF.PT.Common.Simulation.Alarms;

namespace VP.FF.PT.Common.Simulation
{
    [Export(typeof(ISimulationInitializer))]
    [Export(typeof(IShutdown))]
    public class SimulationInitializer : IShutdown, ISimulationInitializer
    {
        private readonly IModuleBusManager _moduleBusManager;
        private readonly IModuleSimulatorFactory _factory;
        private readonly IModuleSimulatorRepository _moduleRepository;
        private readonly ITaktManager _taktManager;
        private readonly SimulationAlarmHandler _alarmHandler;
        private readonly SimulatedItemFlowManager _itemFlowManager;
        private readonly IEnumerable<Lazy<ISimulatedBehavior>> _simulatedBehaviors;

        [ImportingConstructor]
        public SimulationInitializer(
            IModuleBusManager moduleBusManager,
            IModuleSimulatorFactory factory,
            IModuleSimulatorRepository moduleRepository,
            ITaktManager taktManager,
            SimulationAlarmHandler alarmHandler,
            SimulatedItemFlowManager itemFlowManager,
            [ImportMany(typeof(ISimulatedBehavior))] IEnumerable<Lazy<ISimulatedBehavior>> simulatedBehaviors)
        {
            _moduleBusManager = moduleBusManager;
            _factory = factory;
            _moduleRepository = moduleRepository;
            _taktManager = taktManager;
            _alarmHandler = alarmHandler;
            _itemFlowManager = itemFlowManager;
            _simulatedBehaviors = simulatedBehaviors;
        }

        public void Initialize()
        {
            CreateModuleSimulators();

            InitializeBehaviors();

            _itemFlowManager.Initialize();

            Start();
        }

        private void CreateModuleSimulators()
        {
            var originalModules = _moduleBusManager.GraphDto.Vertices;

            foreach (var originalModuleDto in originalModules)
            {
                var newModule = _factory.CreateModule() as ModuleSimulator;
                if (newModule != null)
                {
                    newModule.Initialize(10, originalModuleDto.Name);
                    _moduleRepository.AddModule(newModule);
                }
                else
                {
                    throw new ApplicationException("Cannot create the Module Simulator");
                }
            }
        }

        private void InitializeBehaviors()
        {
            foreach (var simulatedBehavior in _simulatedBehaviors)
            {
                simulatedBehavior.Value.Initialize(_moduleRepository, _alarmHandler);
            }
        }

        private void Start()
        {
            _taktManager.Start();
        }

        public void Shutdown()
        {
            if (_taktManager != null)
            {
                _taktManager.Stop();
            }
        }
    }
}
