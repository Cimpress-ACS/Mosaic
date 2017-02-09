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

namespace VP.FF.PT.Common.PlatformEssentials
{
    public interface ICapacity
    {
        /// <summary>
        /// Occurs when item count has changed.
        /// </summary>
        event EventHandler<ItemCountChangedEventArgs> CurrentItemCountChangedEvent;

        /// <summary>
        /// Gets the current item count.
        /// </summary>
        /// <value>
        /// The current item count.
        /// </value>
        int CurrentItemCount { get; }

        int OverallItemCount { get; }

        /// <summary>
        /// Gets the maximum capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity.
        /// </value>
        int MaxCapacity { get; }

        /// <summary>
        /// Gets or sets the limit item count.
        /// </summary>
        /// <value>
        /// If value is 0 there will be no limit.
        /// </value>
        int LimitItemCount { get; set; }

        /// <summary>
        /// Gets or sets the planned throughput of this module per hour.
        /// </summary>
        /// <value>
        /// The planned throughput. Normally a supervisor sets it as a max target possible, or based on the max calculated capability of the module.
        /// </value>
        int PlannedThroughput { get; set; }
    }

    public class ItemCountChangedEventArgs : EventArgs
    {
        public ItemCountChangedEventArgs(int count)
        {
            Count = count;
        }

        public int Count { get; private set; }
    }
}
