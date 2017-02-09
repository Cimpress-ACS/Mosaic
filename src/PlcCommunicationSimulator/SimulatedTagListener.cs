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
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcCommunicationSimulator.Behavior;

namespace VP.FF.PT.Common.PlcCommunicationSimulator
{
    /// <summary>
    /// TagListener mock for simulation. No real PLC required.
    /// </summary>
    [Export(typeof(ITagListener))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("{AddressAndPath}  ItemCount:{_tags.Count}")]
    public class SimulatedTagListener : ITagListener, IPartImportsSatisfiedNotification
    {
        private readonly LooseTagStorage _looseTagStorage;
        private readonly Subject<Tag> _subject = new Subject<Tag>();
        private readonly IList<Tag> _tags = new List<Tag>();
        private readonly IDictionary<string, object> _udtHandler = new Dictionary<string, object>();

        private string _address;
        private int _port;

        [Import]
        internal ISimulatedBehaviorManagerInternal SimulatedBehaviorManager = null;

        [ImportingConstructor]
        public SimulatedTagListener(LooseTagStorage looseTagStorage)
        {
            _looseTagStorage = looseTagStorage;
            Name = "SimulatedTagListener";
        }

        public void Dispose()
        {
        }

        public string Name { get; set; }
        public event EventHandler<TagChangedEventArgs> TagChanged;
        public event EventHandler PollingEvent;
        public event EventHandler<List<Tag>> CollectedTagChanged;
        public event EventHandler<Exception> CommunicationProblemOccured { add { } remove { } }
        public event EventHandler<bool> ConnectionStateChanged { add { } remove { } }

        public bool IsConnected { get; set; }

        public void Initialize(string address, int path = 0)
        {
            _address = address;
            _port = path;
        }

        public IObservable<Tag> GetTagStream()
        {
            return _subject;
        }

        public void AddTag(Tag tag)
        {
            if (_tags.Contains(tag))
                return;

            tag.ValueChanged += TagValueChanged;

            _tags.Add(tag);
        }

        public void AddTags(Type tagContainer)
        {
            var tags = TagListHelper.ParseTags(tagContainer);

            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public void AddTagsRecursively(Tag rootTag)
        {
            AddTag(rootTag);

            foreach (var child in rootTag.Childs)
            {
                AddTagsRecursively(child);
            }
        }

        public ICollection<Tag> GetTags()
        {
            return _tags;
        }

        public void AddUdtHandler<TNetType>(string plcDataType)
        {
            if (_udtHandler.ContainsKey(plcDataType))
                return;

            _udtHandler.Add(plcDataType, typeof(TNetType));
        }

        public void AddUdtHandler<TCipStruct>(string dataType, ConvertToCustomValueConverterFunction convertToCustomValue)
        {
        }

        public void ReadTagSynchronously(Tag tag)
        {
            IsConnected = true;

            var givenTag = GetTag(tag);

            tag.Value = givenTag.Value;
        }

        private Tag GetTag(Tag tag)
        {
            var tagFromTagList = _tags.FirstOrDefault(t => t.Name == tag.Name && t.Scope == tag.Scope);

            if (tagFromTagList != null)
                return tagFromTagList;

            var tagFromLooseTagStorage = _looseTagStorage.GetOrCreateTag(tag);

            // add missing Tag metadata if needed
            if (string.IsNullOrEmpty(tagFromLooseTagStorage.DataType))
            {
                TagHelper.CopyTagMetadata(tag, tagFromLooseTagStorage);
            }

            return tagFromLooseTagStorage;
        }

        public void RefreshAll()
        {
            IsConnected = true;

            foreach (var tag in _tags)
            {
                TagHelper.SetDefaultValue(tag);
            }
        }

        public double RefreshRate { get; set; }

        /// <summary>
        /// Removes the specified <paramref name="tag"/> from observation.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        public void RemoveTag(Tag tag)
        {
            if (!_tags.Contains(tag))
                return;

            tag.ValueChanged -= TagValueChanged;

            _tags.Remove(tag);
        }

        public void StartListening()
        {
            IsConnected = true;
        }

        public void StartListening(string address, int path)
        {
            _address = address;
            _port = path;
            IsConnected = true;
        }

        public void StopListening()
        {
        }

        public string AddressAndPath
        {
            get { return _address + ":" + _port; }
            set
            {
                string[] addressAndPath = value.Split(':');
                if (addressAndPath.Length == 2)
                    _port = int.Parse(addressAndPath[1]);
                if (addressAndPath.Length > 0)
                    _address = addressAndPath[0];
                else
                    _address = value;
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
        }

        private void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {
            _subject.OnNext(sender);

            if (TagChanged != null)
                TagChanged(this, new TagChangedEventArgs(sender));
        }

        public void OnImportsSatisfied()
        {
            SimulatedBehaviorManager.AddTagListener(this);
        }

        internal void ThrowPollingEvent()
        {
            EventHandler handler = PollingEvent;

            if (handler != null)
            {
                handler(this, null);
            }
        }

    }
}
