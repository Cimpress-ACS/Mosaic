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
using System.ComponentModel.Composition.Hosting;
using System.ServiceModel;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;
using VP.FF.PT.Common.ShellBase;
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;

namespace VP.FF.PT.Common.GuiEssentials
{
    public class GuiModuleInitializer
    {
        private readonly ILogger _logger;
        private readonly IModuleScreenRepository _moduleRepository;
        private readonly IEnumerable<IConfigurationScreen> _configurationScreens;
        private readonly CompositionContainer _container;
        private readonly Dictionary<int, int> _instanceNumberPerType = new Dictionary<int, int>();
        private readonly IList<Task> _runningTasks = new List<Task>();

        private const int WcfReconnectDelay = 2000;
        private const int MaxReconnectFailCount = 6;

        public GuiModuleInitializer(ILogger logger, IModuleScreenRepository moduleRepository, IEnumerable<IConfigurationScreen> configurationScreens, CompositionContainer container)
        {
            _logger = logger;
            _moduleRepository = moduleRepository;
            _configurationScreens = configurationScreens;
            _container = container;
        }

        public void DisableModule(IModuleScreen moduleScreen)
        {
            moduleScreen.IsEnabled = false;
            _logger.WarnFormat("Module {0} has been disabled", moduleScreen.DisplayName);

            var moduleControlScreen = moduleScreen as ModuleControlScreen;
            if (moduleControlScreen != null)
                moduleControlScreen.State = "Disabled (Init failed)";
        }

        public void CreateModules()
        {
            foreach (var factory in _container.GetExportedValues<IModuleFactory>("0"))
            {
                ((ModuleScreenContainer)_moduleRepository).AddNewModule(factory.CreateModule());
            }
        }

        public void CreateModules(ModuleGraphDTO moduleGraphDto)
        {
            foreach (var platformModuleDto in moduleGraphDto.Vertices)
            {
                var module = CreateModuleByType(platformModuleDto.Type);

                if (module == null)
                    continue;

                module.ModuleKey = platformModuleDto.Name;
                module.ModuleTypeId = platformModuleDto.Type;
                module.ModuleInstance = GetNextInstancePerType(platformModuleDto.Type);

                ((ModuleScreenContainer)_moduleRepository).AddNewModule(module);
            }

            // create all other modules without a specific type
            CreateModules();
        }

        public void InitializeModules()
        {
            // initialize and populate screens 
            foreach (var moduleScreen in _moduleRepository.Modules)
            {
                Task task = InitializeModule(moduleScreen);
                _runningTasks.Add(task);
            }

            foreach (var configurationScreen in _configurationScreens)
            {
                try
                {
                    configurationScreen.Initialize();
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Can't initialize configuration screen {0}", configurationScreen.DisplayName, e);
                }
            }
        }

        private IModuleScreen CreateModuleByType(int moduleTypeId)
        {
            try
            {
                var factory = _container.GetExportedValue<IModuleFactory>(moduleTypeId.ToString());
                return factory.CreateModule();
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Can't create UI module type {0} because no matching factory was found in MEF container", moduleTypeId, e);
                return null;
            }
        }

        private int GetNextInstancePerType(int moduleTypeId)
        {
            if (!_instanceNumberPerType.ContainsKey(moduleTypeId))
            {
                _instanceNumberPerType.Add(moduleTypeId, 1);
                return 1;
            }
            return ++_instanceNumberPerType[moduleTypeId];
        }

        private async Task InitializeModule(IModuleScreen moduleScreen)
        {
            _logger.InfoFormat("Initializing shell moduleScreen {0}", moduleScreen.DisplayName);

            for (int i = 0; i < MaxReconnectFailCount; i++)
            {
                try
                {
                    await moduleScreen.Initialize();
                    moduleScreen.IsEnabled = true;
                    return;
                }
                catch (EndpointNotFoundException)
                {
                    // try initialize current moduleScreen later again
                    _logger.WarnFormat("WCF Communication endpoint problem. Try reconnect in {0} ms ...", WcfReconnectDelay);
                }
                catch (Exception e)
                {
                    _logger.ErrorFormat("Cannot initialize screen {0}", moduleScreen.DisplayName, e);
                    DisableModule(moduleScreen);
                    return;
                }

                await Task.Delay(WcfReconnectDelay);
            }

            _logger.ErrorFormat("Can't connect to Server because timeout. Failed {0} times.", MaxReconnectFailCount);
            DisableModule(moduleScreen);
        }
    }
}
