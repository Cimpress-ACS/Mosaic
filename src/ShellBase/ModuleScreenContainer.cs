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
using System.Linq;
using System.Text;
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;

namespace VP.FF.PT.Common.ShellBase
{
    [Export(typeof(IModuleScreenRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ModuleScreenContainer : IModuleScreenRepository
    {
        private readonly IList<IModuleScreen> _modules = new List<IModuleScreen>();

        /// <summary>
        /// Should only be called by module factories during application startup.
        /// </summary>
        public void AddNewModule(IModuleScreen module)
        {
            _modules.Add(module);
        }

        public IModuleScreen GetModule(string moduleKey)
        {
            IModuleScreen foundModule = _modules.FirstOrDefault(m => Equals(m.ModuleKey, moduleKey));
            if (foundModule == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get the module with name '{0}'.", moduleKey).AppendLine()
                      .AppendLine("Unfortunately that module does not exist.").ToString());
            return foundModule;
        }

        public IList<T> GetModulesByType<T>()
        {
            var foundModules = _modules.OfType<T>().ToList();
            return foundModules;
        }

        public T GetModuleByType<T>(int moduleInstance) where T : IModuleScreen
        {
            var foundModules = _modules.OfType<T>();

            var foundModule =
                (from m in foundModules
                 where m.ModuleInstance == moduleInstance
                 select m).SingleOrDefault();

            if (foundModule == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get the module of type '{0}' and instance id '{1}'.", typeof(T), moduleInstance).AppendLine()
                      .AppendLine("Unfortunately that module does not exist.").ToString());

            return foundModule;
        }

        public IList<IModuleScreen> Modules
        {
            get { return _modules.ToReadOnly().ToList(); }
        }
    }
}
