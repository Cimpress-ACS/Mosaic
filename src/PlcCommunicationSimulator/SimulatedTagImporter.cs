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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationSimulator.Behavior;

namespace VP.FF.PT.Common.PlcCommunicationSimulator
{
    /// <summary>
    /// TagImporter mock for simulation. No real PLC required.
    /// </summary>
    [Export(typeof(ITagImporter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimulatedTagImporter : ITagImporter, IPartImportsSatisfiedNotification
    {
        [Import]
        internal ISimulatedBehaviorManagerInternal SimulatedBehaviorManager = null;

        public void Initialize(string address, int port = 0)
        {
        }

        public IReadOnlyCollection<Tag> ImportAllTags()
        {
            return ImportTags(string.Empty, -1).ToReadOnly();
        }

        public IReadOnlyCollection<Tag> ImportTags(string startTag)
        {
            return ImportTags(string.Empty, string.Empty, -1).ToReadOnly();
        }

        public ICollection<Tag> ImportTags(string path, int port = 0)
        {
            return new List<Tag>();
        }

        public ICollection<Tag> ImportTags(string startTag, string path, int port)
        {
            return new List<Tag>();
        }

        public ICollection<Tag> ImportTagsFromCache(string startTag, IEnumerable<Tag> tagList)
        {
            return new List<Tag>();
        }

        public Tag ImportTagRecursive(string scopedPath)
        {
            return ImportTag(scopedPath);
        }

        public Tag ImportTag(string name)
        {
            return new Tag("simulated_tag", "scope");
        }

        public Tag ImportTag(string name, int port)
        {
            return new Tag("simulated_tag", "scope");
        }

        public void OnImportsSatisfied()
        {
            SimulatedBehaviorManager.AddTagImporter(this);
        }

        public void Dispose()
        {
        }
    }
}
