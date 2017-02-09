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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials
{
    [Export(typeof(IPlatformModuleInitializer))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PlatformModuleInitializer : IPlatformModuleInitializer
    {
        private const int UndefinedStreamType = 0;
        private int _streamType = UndefinedStreamType;

        private bool _allModulesInitialized;
        private readonly IPlatformModuleRepository _moduleRepository;
        private readonly ILogger _logger;
        private const int Timeout = 30;

        /// <summary>
        /// Initializes a new <see cref="PlatformModuleInitializer"/> instance.
        /// </summary>
        /// <param name="moduleRepository">An instance of the global module repository. The initializer is responsible to populate this container.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public PlatformModuleInitializer(IPlatformModuleRepository moduleRepository, ILogger logger)
        {
            _moduleRepository = moduleRepository;
            _logger = logger;
            _logger.Init(GetType());
            ModuleInitializationStarted += n => { };
        }

        /// <summary>
        /// Notifies the subscribers that the module initialization has started.
        /// </summary>
        public event Action<string> ModuleInitializationStarted;

        /// <summary>
        /// Initializes all the modules found on the specified <paramref name="stream"/>.
        /// </summary>
        public void Initialize(int stream)
        {
            _streamType = stream;
            InitializeModules();
        }

        public bool AllModulesInitialized
        {
            get
            {
                if (_allModulesInitialized)
                    return true;

                if (_moduleRepository.Modules.Any(module => module.State == PlatformModuleState.NotInitialized ||
                                                            module.State == PlatformModuleState.Undefined))
                {
                    return false;
                }

                _allModulesInitialized = true;
                return true;
            }
        }

        private void InitializeModules()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(Timeout));

            var initModules = (from m in _moduleRepository.Modules where m.StreamType == (int)_streamType select InitializeModule(m, cts.Token)).ToArray();

            try
            {
                Task.WaitAll(initModules, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.Error("Error while initializing modules", ex);
            }
        }

        private async Task InitializeModule(IPlatformModule module, CancellationToken token)
        {
            if (module.State != PlatformModuleState.NotInitialized)
            {
                _logger.ErrorFormat("Module {0} cannot be initialized because it is in state '{1}', expected NotInitialized", module.Name, module.State);
                return;
            }

            try
            {
                _logger.InfoFormat("Initializing module {0} ", module.Name);
                ModuleInitializationStarted(module.Name);
                await module.Initialize(token);
                module.IsInitialized = true;

                if (module.State != PlatformModuleState.NotInitialized)
                    module.State = PlatformModuleState.Initialized;
            }
            catch (Exception e)
            {
                module.Disable();
                _logger.Error("Can't initialize module " + module.Name + ".", e);
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0}#{1}", GetType().Name, GetHashCode());
        }
    }
}
