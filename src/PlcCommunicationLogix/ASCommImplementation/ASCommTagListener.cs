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
using System.Linq;
using System.Reactive.Subjects;
using AutomatedSolutions.Win.Comm.AB.Logix;
using AutomatedSolutions.Win.Comm.AB.Logix.Net;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationLogix.ASCommImplementation
{
    public class ASCommTagListener : ITagListener
    {
        private readonly Subject<Tag> _subject = new Subject<Tag>();

        private bool _disposed;

        private const int DefaultRefreshRate = 200;

        private Device _device;
        private readonly Channel _channel;
        private Group _group;

        private readonly IList<Tag> _tags = new List<Tag>();

        public ASCommTagListener()
        {
            _channel = new Channel();
        }

        public ASCommTagListener(string ipAddress)
        {
            _channel = new Channel();
            _channel.Error += OnChannelError;
            _device = new Device(ipAddress);
            _device.Error += OnDeviceError;
            _group = new Group(false, DefaultRefreshRate);

            _channel.Devices.Add(_device);
            _device.Groups.Add(_group);
        }

        public string Name { get; set; }
        public event EventHandler<TagChangedEventArgs> TagChanged;
        public event EventHandler PollingEvent;
        public event EventHandler<List<Tag>> CollectedTagChanged;
        public event EventHandler<Exception> CommunicationProblemOccured;
        public event EventHandler<bool> ConnectionStateChanged;

        public bool IsConnected
        {
            get { throw new NotImplementedException(); }
        }

        public void Initialize(string ipAddress, int path = 0)
        {
            _channel.Error += OnChannelError;
            _device = new Device(ipAddress);
            _device.Error += OnDeviceError;
            _group = new Group(false, DefaultRefreshRate);

            _channel.Devices.Add(_device);
            _device.Groups.Add(_group);
        }

        public IObservable<Tag> GetTagStream()
        {
            return _subject;
        }

        public void AddTag(Tag tag)
        {
            if (_tags.Contains(tag))
                return;

            var item = new Item(tag.FullName(), 1, ASCommHelper.ParseASCommDataType(tag.DataType));

            item.Error += OnItemError;
            item.DataChanged += TagDataChanged;

            tag.ValueChanged += TagValueChanged;

            _tags.Add(tag);
            _group.Items.Add(item);
        }

        public void AddTagsRecursively(Tag rootTag)
        {
            AddTag(rootTag);

            foreach (var child in rootTag.Childs)
            {
                AddTagsRecursively(child);
            }
        }

        public void AddTags(Type tagContainer)
        {
            var tags = TagListHelper.ParseTags(tagContainer);

            foreach (var tag in tags)
            {
                AddTag(tag);
            }
        }

        public ICollection<Tag> GetTags()
        {
            return _tags;
        }

        public void AddUdtHandler<TNetType>(string plcDataType)
        {
            throw new NotImplementedException();
        }

        public void AddUdtHandler<TCipStruct>(string dataType, ConvertToCustomValueConverterFunction convertToCustomValue)
        {
            throw new NotImplementedException();
        }

        public void ReadTagSynchronously(Tag tag)
        {
            tag.LockValue();

            try
            {
                throw new NotImplementedException();
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        public void RefreshAll()
        {
            foreach (var tag in _tags)
            {
                ReadTagSynchronously(tag);
            }
        }

        public double RefreshRate
        {
            get { return _group.UpdateRate; }
            set
            {
                _group.UpdateRate = (int)value;
            }
        }

        public void RemoveTag(Tag tag)
        {
            tag.ValueChanged -= TagValueChanged;
            _tags.Remove(tag);

            // TODO: remove logix tag from _tagGroup
        }

        public void StartListening()
        {
            _group.Active = true;
        }

        public void StartListening(string address, int path)
        {
            _device = new Device(address);
            _device.Error += OnDeviceError;

            _channel.Devices.Clear();
            _channel.Devices.Add(_device);
            _device.Groups.Add(_group);
        }

        public void StopListening()
        {
            _group.Active = false;
        }

        public string AddressAndPath
        {
            get { return _device.RoutePath; }
            set { _device.RoutePath = value; }
        }

        public int Port { get; private set; }

        private void TagDataChanged(object sender, EventArgs eventArgs)
        {
            var logixTag = (Item)sender;

            var foundTags =
                from t in _tags
                where t.FullName() == logixTag.HWTagName
                select t;

            if (foundTags.Count() != 1)
            {
                return;
            }

            var tag = foundTags.First();
            if (!tag.IsActive)
                return;

            tag.LockValue();

            try
            {
                tag.Value = Enumerable.First(logixTag.Values);
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        private void OnItemError(object sender, AutomatedSolutions.Win.Comm.ItemEventArgs eventArgs)
        {
            var item = sender as Item;
            RaiseCommunicationProblem(new PlcCommunicationException("ASComm Item Error in TagListener with item following tag: " + item.HWTagName, _device.RoutePath, eventArgs.Message));
        }

        private void OnChannelError(object sender, AutomatedSolutions.Win.Comm.ChannelEventArgs eventArgs)
        {
            RaiseCommunicationProblem(new PlcCommunicationException("ASComm Channel Error in TagListener", _device.RoutePath, eventArgs.Message));
        }

        private void OnDeviceError(object sender, AutomatedSolutions.Win.Comm.DeviceEventArgs eventArgs)
        {
            RaiseCommunicationProblem(new PlcCommunicationException("ASComm Devise Error in tag TagListener", _device.RoutePath, eventArgs.Message));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopListening();
                _device.Error -= OnDeviceError;
                _device.Dispose();
                _channel.Error -= OnChannelError;
                _channel.Dispose();
                _group.Dispose();

                _disposed = true;
            }
        }

        private void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {
            _subject.OnNext(sender);

            if (TagChanged != null)
                TagChanged(this, new TagChangedEventArgs(sender));
        }

        private void RaiseCommunicationProblem(Exception e)
        {
            if (CommunicationProblemOccured != null)
                CommunicationProblemOccured(this, e);
        }
    }
}
