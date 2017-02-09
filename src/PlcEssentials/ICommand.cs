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

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Holds informations of a PLC state machine command.
    /// </summary>
    public interface ICommand : IEquatable<Impl.Command>, IComparable<Impl.Command>
    {
        /// <summary>
        /// Gets the unique command id.
        /// </summary>
        int CommandId { get; }

        /// <summary>
        /// Gets the Cmd-Values
        /// </summary>
        IEnumerable<Tag> CmdValues { get; }

        /// <summary>
        /// Gets a value indicating whether this command is available for a controller.
        /// </summary>
        /// <value>
        /// <c>true</c> if this command is available; otherwise, <c>false</c>.
        /// </value>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets the mode tag. The mode tag specifies the target where the mode should be written to.
        /// </summary>
        Tag ModeTag { get; }

        /// <summary>
        /// Gets the command tag. The command tag specifies the target where the command should be written to.
        /// </summary>
        Tag CommandTag { get; }

        /// <summary>
        /// Gets the Tag to the corresponding command that indicates whether the command is available or not.
        /// </summary>
        Tag IsAvailableTag { get; }

        /// <summary>
        /// Gets a value that indicates whether a user defined command and mode tag was specified
        /// </summary>
        bool HasCommandAndModeTag { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the descriptive comment of the command (meta data).
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Gets the controller.
        /// </summary>
        IController Controller { get; }

        /// <summary>
        /// Fires this instance.
        /// </summary>
        void Fire();
    }
}
