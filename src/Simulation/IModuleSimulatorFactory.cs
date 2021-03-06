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

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// Creates a module and makes sure that it get's added to the MEF container.
    /// </summary>
    [InheritedExport]
    public interface IModuleSimulatorFactory
    {
        /// <summary>
        /// Creates the module (and satisfies all MEF imports!).
        /// </summary>
        /// <remarks>
        /// Implementations should use the MEF ExportFactory to create new instances.
        /// </remarks>
        IModuleSimulator CreateModule();

        /// <summary>
        /// Adds all new created modules to the MEF container and triggers a recomposition (IPartImportsSatisfiedNotification will be called and satisfies ImportMany).
        /// This method should called at least once after creating modules.
        /// </summary>
        void TriggerContainerRecomposition();
    }
}
