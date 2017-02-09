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
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials.ControllerImporting
{
    /// <summary>
    /// An implementer of <see cref="IControllerTag"/> is capable
    /// of navigation to some members and child members from a controller tag.
    /// </summary>
    public interface IControllerTag
    {
        /// <summary>
        /// Gets the inner controller tag this instance
        /// navigates from.
        /// </summary>
        Tag Tag { get; }

        /// <summary>
        /// Evaluates the common interface tag and returns it. 
        /// </summary>
        /// <returns>A <see cref="PlcCommunication.Tag"/> instance.</returns>
        Tag GetCommonInterfaceTag();

        /// <summary>
        /// Gets the state tag and returns it.
        /// </summary>
        /// <returns>A <see cref="VP.FF.PT.Common.PlcCommunication.Tag"/> instance.</returns>
        Tag GetStateTag();

        /// <summary>
        /// Gets the sub state tag and returns it.
        /// </summary>
        /// <returns>A <see cref="VP.FF.PT.Common.PlcCommunication.Tag"/> instance.</returns>
        Tag GetSubStateTag();

        /// <summary>
        /// Gets the mode tag and returns it.
        /// </summary>
        /// <returns>A <see cref="VP.FF.PT.Common.PlcCommunication.Tag"/> instance.</returns>
        Tag GetModeTag();

        /// <summary>
        /// Evaluates the common interface data type from a navigable tag.
        /// </summary>
        /// <returns>A string instance.</returns>
        string GetCommonInterfaceDataType();

        /// <summary>
        /// Evaluates the common interface data value from a navigable tag.
        /// </summary>
        /// <returns>A <see cref="CtrlCommonInterface"/> instance.</returns>
        CtrlCommonInterface GetCommonInterfaceValue();

        /// <summary>
        /// Evaluate the parent id.
        /// </summary>
        /// <returns>A <see cref="long"/> value.</returns>
        short GetParentControllerId();

        /// <summary>
        /// Gets the controller identifier.
        /// </summary>
        /// <returns></returns>
        short GetControllerId();

        /// <summary>
        /// Evaluates the path to the controller tag with scope.
        /// </summary>
        /// <returns>A <see cref="string"/> instance.</returns>
        string GetScopedPath();

        /// <summary>
        /// Evaluates the path to the controller tag.
        /// </summary>
        /// <returns>A <see cref="string"/> instance.</returns>
        string GetPath();

        /// <summary>
        /// Evaluates the commands under the controller tag.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Command"/> instances.</returns>
        IEnumerable<Command> GetCommands();
    }
}
