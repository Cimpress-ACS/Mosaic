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

namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// Contains some MetaData for the Tag.
    /// </summary>
    public class TagMetaData
    {
        public TagMetaData()
        {
            Comment = string.Empty;
        }

        /// <summary>
        /// Gets or sets the Tag PLC code comment if PLC system is able to provide this information.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the scale factor of the Tag value.
        /// </summary>
        public double ScaleFactor { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of the Tag.
        /// In case of enum it's the minimum enum value.
        /// </summary>
        /// <value>
        /// The minimum.
        /// </value>
        public object Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the Tag.
        /// In case of enum it's the maximum enum value.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public object Maximum { get; set; }

        /// <summary>
        /// Gets or sets the unit of the Tag value for presentation in UI e.g.
        /// </summary>
        public string UnitForUser { get; set; }

        /// <summary>
        /// Gets or sets whether the value of the pointer of this tag should be included or not
        /// </summary>
        public bool ReadPointer { get; internal set; }

        /// <summary>
        /// Gets the ordered enumeration members. An enumeration value is the enum value (int) and description if available, otherwise an empty string.
        /// The ordering is according how the values where parsed from PLC.
        /// </summary>
        /// <value>
        /// The enumeration members.
        /// </value>
        public IList<EnumerationMember> EnumerationMembers { get; internal set; }
    }

    public struct EnumerationMember
    {
        public short Value { get; set; }
        public string Comment { get; set; }
    }
}
