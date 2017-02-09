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
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;

namespace VP.FF.PT.Common.PlatformEssentials
{
    public delegate void ModuleStateChanged(IPlatformModule sender, PlatformModuleState newState);
    public delegate void ModulePortFullChanged(IPlatformModule sender, int inputPortIndex, bool isFull);
    public delegate void IsInitializedChanged(IPlatformModule sender);

    [InheritedExport]
    public interface IPlatformModule : IModuleRouting, ICapacity
    {
        /// <summary>
        /// Gets the alarm manager of this platform module.
        /// </summary>
        IAlarmManager AlarmManager { get; }

        /// <summary>
        /// Gets the module equipments.
        /// </summary>
        IList<IModuleEquipment> Equipments { get; }

        /// <summary>
        /// Adds a new equipment to the module.
        /// </summary>
        /// <param name="equipment">The equipment.</param>
        void AddEquipment(IModuleEquipment equipment);

        /// <summary>
        /// Gets or sets the PLC port.
        /// </summary>
        /// <value>
        /// The PLC port.
        /// </value>
        int AdsPort { get; set; }

        /// <summary>
        /// The Plc Adress of this module
        /// </summary>
        string PlcAddress { get; set; }

        /// <summary>
        /// Gets or sets the module type identifier.
        /// Multiple modules of same type are allowed.
        /// </summary>
        int ModuleTypeId { get; set; }

        /// <summary>
        /// Some platforms are organized in module groups or module stream.
        /// Gets or sets the type of the stream this module belongs to (optional).
        /// </summary>
        int StreamType { get; set; }

        /// <summary>
        /// Gets the name of the module. Must be unique because it's used as a primary key.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the full path to PLC root controller.
        /// </summary>
        /// <value>
        /// The PLC root controller.
        /// </value>
        string PathRootController { get; set; }

        /// <summary>
        /// Indicates whether a module is fully initialized or not
        /// </summary>
        bool IsInitialized { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// This is the overall module state (PLC, other HW conditions) and can be used for business logic.
        /// Every module must abstract their specific states to one of the values provided in PlatformModuleState.
        /// </summary>
        PlatformModuleState State { get; set; }

        /// <summary>
        /// Gets or sets the historical state before the state has changed.
        /// </summary>
        PlatformModuleState OldState { get; set; }

        /// <summary>
        /// Gets or sets the substate.
        /// The substate is very module specific and is used for information (UI) and debugging.
        /// </summary>
        string SubState { get; set; }

        /// <summary>
        /// Enables or disables the plc state logging
        /// </summary>
        bool EnableStateLogging { get; set; }

        /// <summary>
        /// Enables or disables the plc substate logging
        /// </summary>
        bool EnableSubStateLogging { get; set; }

        /// <summary>
        /// The # of the module. Default is 1.
        /// </summary>
        int ModuleNbr { get; set; }

        /// <summary>
        /// Occurs when state of module has changed.
        /// </summary>
        event ModuleStateChanged ModuleStateChangedEvent;

        /// <summary>
        /// Occurs when an input-port IsFull has changed.
        /// </summary>
        event ModulePortFullChanged ModulePortFullChangedEvent;

        /// <summary>
        /// Occurs when the module IsInitialized property has changed
        /// </summary>
        event IsInitializedChanged IsInitializedChangedEvent;

        /// <summary>
        /// Put critical/potential error stuff here. Do NOT use the constructor for this.
        /// The application will continue to run, even if an exception is thrown in the initialize phase, but just without the failed module (and log entries).
        /// In this case Disable() will be called.
        /// The application will crash if an exception is thrown in the constructor, so use Initialize()!
        /// </summary>
        Task Initialize();

        Task Initialize(CancellationToken token);

        /// <summary>
        ///  Creation of a module is the first step in bringing up a module.
        ///  creation, initialization of data structures and reading static configuration should be here
        ///  </summary>
        void Construct();

        /// <summary>
        /// Activates the module, it's a post-initialization.
        /// Will be called if Initialize was successful.
        /// </summary>
        void ActivateModule();

        /// <summary>
        /// Disables the module.
        /// </summary>
        void Disable();

        /// <summary>
        /// Determines whether the specified item contains item.
        /// </summary>
        /// <param name="itemId">The item.</param>
        /// <returns></returns>
        bool ContainsItem(long itemId);

        /// <summary>
        /// Removes the item and raises item count changed event.
        /// Returns the original item object.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <param name="targetModule"></param>
        void MoveItem(long itemId, IPlatformModule targetModule);

        /// <summary>
        /// Adds the item and raises item count changed event.
        /// </summary>
        /// <param name="item">The item.</param>
        void AddItem(PlatformItem item);

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="itemId">The item.</param>
        void RemoveItem(long itemId);

        /// <summary>
        /// Starts the module and brings it into a state where production can be started.
        /// </summary>
        void Start();

        /// <summary>
        /// Pauses the module in a way to quickly start it up again. The module will not process and release items in this state.
        /// </summary>
        /// <remarks>
        /// Some modules might still be able to receive and buffer items, depends on the features and behavior of the module.
        /// </remarks>
        void Standby();

        /// <summary>
        /// Stops the module and powers off everything.
        /// </summary>
        void Stop();

        /// <summary>
        /// Clears all alarms.
        /// </summary>
        void ResetAlarms();

        /// <summary>
        /// Determines whether the specified input port is full.
        /// </summary>
        /// <remarks>
        /// Each module must provide a list of input ports and whether it's full. 
        /// The CurrentItemCount and MaxCapacity isn't enough information for item flow managment.
        /// </remarks>
        /// <param name="inputPortIndex">Index of the input port.</param>
        /// <returns></returns>
        bool IsFull(int inputPortIndex);

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="itemId">The item unique identifier.</param>
        /// <returns></returns>
        PlatformItem GetItem(long itemId);

        /// <summary>
        /// Factory method to create a PlatformItem. Some modules might overload this method to create custom PlatformItems.
        /// </summary>
        /// <returns></returns>
        PlatformItem CreateNewPlatformItem();
    }
}
