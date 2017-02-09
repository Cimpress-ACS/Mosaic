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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// Import variables using a Beckhoff global variable list (GVL) which is a XML file.
    /// </summary>
    /// <remarks>
    /// The GVL file can be exported with TwinCat 3. The export can be even executed automatically as a post build step.
    /// </remarks>
    public class BeckhoffXmlTagImporter : ITagImporter
    {
        private static readonly XNamespace Ns = "http://www.plcopen.org/xml/tc6_0200";
        private string _address;

        /// <summary>
        /// Initializes this tag importer to communicate with the plc on the specified address and port.
        /// </summary>
        /// <param name="address">The address the plc can found on.</param>
        /// <param name="port">The port in cas of Beckhoff implementation.</param>
        public void Initialize(string address, int port = 0)
        {
            _address = address;
        }

        /// <summary>
        /// Imports all tags under the preconfigured address.
        /// </summary>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        public IReadOnlyCollection<Tag> ImportAllTags()
        {
            return ImportTags(_address);
        }

        /// <summary>
        /// Imports tags recursively from the specified <paramref name="startTag"/>.
        /// </summary>
        /// <param name="startTag">The path of the tag to start the import from.</param>
        /// <returns>An <see cref="IReadOnlyCollection{T}"/> of <see cref="Tag"/> instances.</returns>
        public IReadOnlyCollection<Tag> ImportTags(string startTag)
        {
            return ImportTags(startTag, _address, -1).ToReadOnly();
        }

        /// <summary>
        /// Imports the tags.
        /// </summary>
        /// <param name="path">The connection string or path (file path or IP address depending on implementation). In case of Beckhoff this is the AdsAddress</param>
        /// <param name="port">The ADS Port in case of Beckhoff implementation.</param>
        /// <returns>
        /// Collection of imported tags.
        /// </returns>
        public ICollection<Tag> ImportTags(string path, int dummy)
        {
            var tagList = new List<Tag>();

            XElement xelement;

            try
            {
                xelement = XElement.Load(path);
            }
            catch (FileNotFoundException e)
            {
                throw new TagReaderException("Cant open file " + path, path, e);
            }
            catch (XmlException e)
            {
                throw new TagReaderException("Cant parse xml file " + path, path, e);
            }

            // read scope
            string scope;
            try
            {
                scope = xelement.Descendants(Ns + "ProjectStructure").First().Element(Ns + "Object").Attribute("Name").Value;
            }
            catch (Exception e)
            {
                throw new TagReaderException("Can't read scope in XML file. Unknown structure.", path, e);
            }

            // create tags
            try
            {
                var variables = xelement.Descendants(Ns + "variable");

                foreach (XElement xEle in variables)
                {
                    var tag = new Tag
                        {
                            Name = xEle.Attribute("name").Value,
                            Scope = scope,
                            DataType = xEle.Element(Ns + "type").Descendants().First().Name.LocalName
                        };

                    var address = xEle.Attribute("address");
                    tag.Specifier = address != null ? address.Value : null;

                    tagList.Add(tag);
                }
            }
            catch (Exception e)
            {
                throw new TagReaderException("Can't parse XML file. Unknown structure.", path, e);
            }

            return tagList;
        }

        public ICollection<Tag> ImportTags(string startTag, string path, int port)
        {
            // TODO: implement XML ImportTags subtree
            throw new NotImplementedException();
        }

        public ICollection<Tag> ImportTagsFromCache(string startTag, IEnumerable<Tag> tagList)
        {
            // TODO: implement XML ImportTags subtree
            throw new NotImplementedException();
        }

        public Tag ImportTagRecursive(string scopedPath)
        {
            throw new NotImplementedException();
        }

        public Tag ImportTag(string name)
        {
            throw new NotImplementedException();
        }

        public Tag ImportTag(string name, int Port)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
