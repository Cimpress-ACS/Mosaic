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
using System.Globalization;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using TwinCAT.Ads;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// This TagListener uses Beckhoff ADS notifiction system instead of polling.
    /// Advantages are very low response time and lower communication overhead.
    /// </summary>
    /// <remarks>
    /// But maximum number of handles are limitted by the PLC. Missing unsubscribes might lead to
    /// "handle-leaks", a PLC restart is required in this case.
    /// </remarks>
    [Export("WithAdsHandles", typeof(ITagListener))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("{AddressAndPath}  ItemCount:{_notificationHandlesByTag.Count}")]
    public class BeckhoffTagListener : ITagListener
    {
        private readonly ILogger _logger;
        private readonly Subject<Tag> _subject = new Subject<Tag>();

        private bool _disposed;
        private double _refreshRate;

        private string _adsAddress;
        private int _adsPort;

        private readonly TcAdsClient _twinCatClient = new TcAdsClient();
        
        // default polling rate for Tag value changes
        private const int DefaultRefreshRate = 100;

        // timeout in case of connection problems
        private const int Timeout = 1000;

        // maximum number of possible Beckhoff ADS handles
        private const int MaxTagCount = 500;

        // tag list for listening
        private readonly IList<Tuple<Tag, int>> _notificationHandlesByTag = new List<Tuple<Tag, int>>();

        private readonly Dictionary<string, Type> _udtHandler = new Dictionary<string, Type>();

        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffTagListener"/> class.
        /// </summary>
        public BeckhoffTagListener()
        {
            Initialize();
        }

        [ImportingConstructor]
        public BeckhoffTagListener(ILogger logger)
        {
            _logger = logger;
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffTagListener"/> class.
        /// </summary>
        /// <param name="adsAddress">The ads address.</param>
        /// <param name="adsPort">The ads port.</param>
        public BeckhoffTagListener(string adsAddress, int adsPort)
        {
            _adsAddress = adsAddress;
            _adsPort = adsPort;

            Initialize();
        }

        public string Name { get; set; }
        public event EventHandler<TagChangedEventArgs> TagChanged;
        public event EventHandler PollingEvent;
        public event EventHandler<List<Tag>> CollectedTagChanged;
        public event EventHandler<Exception> CommunicationProblemOccured;
        public event EventHandler<bool> ConnectionStateChanged;

        public bool IsConnected
        {
            get
            {
                return BeckhoffHelper.IsConnected(_twinCatClient);
            }
        }

        /// <summary>
        /// Initializes this instance to listen on the specified <paramref name="address"/> and <paramref name="path"/>.
        /// </summary>
        /// <param name="address">The address to PLC.</param>
        /// <param name="path">The path (in case of Beckhoff ads port).</param>
        public void Initialize(string address, int path = 0)
        {
            _adsAddress = address;
            _adsPort = path;
        }

        public IObservable<Tag> GetTagStream()
        {
            return _subject;
        }

        public void AddTag(Tag tag)
        {
            if (_notificationHandlesByTag.Any(t => Equals(t.Item1, tag)))
                return;

            if (tag.AdsPort == 0)
                tag.AdsPort = _adsPort;

            tag.ValueChanged += TagValueChanged;

            if ((_notificationHandlesByTag.Count + 1) >= 500)
                throw new PlcCommunicationException("Supported Tag limit of " + MaxTagCount +
                                                    " exceeded! Beckhoff does not support more ADS handles.", _adsAddress, _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty);

            // add tag at runtime
            if (_twinCatClient.IsConnected)
            {
                try
                {
                    int handle = _twinCatClient.AddDeviceNotification(tag.FullName(),
                                                                      new AdsStream(tag.BitSize == 0
                                                                                        ? 81
                                                                                        : tag.BitSize/8),
                                                                      AdsTransMode.OnChange, (int) RefreshRate, 0, tag);
                    _notificationHandlesByTag.Add(new Tuple<Tag, int>(tag, handle));
                }
                catch (AdsErrorException e)
                {
                    throw new PlcCommunicationException("Can't add Tag " + tag.FullName() + " to Listener (which is already running)", _adsAddress,
                                                        _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                }
            }
            else
            {
                _notificationHandlesByTag.Add(new Tuple<Tag, int>(tag, 0));
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
            return new List<Tag>(_notificationHandlesByTag.Select(t => t.Item1));
        }

        public void AddUdtHandler<TNetType>(string plcDataType)
        {
            if (_udtHandler.ContainsKey(plcDataType))
                return;

            _udtHandler.Add(plcDataType, typeof(TNetType));
        }

        public void AddUdtHandler<TCipStruct>(string dataType, ConvertToCustomValueConverterFunction convertToCustomValue)
        {
            // not needed for Beckhoff
            throw new NotImplementedException();
        }
        
        public void ReadTagSynchronously(Tag tag)
        {
            tag.LockValue();

            try
            {
                lock (_lock)
                {
                    if (!_twinCatClient.IsConnected)
                    {
                        try
                        {
                            _twinCatClient.Connect(_adsAddress, _adsPort);
                            _twinCatClient.Timeout = Timeout;
                        }
                        catch (AdsException e)
                        {
                            throw new PlcCommunicationException("Can't read tag synchronously. Connection to PLC failed.", _adsAddress,
                                                                _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                        }
                    } 
                }

                // try to read UDT
                if (_udtHandler.ContainsKey(tag.DataType))
                {
                    ReadUdt(tag);
                    return;
                }

                var dataStream = new AdsStream(tag.BitSize == 0 ? 81 : tag.BitSize / 8);

                try
                {
                    int handle = _twinCatClient.CreateVariableHandle(tag.FullName());

                    try
                    {
                        _twinCatClient.Read(handle, dataStream);
                    }
                    finally
                    {
                        _twinCatClient.DeleteVariableHandle(handle);
                    }
                }
                catch (AdsErrorException e)
                {
                    throw new PlcCommunicationException("Can't read tag " + tag.FullName(), _adsAddress,
                                                        _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                }

                ParseValueFromStream(tag, dataStream);
            }
            catch (Exception e)
            {
                if (_logger != null)
                    _logger.Warn(tag.FullName() + " value change leaded to a unhandled exception", e);

                var exception = new PlcCommunicationException("Can't read tag " + tag.FullName(), _adsAddress,
                                    _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                exception.Tag = tag;
                throw exception;
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        public void RefreshAll()
        {
            foreach (Tag tag in GetTags())
            {
                ReadTagSynchronously(tag);
            }
        }

        public double RefreshRate
        {
            get { return _refreshRate; }
            set
            {
                _refreshRate = value;
            }
        }

        /// <summary>
        /// Removes the specified <paramref name="tag"/> from observation.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        public void RemoveTag(Tag tag)
        {
            Tuple<Tag, int> handleByTag = _notificationHandlesByTag.FirstOrDefault(t => Equals(t.Item1, tag));
            if (handleByTag == null)
                return;

            tag.ValueChanged -= TagValueChanged;

            _notificationHandlesByTag.Remove(handleByTag);

            if (_twinCatClient.IsConnected)
            {
                _twinCatClient.DeleteDeviceNotification(handleByTag.Item2);
            }
        }

        public void StartListening()
        {
            List<Tag> tags = _notificationHandlesByTag.Select(t => t.Item1).ToList();
            string failedTags = string.Empty;

            try
            {
                _twinCatClient.Connect(_adsAddress, _adsPort);
                _twinCatClient.Timeout = Timeout;
            }
            catch (AdsErrorException)
            {
                throw new PlcCommunicationException("Can't start listening. Connection to PLC failed.", 
                                                    _adsAddress, _adsPort.ToString(CultureInfo.InvariantCulture));
            }

            AdsErrorException lastInnerException = null;
            _notificationHandlesByTag.Clear();
            foreach (Tag tag in tags)
            {
                try
                {
                    int handle = _twinCatClient.AddDeviceNotification(tag.FullName(), new AdsStream(tag.BitSize == 0 ? 81 : tag.BitSize / 8), AdsTransMode.OnChange, (int)RefreshRate, 0, tag);
                    _notificationHandlesByTag.Add(new Tuple<Tag, int>(tag, handle));
                }
                catch (AdsErrorException e)
                {
                    failedTags += tag.FullName() + "\n";
                    lastInnerException = e;
                }
            }

            if (!string.IsNullOrEmpty(failedTags))
            {
                throw new PlcCommunicationException("Can't start listening. Some tags were not found:\n" + failedTags, 
                                                    _adsAddress, _adsPort.ToString(CultureInfo.InvariantCulture), failedTags, lastInnerException);
            }
        }

        public void StartListening(string address, int port)
        {
            foreach (Tag tag in GetTags())
            {
                if (tag.AdsPort == 0)
                    tag.AdsPort = port;
            }

            _adsAddress = address;
            _adsPort = port;
            StartListening();
        }

        public void StopListening()
        {
            foreach (int notificationHandle in _notificationHandlesByTag.Select(t => t.Item2))
            {
                _twinCatClient.DeleteDeviceNotification(notificationHandle);
            }
        }

        public string AddressAndPath
        {
            get { return _adsAddress + ":" + _adsPort; }
            set 
            { 
                string[] addressAndPath = value.Split(':');
                if (addressAndPath.Length == 2)
                    _adsPort = int.Parse(addressAndPath[1]);
                if (addressAndPath.Length > 0)
                    _adsAddress = addressAndPath[0];
            }
        }

        public int Port
        {
            get
            {
                return _adsPort;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopListening();

                //_twinCatClient.Dispose();

                _disposed = true;
            }
        }

        private void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {
            _subject.OnNext(sender);

            if (TagChanged != null)
                TagChanged(this, new TagChangedEventArgs(sender));
        }

        private void Initialize()
        {
            _refreshRate = DefaultRefreshRate;
            _twinCatClient.AdsNotification += TwinCatClientOnAdsNotification;
            _twinCatClient.AdsStateChanged += TwinCatClientAdsStateChanged;
        }

        private void TwinCatClientAdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged(this, IsConnected); 
            }
        }

        private void TwinCatClientOnAdsNotification(object sender, AdsNotificationEventArgs adsNotificationEventArgs)
        {
            var tag = (Tag) adsNotificationEventArgs.UserData;

            tag.LockValue();

            try
            {
                ParseValueFromStream(tag, adsNotificationEventArgs.DataStream);
            }
            catch (Exception e)
            {
                try
                {
                    StopListening();
                    Console.WriteLine("Stopped Listener " + _adsAddress + ":" + _adsPort + " because of exception during read operation. Check the error event!");
                }
                catch (Exception)
                {
                    // must not crash the application
                }

                RaiseCommunicationProblem(e);
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        private void ReadUdt(Tag tag)
        {
            Type t = _udtHandler[tag.DataType];

            int handle = _twinCatClient.CreateVariableHandle(tag.FullName());

            object newValue;

            try
            {
                newValue = _twinCatClient.ReadAny(handle, t);
            }
            finally
            {
                _twinCatClient.DeleteVariableHandle(handle);
            }

            if (tag.Value == null)
                tag.Value = newValue;
            else if (!tag.Value.CompareObjectsOnlyFields(newValue))
                tag.Value = newValue;
        }

        private void ParseValueFromStream(Tag tag, AdsStream dataStream)
        {
            var binaryReader = new AdsBinaryReader(dataStream);
            dataStream.Position = 0;

            // try to read standard PLC type
            var value = ReadNextDataType(tag.DataType, tag.BitSize, binaryReader);

            if (value != null)
            {
                tag.Value = value;
                return;
            }

            // try to read ARRAY type
            if (StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.IsMatch(tag.DataType))
            {
                var array = ParseArrayValuesFromStream(tag, dataStream);

                if (tag.Value is object[])
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] == null)
                        {
                            continue;
                        }

                        if (!array[i].Equals(((object[])tag.Value)[i]))
                        {
                            tag.Value = array;
                            return;
                        }
                    }
                }
                else
                {
                    tag.Value = array;
                }

                return;
            }

            // fallback, could not read
            //Console.WriteLine(" BeckhoffPollingTagListener warning: Could not read " + tag.FullName() + " because of unknown type " + tag.DataType);
            tag.Value = null;
        }

        private object[] ParseArrayValuesFromStream(Tag tag, AdsStream dataStream)
        {
            Match match = StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.Match(tag.DataType);

            int lowerBound = Convert.ToInt32(match.Groups[1].Value);
            int upperBound = Convert.ToInt32(match.Groups[2].Value);
            string dataType = match.Groups[3].Value;

            var binaryReader = new AdsBinaryReader(dataStream);
            dataStream.Position = 0;

            var returnArray = new object[upperBound - lowerBound + 1];

            for (int i = 0; i < returnArray.Length; i++)
            {
                returnArray[i] = ReadNextDataType(dataType, tag.BitSize, binaryReader);
            }

            return returnArray;
        }

        private object ReadNextDataType(string dataType, int bitSize, AdsBinaryReader binaryReader)
        {
            return BeckhoffHelper.ReadDataType(dataType, bitSize, binaryReader);
        }

        private void RaiseCommunicationProblem(Exception e)
        {
            if (CommunicationProblemOccured != null)
                CommunicationProblemOccured(this, e);
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
