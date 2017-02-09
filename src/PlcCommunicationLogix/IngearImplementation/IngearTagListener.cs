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
using System.Globalization;
using System.Reactive.Subjects;
using Logix;
using VP.FF.PT.Common.PlcCommunication;
using System.Linq;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using Tag = VP.FF.PT.Common.PlcCommunication.Tag;

namespace VP.FF.PT.Common.PlcCommunicationLogix.IngearImplementation
{
    [Export(typeof(ITagListener))]
    public class IngearTagListener : ITagListener
    {
        private readonly Subject<Tag> _subject = new Subject<Tag>();

        private bool _disposed;

        // default polling rate for Tag value changes
        private const int DefaultRefreshRate = 200;

        // timeout in case of connection problems
        private const int Timeout = 3000;

        // tag list for listening
        private readonly IList<Tag> _tags = new List<Tag>();

        private readonly Controller _controlLogix = new Controller();
        private readonly TagGroup _tagGroup = new TagGroup();
        private readonly PeerMessage _peerMessage = new PeerMessage();

        private readonly Dictionary<string, ConvertToCustomValueConverterFunction> _tagValueConverters = 
            new Dictionary<string, ConvertToCustomValueConverterFunction>();
        private readonly Dictionary<string, Type> _cipStructTypes = new Dictionary<string, Type>();

        // helper for UDT conversion
        private readonly DTEncoding _encoding = new DTEncoding();

        public IngearTagListener()
        {
            Initialize();
        }

        public IngearTagListener(string ipAddress, string path = "0")
        {
            Initialize();

            _controlLogix.IPAddress = ipAddress;
            _controlLogix.Path = path;
        }

        public event EventHandler<TagChangedEventArgs> TagChanged;
        public event EventHandler PollingEvent;
        public event EventHandler<List<Tag>> CollectedTagChanged;
        public event EventHandler<Exception> CommunicationProblemOccured;
        public event EventHandler<bool> ConnectionStateChanged;

        public string Name { get; set; }
        
        public bool IsConnected
        {
            get { return _controlLogix.IsConnected; }
        }

        /// <summary>
        /// Initializes this instance to listen on the specified <paramref name="ipAddress"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="ipAddress">The address to PLC.</param>
        /// <param name="path">The path (in case of Beckhoff ads port).</param>
        public void Initialize(string ipAddress, int path = 0)
        {
            _controlLogix.IPAddress = ipAddress;
            _controlLogix.Path = path.ToString(CultureInfo.InvariantCulture);
        }

        public IObservable<Tag> GetTagStream()
        {
            return _subject;
        }

        public void AddTag(Tag tag)
        {
            if (_tags.Contains(tag))
                return;

            var logixTag = new Logix.Tag(_controlLogix)
                               {
                                   Name = tag.FullName(), 
                                   DataType = IngearHelper.ParseNetLogixDataType(tag.DataType)
                               };

            logixTag.Changed += TagDataChanged;

            tag.ValueChanged += TagValueChanged;

            _tags.Add(tag);
            _tagGroup.AddTag(logixTag);
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

        public void RemoveTag(Tag tag)
        {
            tag.ValueChanged -= TagValueChanged;
            _tags.Remove(tag);

            // TODO: remove logix tag from _tagGroup
        }

        public ICollection<Tag> GetTags()
        {
            return _tags;
        }

        public void AddUdtHandler<NetType>(string plcDataType)
        {
            throw new NotImplementedException();
        }

        public void AddUdtHandler<TCipStruct>(string dataType, ConvertToCustomValueConverterFunction convertToCustomValue)
        {
            _tagValueConverters.Add(dataType, convertToCustomValue);
            _cipStructTypes.Add(dataType, typeof(TCipStruct));
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
            get { return _tagGroup.Interval; }
            set
            {
                _tagGroup.Interval = (int)value;
            }
        }

        public void StartListening()
        {
            if (string.IsNullOrEmpty(_controlLogix.IPAddress))
            {
                throw new PlcCommunicationException(
                    "IngearTagListener cannot start listening to ControlLogix because the IpAddress was not set!",
                    string.Empty, string.Empty);
            }

            var result = _controlLogix.Connect();
            if (result != ResultCode.E_SUCCESS)
            {
                throw new PlcCommunicationException("Can't connect to ControlLogix.", 
                    _controlLogix.IPAddress, _controlLogix.Path, result.ToString());
            }

            _tagGroup.ScanStart();

            // TODO: port is already in use very often. Fix it!
            // TODO: do we need CIP messages? because we go to beckhoff PLC systems...
            //try
            //{
            //    _peerMessage.Listen();
            //}
            //catch (Exception e)
            //{
            //    throw new PlcCommunicationException(
            //        "IngearTagListener cannot start listening peerMessage listener (CIP messages). Maybe the port is already used by another process!",
            //        _controlLogix.IPAddress, e);
            //}
        }

        public void StartListening(string address, int path)
        {
            _controlLogix.IPAddress = address;
            _controlLogix.Path = path.ToString(CultureInfo.InvariantCulture);
            StartListening();
        }

        public void StopListening()
        {
            _tagGroup.ScanStop();
            _controlLogix.Disconnect();

            //_peerMessage.ShutDown();
        }

        public string AddressAndPath 
        { 
            get { return _controlLogix.IPAddress + ":" + _controlLogix.Path; }
            set
            {
                string[] addressAndPath = value.Split(':');
                if (addressAndPath.Length == 2)
                    _controlLogix.Path = addressAndPath[1];
                if (addressAndPath.Length > 0)
                    _controlLogix.IPAddress = addressAndPath[0];
            }
        }

        public int Port { get; private set; }

        public void ReadTagSynchronously(Tag tag)
        {
            tag.LockValue();

            try
            {
                var logixTag = GetLogixTag(tag.FullName());

                _controlLogix.ReadTag(new[] {logixTag});

                // handle special DataTypes like UDTs
                object convertedValue;
                if (TryConvertToCustomValue(tag, logixTag, out convertedValue))
                {
                    tag.Value = convertedValue;
                }
                else
                {
                    tag.Value = logixTag.Value;
                }

                if (TagChanged != null)
                    TagChanged(this, new TagChangedEventArgs(tag));
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopListening();

                _disposed = true;
            }
        }

        private void TagDataChanged(object sender, EventArgs e)
        {
            var logixTag = (Logix.Tag) sender;

            var tag = GetTag(logixTag.Name);
            if (tag == null || !tag.IsActive)
                return;

            tag.LockValue();
            try
            {
                // handle special DataTypes like UDTs
                object convertedValue;
                if (TryConvertToCustomValue(tag, logixTag, out convertedValue))
                {
                    tag.Value = convertedValue;
                }
                else
                {
                    tag.Value = logixTag.Value;
                }
            }
            catch (Exception exception)
            {
                try
                {
                    StopListening();
                    Console.WriteLine("Stopped Listener " + _controlLogix.IPAddress + ":" + _controlLogix.Path + " because of exception during read operation. Check the error event!");
                }
                catch (Exception)
                {
                    // must not crash application
                }

                RaiseCommunicationProblem(exception);
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        private bool TryConvertToCustomValue(Tag tag, Logix.Tag logixTag, out object convertedValue)
        {
            ConvertToCustomValueConverterFunction convertFunction;
            if (_tagValueConverters != null && _tagValueConverters.TryGetValue(tag.DataType, out convertFunction))
            {
                try
                {
                    var cipStruct = _encoding.ToType(logixTag, _cipStructTypes[tag.DataType]);
                    convertedValue = convertFunction(cipStruct);
                }
                catch(Exception exception)
                {
                    throw new PlcCommunicationException("Error occured when converting INGEAR Tag value into " + tag.DataType, _controlLogix.IPAddress, exception);
                }

                return true;
            }

            convertedValue = null;
            return false;
        }

        private void PeerMessageOnReceived(object sender, EventArgs eventArgs)
        {
            var message = eventArgs as MessageEventArgs;
            var tag = GetTag(message.ItemName);

            if (tag == null)
                return;

            // handle special DataTypes like UDTs
            ConvertToCustomValueConverterFunction convertFunction;
            if (tag.DataType != null && _tagValueConverters != null && _tagValueConverters.TryGetValue(tag.DataType, out convertFunction))
            {
                try
                {
                    var cipStruct = _encoding.ToType((byte[]) message.Value, _cipStructTypes[tag.DataType]);
                    tag.Value = convertFunction(cipStruct);
                }
                catch(Exception exception)
                {
                    throw new PlcCommunicationException("Error occured when converting INGEAR message value into " + tag.DataType, _controlLogix.IPAddress, exception);
                }
            }
            else
            {
                tag.Value = message.Value;
            }

            if (TagChanged != null)
                TagChanged(this, new TagChangedEventArgs(tag));
        }

        private Tag GetTag(string tagName)
        {
            var foundTags =
            from t in _tags
            where t.FullName() == tagName 
            select t;

            if (foundTags.Count() != 1)
                return null;

            return foundTags.First();
        }

        private Logix.Tag GetLogixTag(string tagName)
        {
            foreach (Logix.Tag logixTag in _tagGroup.Tags)
            {
                if (logixTag.Name == tagName)
                    return logixTag;
            }

            return null;
        }

        private void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {
            _subject.OnNext(sender);

            if (TagChanged != null)
                TagChanged(this, new TagChangedEventArgs(sender));
        }

        private void Initialize()
        {
            RefreshRate = DefaultRefreshRate;
            _controlLogix.Timeout = Timeout;
            
            _tagGroup.Controller = _controlLogix;
            _tagGroup.ScanningMode = TagGroup.SCANMODE.ReadOnly;

            _peerMessage.Connections = 40;
            _peerMessage.IPAddressNIC = ""; // listen to any local NIC on our machine
            _peerMessage.Received += PeerMessageOnReceived;
        }

        private void RaiseCommunicationProblem(Exception e)
        {
            if (CommunicationProblemOccured != null)
                CommunicationProblemOccured(this, e);
        }
    }
}
