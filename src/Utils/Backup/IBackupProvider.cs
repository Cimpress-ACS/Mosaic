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


using System.Collections.Generic;

namespace VP.FF.PT.Common.Utils.Backup
{
    /// <summary>
    /// Interface for a generic backup provider
    /// </summary>
    public interface IBackupProvider
    {
        /// <summary>
        /// Initialize the backup provider structure with a list of elements (name, identifier)
        /// </summary>
        /// <param name="items">collection of items to be monitored for backup purposes</param>
        void Initialize(Dictionary<string, string> items);

        /// <summary>
        /// Initialize the backup provider with a collection of objects of tyep <see cref="IConfigItem"/>
        /// allowing additional information to be taken into account (e.g. SaveAtStart)
        /// </summary>
        /// <param name="itemsToMonitor">collection of items to be monitored for backup purposes</param>
        void Initialize(IEnumerable<IConfigItem> itemsToMonitor);

        /// <summary>
        /// Add a single item to be monitored
        /// </summary>
        /// <param name="item">item to be monitored</param>
        /// <param name="identifier">identifier</param>
        void Add(string item, string identifier = null);

        /// <summary>
        /// Add a single item to be monitored (additional information available through IConfigItem 
        /// interface
        /// </summary>
        /// <param name="item">item to be monitored</param>
        void Add(IConfigItem item);

        /// <summary>
        /// Remove item to be monitored
        /// </summary>
        /// <param name="item">key</param>
        void Remove(string item);

        /// <summary>
        /// Start monitoring process
        /// </summary>
        void StartMonitoring();

        /// <summary>
        /// Stop monitoring process
        /// </summary>
        void StopMonitoring();

        /// <summary>
        /// True if the backup provider is actively monitoring
        /// </summary>
        bool IsMonitoring { get; }
    }
}
