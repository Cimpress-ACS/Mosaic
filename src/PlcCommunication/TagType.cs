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


namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// Specifies the tag type.
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// Normal tag type. Default value.
        /// </summary>
        Tag,

        /// <summary>
        /// An alias is just a reference to another alias or tag.
        /// </summary>
        /// <remarks>
        /// In RSLogix the DataType is empty in this case (it's the DataType of the target element) and the
        /// Specifier contains the target element address.
        /// </remarks>
        Alias,

        /// <summary>
        /// A special type which is not really a Tag. Therefore it will be ignored by the importer.
        /// </summary>
        /// <remarks>
        /// This is only available for Rockwell PLC systems.
        /// </remarks>
        Comment
    }
}
