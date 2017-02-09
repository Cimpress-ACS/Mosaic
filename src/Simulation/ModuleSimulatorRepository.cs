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
using System.Linq;
using System.Text;

namespace VP.FF.PT.Common.Simulation
{
    [Export(typeof(IModuleSimulatorRepository))]
    public class ModuleSimulatorRepository : IModuleSimulatorRepository
    {
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IModuleSimulator> _importedModules;

        private readonly List<IModuleSimulator> _modulesAddedAtRuntime = new List<IModuleSimulator>();

        public void AddModule(IModuleSimulator module)
        {
            _modulesAddedAtRuntime.Add(module);
        }

        public IModuleSimulator GetModule(string name)
        {
            IModuleSimulator foundModule = AllModules.FirstOrDefault(m => Equals(m.Name, name));
            if (foundModule == null)
                throw new SimulationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get the module with name '{0}'.", name).AppendLine()
                      .AppendLine("Unfortunately that module does not exist.").ToString());

            return foundModule;
        }

        public IEnumerable<IModuleSimulator> Modules
        {
            get { return AllModules; }
        }

        private IEnumerable<IModuleSimulator> AllModules
        {
            get { return _importedModules.Concat(_modulesAddedAtRuntime).Distinct(); }
        }
    }
}
