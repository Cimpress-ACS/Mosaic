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
using System.ComponentModel.Composition;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    /// <summary>
    /// Container for Tags which are not added to any TagListener.
    /// </summary>
    /// <remarks>
    /// Sometimes Tags are used temporary with TagListener.ReadTagSynchronously. To hook those Tags up for simulation this storage will help.
    /// </remarks>>
    [Export]
    public class LooseTagStorage
    {
        // unique key is: tagscope.tagname:port
        private readonly IDictionary<string, Tag> _tags = new Dictionary<string, Tag>();

        public Tag GetOrCreateTag(Tag tag)
        {
            Tag looseTag;
            
            if (!_tags.TryGetValue(tag.Scope + "." + tag.Name + ":" + tag.AdsPort, out looseTag))
            {
                looseTag = tag;
                TagHelper.SetDefaultValue(looseTag);
                _tags.Add(looseTag.Scope + "." + looseTag.Name + ":" + looseTag.AdsPort, looseTag);
            }

            return looseTag;
        }

        public Tag GetOrCreateTag(string fullName, int port)
        {
            Tag looseTag;
            _tags.TryGetValue(fullName + ":" + port, out looseTag);

            if (looseTag == null)
            {
                looseTag = new Tag(fullName, null, null, port);
                TagHelper.SetDefaultValue(looseTag);
                _tags.Add(fullName + ":" + port, looseTag);
            }

            return looseTag;
        }
    }
}
