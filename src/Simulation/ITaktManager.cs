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

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// Global simulation controller to start, stop or single step the whole simulation.
    /// </summary>
    public interface ITaktManager
    {
        /// <summary>
        /// Will start the simulation. A timer will automatically trigger takts (single steps).
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the simulation.
        /// </summary>
        void Stop();

        /// <summary>
        /// Triggers a single step (single takt) which is useful for debugging or unit tests.
        /// </summary>
        /// <returns></returns>
        void SingleStep();

        /// <summary>
        /// Gets the current takt count.
        /// </summary>
        ulong TaktCount { get; }
        
        /// <summary>
        /// Specifies the simulation speed. Default is 100ms per takt.
        /// </summary>
        /// <remarks>
        /// The value can be set to 0 for unit tests (as fast as possible).
        /// </remarks>
        TimeSpan DelayPerTakt { get; set; }
    }
}
