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
    /// An implementer of <see cref="IControllerTree"/> is capable
    /// to interact with a controller tree.
    /// </summary>
    public interface IControllerTree
    {
        /// <summary>
        /// Indicates whether the <see cref="ControllerTree"/> is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// The event <see cref="AlarmsRemoved"/> is raised if old alarms were removed
        /// </summary>
        event Action<IEnumerable<Alarm>> AlarmsRemoved;

        /// <summary>
        /// The event <see cref="AlarmsAdded"/> is raised if new alarms were added
        /// </summary>
        event Action<IEnumerable<Alarm>> AlarmsAdded;

        /// <summary>
        /// The event <see cref="AlarmsChanged"/> is raised if alarms were replaced
        /// </summary>
        event Action<IEnumerable<Alarm>> AlarmsChanged;

        /// <summary>
        /// The event <see cref="ControllerTree.OverallInformationChanged"/> is raised when ever a controller in this tree
        /// changed its overall information.
        /// </summary>
        event Action OverallInformationChanged;

        /// <summary>
        /// Initializes the new <see cref="ControllerTree"/> instance.
        /// </summary>
        /// <param name="rootController">The root controller of the tree.</param>
        void Initialize(Controller rootController);

        /// <summary>
        /// Gets the root controller of the tree.
        /// </summary>
        IController RootController { get; }

        /// <summary>
        /// Tries to get the controller with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the desired controller.</param>
        /// <returns>An <see cref="IController"/> implementation or null.</returns>
        IController TryGetController(int id);

        /// <summary>
        /// Gets the controller with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of the desired controller.</param>
        /// <exception cref="InvalidOperationException">
        /// Throws an invalid operation exception if the desired controller does no exist.
        /// </exception>
        /// <returns>An <see cref="IController"/> implementation.</returns>
        IController GetController(int id);

        /// <summary>
        /// Gets all controller instances in this tree as a collection.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="IController"/> instances.</returns>
        IReadOnlyCollection<IController> GetAllControllers();
        
        /// <summary>
        /// Gets all <see cref="IAlarm"/> instances assigned to controller in this tree.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IAlarm"/> implementations.</returns>
        IEnumerable<IAlarm> GetAllAlarms();

        /// <summary>
        /// Acknowledges all alarms found on controller in this tree.
        /// </summary>
        void AcknowledgeAlarms();
        
        /// <summary>
        /// Updates the alarms in any controller. Compares the alarms of all controllers with
        /// <paramref name="currentPlcAlarms"/>. Removes alarms from controllers that doesn't exist
        /// in the <paramref name="currentPlcAlarms"/> anymore.
        /// Add alarms from <paramref name="currentPlcAlarms"/> to controllers that not already were added.
        /// </summary>
        /// <param name="currentPlcAlarms">The current pending alarms from PLC.</param>
        void UpdateAlarms(IEnumerable<Alarm> currentPlcAlarms);

        /// <summary>
        /// Gets all overall information tags of this controller tree.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        IReadOnlyCollection<Tag> GetOverallInformationTags();
    }
}
