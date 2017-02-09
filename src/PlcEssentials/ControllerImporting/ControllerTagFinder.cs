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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcEssentials.ControllerImporting
{
    /// <summary>
    /// The <see cref="ControllerTagFinder"/> is capable of importing
    /// controller tags.
    /// </summary>
    [Export(typeof(IFindControllerTags))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ControllerTagFinder : IFindControllerTags
    {
        private const int NamingConventionIndexOfScope = 2;

        private readonly ITagImporter _tagImporter;
        private readonly ILogger _logger;
        private ITagListener _tagListener;

        /// <summary>
        /// Initializes a new <see cref="ControllerTagFinder"/> instance.
        /// </summary>
        /// <param name="tagImporter">The tag importer to import the datasourc to find the controller in.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public ControllerTagFinder(ITagImporter tagImporter, ILogger logger)
        {
            _tagImporter = tagImporter;
            _logger = logger;
            _logger.Init(GetType());
        }

        /// <summary>
        /// Initializes the instance with the specified items.
        /// </summary>
        /// <param name="tagListener">The tag listener this instance uses to read tags.</param>
        /// <param name="address">The address under which the tags can be found.</param>
        /// <param name="port">The port under which the the tags can be found.</param>
        public void Initialize(ITagListener tagListener, string address, int port = 0)
        {
            _tagListener = tagListener;
            _tagImporter.Initialize(address, port);
        }

        /// <summary>
        /// Imports all controller tags found on the tagImporter.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IControllerTag"/> instances.</returns>
        public IReadOnlyCollection<IControllerTag> FindControllerTags()
        {
            return FindControllerTagsEnumerable().ToReadOnly();
        }

        private IEnumerable<IControllerTag> FindControllerTagsEnumerable()
        {
            if (_tagListener == null)
                yield break;
            IEnumerable<Tag> pointersToControllerTags = GetPointersToControllerTags();
            IEnumerable<Tag> controllerTags = ReadControllerTags(pointersToControllerTags);
            foreach (Tag controllerTag in controllerTags)
            {
                yield return new ControllerTagWrapper(controllerTag);
            }
        }

        private IEnumerable<Tag> GetPointersToControllerTags()
        {
            Tag globalControllerArrayTag = ImportGlobaleContorlerArrayTag();
            if (globalControllerArrayTag == null)
                yield break;

            foreach (var tag in globalControllerArrayTag.Childs)
            {
                yield return tag;
            }
        }

        private Tag ImportGlobaleContorlerArrayTag()
        {
            return _tagImporter.ImportTagRecursive(NamingConventions.Global.ControllerArray());
        }

        private IEnumerable<Tag> ReadControllerTags(IEnumerable<Tag> pointersToControllerTags)
        {
            foreach (Tag pointerToControllerTag in pointersToControllerTags)
            {
                TryReadValue(pointerToControllerTag);

                ulong address;
                                
                if (!ulong.TryParse(pointerToControllerTag.Value.ToString(), out address))
                    continue;

                if (address == 0)
                    continue;
                
                string pointerTagName = string.Format("{0}^.strMyInstancePath", pointerToControllerTag.Name);
                var tagInstancePath = new Tag(pointerTagName, NamingConventions.Global, "STRING");

                TryReadValue(tagInstancePath);

                var instancePath = tagInstancePath.Value as string;

                if (string.IsNullOrEmpty(instancePath))
                    continue;

                string scopedControllerPath = ExtractScopedControllerPath(instancePath);
                yield return _tagImporter.ImportTagRecursive(scopedControllerPath);
            }
        }

        private void TryReadValue(Tag tag)
        {
            try
            {
                _tagListener.ReadTagSynchronously(tag);
            }
            catch (PlcCommunicationException)
            {
                var message = new StringBuilder()
                    .AppendFormat("Tried to read tag '{0}' on tag listener '{1}'.", tag, _tagListener).AppendLine()
//                    .AppendFormat("The tag listener threw plc communication exception with message '{0}'",
//                        exception.Message).AppendLine()
                    .AppendLine("The controller tag finder will skip this controller array item.");
//                      .AppendLine("Exception:")
//                      .AppendLine(exception.ToString());
                _logger.ErrorFormat(message.ToString());
            }
        }

        private string ExtractScopedControllerPath(string instancePath)
        {
            // p.e. { 'TwinCAT_Device', 'LoadingStationModule', 'Main', 'fbLDS_1' }
            string[] instancePathSegments = instancePath.Split('.');
            return string.Join(".", instancePathSegments.Skip(NamingConventionIndexOfScope));
        }
    }
}
