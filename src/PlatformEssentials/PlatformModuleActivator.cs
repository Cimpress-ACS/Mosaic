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


using System.ComponentModel.Composition;
using System.Linq;
using ILogger = VP.FF.PT.Common.Infrastructure.Logging.ILogger;

namespace VP.FF.PT.Common.PlatformEssentials
{
    [Export(typeof(IPlatformModuleActivator))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlatformModuleActivator : IPlatformModuleActivator
    {
        private readonly IPlatformModuleRepository _moduleRepository;
        private readonly ILogger _logger;


        [ImportingConstructor]
        public PlatformModuleActivator(IPlatformModuleRepository moduleRepository, ILogger logger)
        {
            _moduleRepository = moduleRepository;
            _logger = logger;
            _logger.Init(GetType());

        }

        public void ActivateModules(int streamType)
        {
            foreach (var module in _moduleRepository.Modules.Where(module => module.StreamType == streamType))
            {
                module.IsInitializedChangedEvent += OnIsInitializedChanged;

                if (module.IsInitialized)
                {
                    module.ActivateModule();
                    _logger.InfoFormat("Module {0} was activated!", module.Name);
                }
                else
                {
                    _logger.Warn("Module " + module.Name + " could not be activated!");
                }
            }
        }

        void OnIsInitializedChanged(IPlatformModule sender)
        {
            if (sender.IsInitialized)
            {
                sender.ActivateModule();
            }
        }

    }
}
