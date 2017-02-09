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
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;

namespace VP.FF.PT.Common.GuiEssentials
{
    /// <summary>
    /// Base implementation add a new module instance and triggers a MEF container recomposition.
    /// </summary>
    public abstract class ModuleFactoryBase : IModuleFactory
    {
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<CompositionContainer> _container = null;

        /// <summary>
        /// Creates the module (and satisfies all MEF imports!).
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the MEF ExportFactory to create new instances.
        /// </remarks>
        public IModuleScreen CreateModule()
        {
            var instance = CreateModuleInstance();

            var batch = new CompositionBatch();
            batch.AddPart(instance);

            batch.AddExportedValue(_container);

            _container.First().Compose(batch);

            return instance;
        }

        /// <summary>
        /// Creates the module instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the MEF ExportFactory to create new instances.
        /// </remarks>
        protected abstract IModuleScreen CreateModuleInstance();
    }
}
