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
using System.Linq;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;

namespace VP.FF.PT.Common.PlcEssentials.ControllerImporting
{
    /// <summary>
    /// The <see cref="ControllerTagWrapper"/> wraps around a <see cref="PlcCommunication.Tag"/> instance
    /// pointing to a controller.
    /// </summary>
    public class ControllerTagWrapper : IControllerTag
    {
        private readonly Tag _tag;

        /// <summary>
        /// Initializes a new <see cref="ControllerTagWrapper"/> instance.
        /// </summary>
        /// <param name="tag">The controller tag to wrap around.s</param>
        public ControllerTagWrapper(Tag tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Gets the inner controller tag this instance
        /// navigates from.
        /// </summary>
        public Tag Tag
        {
            get { return _tag; }
        }

        /// <summary>
        /// Gets the common interface tag and returns it. 
        /// </summary>
        /// <returns>A <see cref="PlcCommunication.Tag"/> instance.</returns>
        public Tag GetCommonInterfaceTag()
        {
            return _tag.GetChildTagUnderPath(NamingConventions.CommonInterface);
        }

        /// <summary>
        /// Gets the state tag and returns it.
        /// </summary>
        /// <returns>A <see cref="VP.FF.PT.Common.PlcCommunication.Tag"/> instance.</returns>
        public Tag GetStateTag()
        {
            return _tag.GetChildTagUnderPath(TagName.CommonInterface(parent: null).Job().State());
        }

        /// <summary>
        /// Gets the sub state tag and returns it.
        /// </summary>
        /// <returns>A <see cref="VP.FF.PT.Common.PlcCommunication.Tag"/> instance.</returns>
        public Tag GetSubStateTag()
        {
            return _tag.GetChildTagUnderPath(TagName.CommonInterface(parent: null).Job().SubState());
        }

        /// <summary>
        /// Gets the mode tag and returns it.
        /// </summary>
        /// <returns>A <see cref="VP.FF.PT.Common.PlcCommunication.Tag"/> instance.</returns>
        public Tag GetModeTag()
        {
            return _tag.GetChildTagUnderPath(TagName.CommonInterface(parent: null).Job().Mode());
        }

        /// <summary>
        /// Gets the common interface data type from.
        /// </summary>
        /// <returns>A string instance.</returns>
        public string GetCommonInterfaceDataType()
        {
            Tag commonInterfaceTag = GetCommonInterfaceTag();
            if (commonInterfaceTag == null)
                return string.Empty;
            string commonInterfaceDataType = commonInterfaceTag.DataType;
            if (commonInterfaceDataType == null)
                return string.Empty;
            return commonInterfaceDataType;
        }

        /// <summary>
        /// Gets the common interface data value from a navigable tag.
        /// </summary>
        /// <returns>A <see cref="CtrlCommonInterface"/> instance.</returns>
        public CtrlCommonInterface GetCommonInterfaceValue()
        {
            Tag commonInterfaceTag = GetCommonInterfaceTag();
            if (commonInterfaceTag == null)
                return null;
            return commonInterfaceTag.Value as CtrlCommonInterface;
        }

        /// <summary>
        /// Gets the parents controller id.
        /// </summary>
        /// <returns>A <see cref="long"/> value.</returns>
        public short GetParentControllerId()
        {
            const short defaultValue = -1;
            Tag commonInterfaceTag = GetCommonInterfaceTag();
            if (commonInterfaceTag == null)
                return defaultValue;
            var commonInterfaceValue = commonInterfaceTag.Value as CtrlCommonInterface;
            if (commonInterfaceValue == null)
                return defaultValue;
            return commonInterfaceValue.Info.CtrlIdOfParent;
        }

        public short GetControllerId()
        {
            const short defaultValue = -1;
            Tag commonInterfaceTag = GetCommonInterfaceTag();
            if (commonInterfaceTag == null)
                return defaultValue;
            var commonInterfaceValue = commonInterfaceTag.Value as CtrlCommonInterface;
            if (commonInterfaceValue == null)
                return defaultValue;
            return commonInterfaceValue.Info.CtrlId;
        }

        /// <summary>
        /// Gets the path to the controller tag with scope.
        /// </summary>
        /// <returns>A <see cref="string"/> instance.</returns>
        public string GetScopedPath()
        {
            return string.Format("{0}.{1}", _tag.Scope, _tag.Name);
        }

        /// <summary>
        /// Gets the path to the controller tag.
        /// </summary>
        /// <returns>A <see cref="string"/> instance.</returns>
        public string GetPath()
        {
            return _tag.Name;
        }

        /// <summary>
        /// Gets the common interface job tag and returns it. 
        /// </summary>
        /// <returns>A <see cref="PlcCommunication.Tag"/> instance.</returns>
        public Tag GetCommonInterfaceJobTag()
        {
            string jobTagName = String.Format("{0}.{1}", NamingConventions.CommonInterface, NamingConventions.CommonInterfaceJob);
            return _tag.GetChildTagUnderPath(jobTagName);
        }

        /// <summary>
        /// Gets the commands under the controller tag.
        /// </summary>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="Command"/> instances.</returns>
        public IEnumerable<Command> GetCommands()
        {
            CtrlCommonInterface commonInterfaceValue = GetCommonInterfaceValue();
            if (commonInterfaceValue == null)
                yield break;
            CtrlJob job = commonInterfaceValue.Job;
            if (job == null)
                yield break;
            foreach (Command command in job.Commands().Where(c => !c.Name.IsNullOrEmpty()))
                yield return command;
        }
    }
}
