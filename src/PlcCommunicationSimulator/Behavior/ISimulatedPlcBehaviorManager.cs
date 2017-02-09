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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.Simulation;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    /// <summary>
    /// The SimulatedPlcBehaviorManager adds PLC behavior and provides a way to mock the hardware away.
    /// It basically listenes to Tag value changes and performs defined actions.
    /// Further it provides timed behaviors and cyclic actions.
    /// </summary>
    public interface ISimulatedPlcBehaviorManager : ITakt
    {
        /// <summary>
        /// Initializes all defined behaviors.
        /// Call this in the very end after every part has added it Tags to the TagListeners and TagControllers.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Occurs when the SimulatedPlcBehaviorManager is initialized.
        /// </summary>
        event EventHandler Initialized;

        /// <summary>
        /// Fluent syntax to define and add a new "When-Then" statement.
        /// Use this method to add behavior to the PLC simulator.
        /// </summary>
        /// <param name="tag">The tag to observer for value changes.</param>
        FluentTagConditionInterface WhenTag(Tag tag);

        /// <summary>
        /// Fluent syntax to define and add a new "When-Then" statement.
        /// Use this method to add behavior to the PLC simulator.
        /// </summary>
        /// <param name="tagFullName">Full path of the Tag.</param>
        /// <param name="port">Optional port. Will be ignored if port is 0 (in this case it would search for any tag matching the fullname).</param>
        /// <returns></returns>
        /// <remarks>
        /// Also string path to a Tag is supported, in case the real Tag object is not available or private.
        /// </remarks>
        FluentTagConditionInterface WhenTag(string tagFullName, int port = 0);

        /// <summary>
        /// Adds a periodic action which will be executed forever in a specified time interval.
        /// </summary>
        /// <param name="executeAfterTakts">The action will be executed after ever N-th takt.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>A task to have a chance to continue with simulation on a faulted state "thrown exception" (optional).</returns>
        void AddPeriodicAction(int executeAfterTakts, Action action);

        /// <summary>
        /// Adds a periodic action which will be executed forever in a specified time interval.
        /// </summary>
        /// <param name="executeAfterTakts">The action will be executed after ever N-th takt.</param>
        /// <param name="condition">Only executes the periodic action as long as the condition is true. When the condition is false it will still check periodical.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>A task to have a chance to continue with simulation on a faulted state "thrown exception" (optional).</returns>
        void AddPeriodicAction(int executeAfterTakts, Func<bool> condition, Action action);

        /// <summary>
        /// Searches the a Tag by full name (which is scope + name).
        /// It will search for all Tags in all TagListeners.
        /// </summary>
        /// <param name="tagFullName">Full path of the Tag.</param>
        /// <param name="port">Optional port. Will be ignored if port is 0 (in this case it would search for any tag matching the fullname).</param>
        /// <returns>
        /// The Tag found or null if not existing.
        /// </returns>
        Tag SearchTag(string tagFullName, int port = 0);

        /// <summary>
        /// Some Tags are temporary used in TagListener.ReadTagSynchronously. To simulate those Tags this method can be used.
        /// </summary>
        /// <param name="tagFullName">Full path of the Tag.</param>
        /// <param name="port">Optional port. Will be ignored if port is 0 (in this case it would search for any tag matching the fullname).</param>
        /// <returns>
        /// The found or created Tag.
        /// </returns>
        Tag SearchOrCreateLooseTag(string tagFullName, int port = 0);

    }
}
