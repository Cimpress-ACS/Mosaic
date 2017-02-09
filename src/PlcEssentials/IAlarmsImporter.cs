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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// An implementer of <see cref="IAlarmsImporter"/> is capable of 
    /// getting and observe alarms.
    /// </summary>
    public interface IAlarmsImporter
    {
        /// <summary>
        /// Notifies the subscribers about changed alarms.
        /// </summary>
        event Action<IEnumerable<Alarm>> AlarmsChanged;

        /// <summary>
        /// Initializes this alarms importer.
        /// </summary>
        /// <param name="tagListener">The tag listener to get used by this instance.</param>
        void Initialize(ITagListener tagListener);

        /// <summary>
        /// Imports all alarms from the plc.
        /// </summary>
        void ImportAlarms();

        /// <summary>
        /// Gets all alarms which were previously imported.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        IEnumerable<Alarm> GetAllImportedAlarms();

        /// <summary>
        /// Gets the alarms from the controller with the specified <paramref name="controllerId"/>.
        /// </summary>
        /// <param name="controllerId">The controller id of the desired alarms.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        IEnumerable<Alarm> GetAlarms(int controllerId);
    }
}
