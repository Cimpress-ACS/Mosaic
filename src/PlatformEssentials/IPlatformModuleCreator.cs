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


using VP.FF.PT.Common.PlatformEssentials.ItemFlow;

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// Module initializer helper which creates a stream-graphs and ModuleBusManagers.
    /// </summary>
    public interface IPlatformModuleCreator
    {
        /// <summary>
        /// Creates all the modules found on the specified <paramref name="streamType"/>.
        /// </summary>
        void ConstructModules(int streamType);

        /// <summary>
        /// Gets or sets a value indicating whether all modules are initialized.
        /// </summary>

        bool AllModulesCreated { get; }


        /// <summary>
        /// Gets the graph where Vertices are modules and Edges are routes from one module to the next module.
        /// </summary>
        ModuleGraph Graph { get; }
    }
}
