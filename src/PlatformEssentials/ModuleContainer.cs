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

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// The <see cref="ModuleContainer"/> contains <see cref="IPlatformModule"/> instances
    /// and provides methods to find them.
    /// </summary>
    [Export(typeof(IPlatformModuleRepository))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ModuleContainer : IPlatformModuleRepository
    {
        private readonly IList<IPlatformModule> _modules = new List<IPlatformModule>();

        /// <summary>
        /// Initializes a new <see cref="ModuleContainer"/> instance. 
        /// This ctor is called by MEF, do not remove it.
        /// </summary>
        public ModuleContainer()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ModuleContainer"/> instance. This optional ctor is made for UnitTests only.
        /// </summary>
        /// <param name="modules">The modules this instance contains.</param>
        public ModuleContainer(
            IList<IPlatformModule> modules)
        {
            _modules = modules;
        }

        /// <summary>
        /// Should only be called by module factories during application startup.
        /// </summary>
        public void AddNewModule(IPlatformModule module)
        {
            _modules.Add(module);
        }

        /// <summary>
        /// Finds a <see cref="IPlatformModule"/> instance with the specified <paramref name="moduleName"/>.
        /// </summary>
        /// <param name="moduleName">The name of the desired module.</param>
        /// <returns>An <see cref="IPlatformModule"/> instance.</returns>
        public IPlatformModule GetModule(string moduleName)
        {
            IPlatformModule foundModule = _modules.FirstOrDefault(m => Equals(m.Name, moduleName));
            if (foundModule == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get the module with name '{0}'.", moduleName).AppendLine()
                      .AppendLine("Unfortunately that module does not exist.").ToString());
            return foundModule;
        }

        public IList<T> GetModulesByType<T>() where T : IPlatformModule
        {
            var foundModules = _modules.OfType<T>().ToList();
            return foundModules;
        }

        public T GetModuleByType<T>(int moduleInstance) where T : IPlatformModule
        {
            var foundModules = _modules.OfType<T>();

            var foundModule =
                (from m in foundModules
                 where m.ModuleNbr == moduleInstance
                 select m).SingleOrDefault();

            if (foundModule == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to get the module of type '{0}' and instance id '{1}'.", typeof(T), moduleInstance).AppendLine()
                      .AppendLine("Unfortunately that module does not exist.").ToString());

            return foundModule;
        }

        /// <summary>
        /// Find all <see cref="IPlatformModule"/> instances.
        /// </summary>
        /// <value>An
        ///   <see cref="IEnumerable{T}"/> of <see cref="IPlatformModule"/>
        ///   instances.</value>
        public IList<IPlatformModule> Modules
        {
            get { return _modules.ToReadOnly().ToList(); }
        }
    }
}
