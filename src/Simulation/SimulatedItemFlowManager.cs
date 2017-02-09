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
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Simulation
{
    [Export]
    public class SimulatedItemFlowManager : IPartImportsSatisfiedNotification
    {
        private readonly ILogger _logger;
        private readonly IModuleSimulatorRepository _moduleRepository;
        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        [ImportingConstructor]
        public SimulatedItemFlowManager(
            ILogger logger, 
            IModuleSimulatorRepository moduleRepository,
            [Import(AllowDefault = true)] INextModuleProvider nextModuleProvider,
            [Import(AllowDefault = true)] MosaicNextModuleProvider fallbackNextModuleProvider)
        {
            _logger = logger;
            _moduleRepository = moduleRepository;

            if (nextModuleProvider == null)
                NextModuleProvider = fallbackNextModuleProvider;
            else
                NextModuleProvider = nextModuleProvider;

            _logger.Init(GetType());
        }

        public INextModuleProvider NextModuleProvider { get; private set; }

        public void OnImportsSatisfied()
        {
            Initialize();
        }

        public void Initialize()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();

            foreach (var simulatedModule in _moduleRepository.Modules)
                _subscriptions.Add(simulatedModule.ItemLeft.Subscribe(ItemLeft));
        }

        private void ItemLeft(SimulatedItemLeftModuleData data)
        {
            data.OriginModuleSimulator.RemoveItem(data.Item);
            var nextModule = NextModuleProvider.GetNextModule(data.OriginModuleSimulator);
            if (nextModule != null)
            {
                nextModule.AddItem(data.Item);

                var logMessage = "Moved item from " + data.OriginModuleSimulator.Name + " to " + nextModule.Name;
                data.Item.AddLog(logMessage, nextModule.Name);
            }
        }
    }
}
