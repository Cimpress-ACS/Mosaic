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
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    /// <summary>
    /// Interface for creating alarms about downstream module being unavailable.
    /// </summary>
    public interface IDownstreamUnavailableAlarms
    {
        /// <summary>
        /// Get an alarm with a message saying that a module was stopped
        /// because its downstream module is unavailable.
        /// </summary>
        /// <remarks>
        /// Only one alarm object is created per module. Multiple calls with the
        /// same module will return the same instance.
        /// The default message is Stopped {0} because {1} is not available.
        /// {0} = module
        /// {1} = downstream module (will be determined automatically)
        /// </remarks>
        /// <param name="module">The affected module</param>
        /// <returns>The alarm</returns>
        Alarm GetAlarm(IPlatformModule module);

        /// <summary>
        /// Get an alarm with a message saying that a module was stopped
        /// because its downstream module is unavailable.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="errorMessageFunc">The custom error message function.</param>
        /// <returns>
        /// The alarm
        /// </returns>
        /// <remarks>
        /// Only one alarm object is created per module. Multiple calls with the
        /// same module will return the same instance.
        /// </remarks>
        Alarm GetAlarm(IPlatformModule module, Func<string> errorMessageFunc);
    }
}
