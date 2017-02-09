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


namespace VP.FF.PT.Common.Simulation.Equipment
{
    public interface ISimulatedEquipment
    {
        /// <summary>
        /// Gets or sets a value indicating whether this equipment is active.
        /// A deactivated equipment will still receive "ItemPassed" calls but do nothing with the items.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets the slot position of the equipment within a ModuleSimulator.
        /// </summary>
        int Position { get; }

        /// <summary>
        /// Gets the counter of visited item.
        /// </summary>
        ulong ItemPassedCount { get; }

        /// <summary>
        /// Will be called when an item just passed the equipment.
        /// </summary>
        /// <param name="item">The item.</param>
        void ItemPassed(ISimulatedItem item);
    }
}
