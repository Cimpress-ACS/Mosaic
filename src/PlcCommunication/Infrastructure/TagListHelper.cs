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
using System.Reflection;

namespace VP.FF.PT.Common.PlcCommunication.Infrastructure
{
    /// <summary>
    /// Provides helper to get Tags out of a static class structure.
    /// </summary>
    public static class TagListHelper
    {
        /// <summary>
        /// Gets all Tags from a static class using reflection. Nested static classes are allowed, even for deep nesting.
        /// </summary>
        /// <param name="tagContainer">The static class which serves as a tag container.</param>
        /// <returns>List of all Tags.</returns>
        public static IList<Tag> ParseTags(Type tagContainer)
        {
            var list = new List<Tag>();

            AddTagFields(list, tagContainer);

            ParseTagsRecursively(list, tagContainer);

            return list;
        }

        private static void ParseTagsRecursively(IList<Tag> list, Type tagContainerLevel)
        {
            var nestedTypes = tagContainerLevel.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            if (nestedTypes.Length == 0)
                return;

            foreach (var type in nestedTypes)
            {
                AddTagFields(list, type);
                ParseTagsRecursively(list, type);
            }
        }

        private static void AddTagFields(IList<Tag> list, Type tagContainer)
        {
            var fields = tagContainer.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var fieldInfo in fields)
            {
                var tag = (Tag)fieldInfo.GetValue(null);

                if (tag != null)
                    list.Add(tag);
            }
        }
    }
}
