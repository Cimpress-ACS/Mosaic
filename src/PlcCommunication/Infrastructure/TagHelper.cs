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

namespace VP.FF.PT.Common.PlcCommunication.Infrastructure
{
    /// <summary>
    /// Bag of useful Tag helpers.
    /// </summary>
    public static class TagHelper
    {
        /// <summary>
        /// Gets the child tag.
        /// </summary>
        /// <param name="tag">The root tag to search for.</param>
        /// <param name="nestedTagName">
        /// Name of the nested tag.
        /// A nested path with . syntax is allowed, e.g. when searching for Cylinder.Info.intCtrlId.
        /// </param>
        /// <returns>The found child tag or null if child tag does not exist.</returns>
        public static Tag GetChildTag(this Tag tag, string nestedTagName)
        {
            var names = nestedTagName.Split('.');

            Tag foundTag = tag;

            foreach (string name in names)
            {
                var child =
                    from c in foundTag.Childs
                    where c.NestedName == name
                    select c;
                
                if (child.Any())
                {
                    foundTag = child.First();
                }
                else
                {
                    return null;
                }
            }

            return foundTag;
        }

        /// <summary>
        /// Searches for a tag recursively.
        /// </summary>
        /// <param name="root">The root to search for childs recursively.</param>
        /// <param name="tagName">Name of the tag to search for.</param>
        /// <returns>Since a single tag name is not unique (only the full path is unique) the result might be a collection of tags.</returns>
        public static IEnumerable<Tag> SearchChildTags(Tag root, string tagName)
        {
            var result = new List<Tag>();

            SearchChildTagsRecursively(ref result, root, tagName);

            return result;
        }

        /// <summary>
        /// Gets the PLC unit of a tag or an empty string if not available.
        /// </summary>
        /// <remarks>
        /// The PLC unit is given by a naming convention of the Tag name.
        /// </remarks>
        /// <param name="tag">The tag.</param>
        /// <returns>Unit, e.g. mm or um.</returns>
        public static string GetUnit(Tag tag)
        {
            string[] names = tag.NestedName.Split('_');

            if (names.Length > 1)
                return names.Last();

            return string.Empty;
        }

        private static void SearchChildTagsRecursively(ref List<Tag> foundTags, Tag root, string tagName)
        {
            foreach (var child in root.Childs)
            {
                if (child.NestedName == tagName)
                    foundTags.Add(child);

                if (child.Childs.Any())
                    SearchChildTagsRecursively(ref foundTags, child, tagName);
            }
        }
    }
}
