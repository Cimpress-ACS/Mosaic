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
using VP.FF.PT.Common.Simulation.Alarms;

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// Use this interface to hook into the simulation API to add custom behavior to ModuleSimulators (equipments etc.).
    /// </summary>
    [InheritedExport]
    public interface ISimulatedBehavior
    {
        /// <summary>
        /// Will be called as soon as the simulations is fully initialized which means all ModuleSimulators and the MosaicNextModuleProvider were created.
        /// At this point the simulation is not running though because this call is part of the initialization.
        /// </summary>
        /// <param name="moduleSimulatorRepository">
        /// The module simulator repository can be used to get a specific module to add more equipment and behavior to it.
        /// The level of detail is not defined and depends on the kind of simulation.</param>
        /// <param name="simulationAlarmHandler">
        /// The simulation alarm handler handles behavior how the simulation should react on certain alarms.
        /// </param>
        void Initialize(IModuleSimulatorRepository moduleSimulatorRepository, SimulationAlarmHandler simulationAlarmHandler);
    }
}
