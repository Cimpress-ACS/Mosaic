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
using System.ComponentModel.Composition;
using VP.FF.PT.Common.PlatformEssentials;
using VP.FF.PT.Common.Simulation.Alarms;
using VP.FF.PT.Common.Simulation.Equipment;

namespace VP.FF.PT.Common.Simulation
{
    /// <summary>
    /// The module simulator emulates a module and is an abstracted conveyor with a defined length.
    /// Each slot can hold an item and/or an equipment or can just be empty. The conveyor works on a takt-basis (it's not timer based) and moves items around per takt.
    /// If an item exceeds the last slot it would leave the module.
    /// Equipments are hardware features like sensors (DataChannel, Barcode-Reader, ...) or actors (Junctions, Stoppers, Buffers, ...).
    /// </summary>
    /// <remarks>
    /// A Mosaic module must not necessarily reflected by exactly one ModuleSimulator. For complex Saber modules multiple ModuleSimulators can be used.
    /// </remarks>
    [InheritedExport]
    public interface IModuleSimulator : ITakt, IAlarmSource
    {
        /// <summary>
        /// Initializes the module simulator for length (number of slots) and name.
        /// </summary>
        /// <param name="length">The length defines how many slots a module have. Items will be moved from one slot the the next when a takt was triggered.</param>
        /// <param name="name">The name which can be used as a primary key for searching.</param>
        void Initialize(int length, string name);

        /// <summary>
        /// Optional. Reacts on the the state changes of the specified PlatformModule and will freeze the ModuleSimulator is the Module is not in RUN state.
        /// </summary>
        /// <param name="platformModule">The platform module to subscribe events to.</param>
        void ReactOnPlatformModuleState(IPlatformModule platformModule);

        /// <summary>
        /// Adds an equipment to the module. The position is specified within the equipment.
        /// </summary>
        void AddEquipment(ISimulatedEquipment equipment);

        /// <summary>
        /// Removes and deactivates the equipment from the module and will do nothing anymore on takt.
        /// </summary>
        /// <param name="equipment">The equipment.</param>
        void RemoveEquipment(ISimulatedEquipment equipment);

        /// <summary>
        /// Gets the equipments.
        /// </summary>
        IEnumerable<ISimulatedEquipment> Equipments { get; }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The length indicated how many item slots a module has. Each slot can hold one item and an equipment (optional).
        /// If an item's position exceeds the module length it will leave the module and probably added to another module.
        /// </summary>
        /// <remarks>
        /// Note: It's 0-index-based. A module of length 10 has slot index from 0..9
        /// </remarks>
        int Length { get; }

        /// <summary>
        /// Adds an item to the module at slot index 0.
        /// </summary>
        /// <remarks>
        /// If the slot is already occupied by another item an Alarm will occur and should be handled by the Integration-Test, e.g. the test should fail or
        /// the item should be removed and marked as dump.
        /// </remarks>
        /// <param name="item">The item.</param>
        void AddItem(ISimulatedItem item);

        /// <summary>
        /// Adds an item to the module at the specified slot index position.
        /// </summary>
        /// <remarks>
        /// If the slot is already occupied by another item an Alarm will occur and should be handled by the Integration-Test, e.g. the test should fail or
        /// the item should be removed and marked as dump.
        /// </remarks>
        /// <param name="item">The item.</param>
        /// <param name="position">The index position where to add the item.</param>
        void AddItem(ISimulatedItem item, int position);

        /// <summary>
        /// Removes an item from the module.
        /// </summary>
        /// <param name="item">The item.</param>
        void RemoveItem(ISimulatedItem item);

        /// <summary>
        /// Gets the item by index position.
        /// </summary>
        /// <param name="position">The index is 0-based.</param>
        /// <returns>The item or null is the slot is empty.</returns>
        ISimulatedItem GetItemByPosition(int position);

        /// <summary>
        /// Gets the item by the item id which should be unique.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns>The item or null if not existing.</returns>
        ISimulatedItem GetItemById(ulong itemId);

        /// <summary>
        /// Gets the position of item.
        /// </summary>
        /// <param name="itemId">The item identifier.</param>
        /// <returns>The slot index of the item or -1 if not found.</returns>
        int GetPositionOfItem(ulong itemId);

        /// <summary>
        /// This RX event is raised whenever an item left the module.
        /// </summary>
        IObservable<SimulatedItemLeftModuleData> ItemLeft { get; }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        int ItemCount { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the module is active or not which means whether the module moves items around or is stopped.
        /// Default value should be true.
        /// </summary>
        bool IsActive { get; set; }
    }
}
