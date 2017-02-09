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

namespace VP.FF.PT.Common.PlatformEssentials.Statistic
{
    /// <summary>
    /// OEE (overall equpment efficiency) calculator for a PlatformModule.
    /// OEE = Availability * Performance * Quality.
    /// Where Availability = uptime / (uptime + downtime).
    /// Where Performance = actual output / planned output.
    /// Where Quality = (Items - reprints - ejects) / Items.
    /// </summary>
    public interface IModuleMetricMeasurement : IDisposable
    {
        event EventHandler<ModuleMetricUpdateEventArgs> MetricsUpdatedEvent;

        /// <summary>
        /// Gets or sets the refresh rate to trigger a metric recalculation.
        /// Note that a recalculation will be anyway triggered immediately after certain module events.
        /// </summary>
        TimeSpan RefreshRate { get; set; }

        /// <summary>
        /// Gets or sets the floating time window (default is 1h). The OEE calculations are done for this time window.
        /// </summary>
        TimeSpan FloatingTimeWindow { get; set; }

        /// <summary>
        /// Initializes the metric measurement for a specific module.
        /// </summary>
        /// <param name="module">The module.</param>
        void Initialize(IPlatformModule module);

        /// <summary>
        /// Resets measurement and starts from scratch.
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets the availability.
        /// </summary>
        /// <value>
        /// Where Availability = uptime / (uptime + downtime).
        /// </value>
        double Availability { get; }

        /// <summary>
        /// Gets the performance.
        /// </summary>
        /// <value>
        /// Where Performance = actual output / planned output.
        /// </value>
        double Performance { get; }

        /// <summary>
        /// Gets the quality.
        /// </summary>
        /// <value>
        /// Where Quality = (Items - reprints - ejects) / Items.
        /// </value>
        double Quality { get; }

        /// <summary>
        /// Gets the overall equipment efficiency.
        /// </summary>
        /// <value>
        /// OEE = Availability * Performance * Quality.
        /// </value>
        double OverallEquipmentEfficiency { get; }

        /// <summary>
        /// Gets up time of a module.
        /// E.g. the RUN state as well a the OFF state are considered as UpTime.
        /// </summary>
        TimeSpan UpTime { get; }

        /// <summary>
        /// Gets down time of a module, e.g. if the module is in ERROR state.
        /// </summary>
        TimeSpan DownTime { get; }

    }

    public class ModuleMetricUpdateEventArgs : EventArgs
    {
        public ModuleMetricUpdateEventArgs(double availability, double performance, double quality, double overallEquipmentEfficiency)
        {
            Availability = availability;
            Performance = performance;
            Quality = quality;
            OverallEquipmentEfficiency = overallEquipmentEfficiency;
        }

        /// <summary>
        /// Gets the availability.
        /// </summary>
        /// <value>
        /// Where Availability = uptime / (uptime + downtime).
        /// </value>
        public double Availability { get; private set; }

        /// <summary>
        /// Gets the performance.
        /// </summary>
        /// <value>
        /// Where Performance = actual output / planned output.
        /// </value>
        public double Performance { get; private set; }

        /// <summary>
        /// Gets the quality.
        /// </summary>
        /// <value>
        /// Where Quality = (Items - reprints - ejects) / Items.
        /// </value>
        public double Quality { get; private set; }

        /// <summary>
        /// Gets the overall equipment efficiency.
        /// </summary>
        /// <value>
        /// OEE = Availability * Performance * Quality.
        /// </value>
        public double OverallEquipmentEfficiency { get; private set; }
    }
}
