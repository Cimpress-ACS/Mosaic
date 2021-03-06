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
using System.Linq;

namespace VP.FF.PT.Common.Simulation
{
    public class ModuleSimulatorFactory : IModuleSimulatorFactory
    {
        private readonly IList<IModuleSimulator> _createdModules = new List<IModuleSimulator>();

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<CompositionContainer> _container = null;

        [Import]
        private ExportFactory<IModuleSimulator> _exportFactory = null;

        /// <summary>
        /// Creates the module (and satisfies all MEF imports!).
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the MEF ExportFactory to create new instances.
        /// </remarks>
        public IModuleSimulator CreateModule()
        {
            var instance = CreateModuleInstance();

            _createdModules.Add(instance);

            return instance;
        }

        /// <summary>
        /// Adds all new created modules to the MEF container and triggers a recomposition (IPartImportsSatisfiedNotification will be called and satisfies ImportMany).
        /// This method should called at least once after creating modules.
        /// </summary>
        public void TriggerContainerRecomposition()
        {
            var batch = new CompositionBatch();

            foreach (var newModule in _createdModules)
            {
                batch.AddPart(newModule);
            }

            batch.AddExportedValue(_container);

            _container.First().Compose(batch);
        }

        /// <summary>
        /// Creates the module instance.
        /// </summary>
        /// <returns></returns>.
        /// <remarks>
        /// Implementations should use the MEF ExportFactory to create new instances.
        /// Otherwise a standard ModuleSimulator will be created by default.
        /// </remarks>
        protected virtual IModuleSimulator CreateModuleInstance()
        {
            return _exportFactory.CreateExport().Value;
        }
    }
}
