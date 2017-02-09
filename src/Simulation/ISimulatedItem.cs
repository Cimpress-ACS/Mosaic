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

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// Represents a placeholder for an item (or item collection) which can be added to a free slot of an ModuleSimulator.
    /// </summary>
    public interface ISimulatedItem : IEquatable<ISimulatedItem>
    {
        /// <summary>
        /// Gets the log history of this item. It includes at least all visited modules and equipments.
        /// </summary>
        string LogHistory { get; }

        /// <summary>
        /// Gets or sets the item identifier which should be unique.
        /// </summary>
        ulong ItemId { get; set; }

        /// <summary>
        /// Adds a log message to the LogHistory with timestamp.
        /// </summary>
        void AddLog(string message, string moduleName = "");

        /// <summary>
        /// Gets or sets a value indicating whether this item can move or not (frozen).
        /// </summary>
        /// <value>
        ///   If <c>true</c> this instance is frozen and will not be moved when a takt is triggered.
        /// </value>
        bool IsFrozen { get; set; }

        /// <summary>
        /// Contains a set of key-value metadata information. Useful to track actions which happened to an item, e.g. a barcode was applied.
        /// </summary>
        IDictionary<string, object> Metadata { get; }
    }
}
