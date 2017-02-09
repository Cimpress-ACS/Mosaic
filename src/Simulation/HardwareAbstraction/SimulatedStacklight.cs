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
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials;
using VP.FF.PT.Common.PlatformEssentials.HardwareAbstraction;

namespace VP.FF.PT.Common.Simulation.HardwareAbstraction
{
    /// <summary>
    /// This fakes a IStackLight interface for mosaic, don't mismatch it with the simulated equipment.
    /// </summary>
    [Export("simulation", typeof(IStackLight))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimulatedStacklight : ModuleEquipment, IStackLight
    {
        private readonly ILogger _logger;

        [ImportingConstructor]
        public SimulatedStacklight(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            EquipmentName = GetType().Name;
        }

        public void IndicateRun()
        {
            _logger.Debug("IndicateRun called");
        }

        public void IndicateStandby()
        {
            _logger.Debug("IndicateStandby called");
        }

        public void IndicateWarning()
        {
            _logger.Debug("IndicateWarning called");
        }

        public void IndicateError()
        {
            _logger.Debug("IndicateError called");
        }

        public void ControlLamp(StacklightColor color, StacklightPattern pattern)
        {
            _logger.DebugFormat("ControlLamp called with color {0} and pattern {1}", color, pattern);
        }
    }
}
