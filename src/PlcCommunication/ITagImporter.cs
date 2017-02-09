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

namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// Imports tags.
    /// </summary>
    public interface ITagImporter : IDisposable
    {
        /// <summary>
        /// Initializes this tag importer to communicate with the plc on the specified address and port.
        /// </summary>
        /// <param name="address">The address the plc can found on.</param>
        /// <param name="port">The port in cas of Beckhoff implementation.</param>
        void Initialize(string address, int port = 0);

        /// <summary>
        /// Imports all tags under the preconfigured address.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        IReadOnlyCollection<Tag> ImportAllTags();

        /// <summary>
        /// Imports tags recursively from the specified <paramref name="startTag"/>.
        /// </summary>
        /// <param name="startTag">The path of the tag to start the import from.</param>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        IReadOnlyCollection<Tag> ImportTags(string startTag);
            
        /// <summary>
        /// Imports the tags.
        /// </summary>
        /// <param name="path">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        /// <returns>
        /// Collection of imported tags.
        /// </returns>
        ICollection<Tag> ImportTags(string path, int port);

        /// <summary>
        /// Imports the tags recursively from startTag.
        /// </summary>
        /// <param name="startTag">The startTag address indicates the start position for import.</param>
        /// <param name="path">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        /// <returns>
        /// Collection of imported tags.
        /// </returns>
        ICollection<Tag> ImportTags(string startTag, string path, int port);

        /// <summary>
        /// Search for the startTag in the passed tagList. There is no access needed to plc.
        /// </summary>
        /// <param name="startTag">The startTag address indicates the start position for import.</param>
        /// <param name="tagList">The list which is searched for the startTag</param>
        /// <returns>
        /// Collection of imported tags.
        /// </returns>
        ICollection<Tag> ImportTagsFromCache(string startTag, IEnumerable<Tag> tagList);

        /// <summary>
        /// Imports a tag with all its children recursively.
        /// </summary>
        /// <param name="scopedPath">The path to the tag with its scope.</param>
        /// <returns>A <see cref="Tag"/> instance.</returns>
        Tag ImportTagRecursive(string scopedPath);

        /// <summary>
        /// Imports just a single tag.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        Tag ImportTag(string name);

        /// <summary>
        /// Imports just a single tag.
        /// </summary>
        /// <param name="name">The tag address.</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        /// <returns></returns>
        Tag ImportTag(string name, int port);
    }
}
