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
using System.Linq;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    /// <summary>
    /// Holds informations of a PLC state machine command.
    /// </summary>
    public class Command : ICommand
    {
        private readonly IController _controller;
        private PlcEssentials.Command _innerCommand;
        private IReadOnlyCollection<Tag> _childTags;
        private Tag _modeTag;
        private Tag _commandTag;

        public Command(IController associatedController)
        {
            _controller = associatedController;
        }

        public Command(IController controller, PlcEssentials.Command innerCommand, IReadOnlyCollection<Tag> childTags)
        {
            _controller = controller;
            _innerCommand = innerCommand;
            _childTags = childTags;
        }

        /// <summary>
        /// With this constructor user defined command can be created
        /// </summary>
        /// <param name="modeTag">An instance of a tag that should be used to set the mode</param>
        /// <param name="commandTag">An instance of a tag that should be used to send the command to</param>
        public Command(
            IController controller, PlcEssentials.Command innerCommand, 
            IReadOnlyCollection<Tag> childTags, Tag modeTag, Tag commandTag)
        {
            _controller = controller;
            _innerCommand = innerCommand;
            _childTags = childTags;
            _modeTag = modeTag;
            _commandTag = commandTag;
        }

        /// <summary>
        /// Gets a value that indicates whether a user defined command and mode tag was specified
        /// </summary>
        public bool HasCommandAndModeTag
        {
            get { return _commandTag != null && _modeTag != null ? true : false; }
        }

        /// <summary>
        /// Gets the unique command id.
        /// </summary>
        public int CommandId
        {
            get { return _innerCommand.CommandId; }
        }

        /// <summary>
        /// Gets the Cmd-Values
        /// </summary>
        public IEnumerable<Tag> CmdValues
        {
            get { return _childTags; }
        }

        /// <summary>
        /// Gets a value indicating whether this command is available for a controller.
        /// </summary>
        /// <value>
        /// <c>true</c> if this command is available; otherwise, <c>false</c>.
        /// </value>
        public bool IsAvailable
        {
            get { return _innerCommand.Available; }
        }
        
        /// <summary>
        /// Gets the mode tag. The mode tag specifies the target where the mode should be written to.
        /// </summary>
        public Tag ModeTag
        {
            get { return _modeTag; }
        }
        
        /// <summary>
        /// Gets the command tag. The command tag specifies the target where the command should be written to.
        /// </summary>
        public Tag CommandTag
        {
            get { return _commandTag; }
        }

        /// <summary>
        /// Gets the Tag to the corresponding command that indicates whether the command is available or not.
        /// </summary>
        public Tag IsAvailableTag
        {
            get { return _childTags.First(t => t.Name.EndsWith(NamingConventions.IsCmdAvailable)); }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return _innerCommand.Name; }
        }

        /// <summary>
        /// Gets the descriptive comment of the command (meta data).
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets the controller.
        /// </summary>
        public IController Controller
        {
            get { return _controller; }
        }

        /// <summary>
        /// Updates the command data of this command.
        /// </summary>
        /// <param name="updatedCommand">the updated data.</param>
        public void UpdateCommandData(PlcEssentials.Command updatedCommand)
        {
            _innerCommand = updatedCommand;
        }

        /// <summary>
        /// Fires this instance.
        /// </summary>
        public void Fire()
        {
            Controller.SendCommand(this);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Command other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CommandId == other.CommandId;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Command) obj);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This object is less than the <paramref name="other" /> parameter.
        /// Zero
        /// This object is equal to <paramref name="other" />.
        /// Greater than zero
        /// This object is greater than <paramref name="other" />.
        /// </returns>
        public int CompareTo(Command other)
        {
            return CommandId - other.CommandId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = CommandId;
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
