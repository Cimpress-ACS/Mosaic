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

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// The <see cref="IHavePlcInformation"/> interface marks the implementer as
    /// information expert about a PLC.
    /// </summary>
    public interface IHavePlcInformation
    {
        /// <summary>
        /// The <see cref="ControllerTreeUpdated"/> event is raised whenever the overall information of the controller
        /// tree has changed.
        /// </summary>
        event Action<IControllerTree> ControllerTreeUpdated;

        /// <summary>
        /// Gets the name of this instance.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the controller tree of this module.
        /// </summary>
        IControllerTree ControllerTree { get; }

        /// <summary>
        /// Updates the controller tree over the plc.
        /// </summary>
        void UpdateControllerTree();

        /// <summary>
        /// Subscribes the specified <paramref name="handler"/> on the controller with the specified <paramref name="controllerId"/>.
        /// The handler gets executed whenever the controller changed a value.
        /// </summary>
        /// <param name="controllerId">The id of the controller to observe.</param>
        /// <param name="handler">The handler to get executed on a controller change.</param>
        void SubscribeForControllerChanges(int controllerId, Action<IController> handler);

        /// <summary>
        /// Unsubscribes the specified <paramref name="handler"/> from the controller with the specified <paramref name="controllerId"/>.
        /// The handler won't get executed on a change of the controller.
        /// </summary>
        /// <param name="controllerId">The id of the controller to not anymore observe.</param>
        /// <param name="handler">The handler to unsubscribe.</param>
        void UnsubsribeFromControllerChanges(int controllerId, Action<IController> handler);
    }
}
