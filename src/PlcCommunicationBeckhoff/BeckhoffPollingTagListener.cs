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
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    /// <summary>
    /// This listener implementation does not use the ADS notification functionality but an own polling timer to read the values regularly.
    /// Thats just a workaround because Beckhoff ADS notification seems to be buggy....
    /// Further the amount of maximum ADS handles is limited to 500. A polling listener does not have this limitation.
    /// </summary>
    [Export(typeof(ITagListener))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("{AddressAndPath}  ItemCount:{_tagsList.Count}")]
    public class BeckhoffPollingTagListener : ITagListener
    {
        // default polling rate for Tag value changes
        private const int DefaultRefreshRate = 200;

        // timeout in case of connection problems
        private const int Timeout = 3000;

        private readonly object _enqueueTaskLock = new object();
        private readonly object _lock = new object();

        private readonly Subject<Tag> _subject = new Subject<Tag>();
        private readonly Dictionary<Tag, object> _cachedValues = new Dictionary<Tag, object>();
        private readonly List<Tag> _tagsList = new List<Tag>();
        private readonly Dictionary<string, Type> _udtHandler = new Dictionary<string, Type>();
        private readonly TcAdsClient _twinCatClient = new TcAdsClient();
        private readonly TcAdsClient _twinCatClientBackground = new TcAdsClient();
        private readonly ILogger _logger;

        private IReadOnlyCollection<Tag> _tagsSnapshot;
        private bool _disposed;
        private double _refreshRate;
        private string _adsAddress;
        private int _adsPort;
        private readonly IGlobalLock _globalLock;
        private PollingManagerForTagListener _pollingManagerForTagListener;
        private Task _latestAddressCacheTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffTagListener"/> class.
        /// </summary>
        public BeckhoffPollingTagListener()
        {
            _tagsSnapshot = _tagsList.ToReadOnly();
            _logger = new NullLogger();
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffTagListener" /> class.
        /// </summary>
        /// <param name="adsAddress">The ads address.</param>
        /// <param name="adsPort">The ads port.</param>
        /// <param name="globalLock">The global lock is a singleton and hold a global semaphor needed for TagListener synchronization.</param>
        /// <param name="logger">The logger.</param>
        public BeckhoffPollingTagListener(string adsAddress, int adsPort, IGlobalLock globalLock, ILogger logger = null)
        {
            _adsAddress = adsAddress;
            _adsPort = adsPort;
            _globalLock = globalLock;
            _logger = logger ?? new NullLogger();
            _tagsSnapshot = _tagsList.ToReadOnly();
            Initialize();
        }

        [ImportingConstructor]
        public BeckhoffPollingTagListener(ILogger logger, IGlobalLock globalLock)
        {
            _logger = logger ?? new NullLogger();
            _globalLock = globalLock;
            _tagsSnapshot = _tagsList.ToReadOnly();
            Initialize();
        }

        private void Initialize()
        {
            _logger.Init(GetType());

            _refreshRate = DefaultRefreshRate;
            _pollingManagerForTagListener = new PollingManagerForTagListener(this, _twinCatClient, DefaultRefreshRate, _globalLock, _logger);
            _twinCatClient.AdsStateChanged += TwinCatClientAdsStateChanged;
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

        public event EventHandler<TagChangedEventArgs> TagChanged;
        public event EventHandler PollingEvent;
        public event EventHandler<List<Tag>> CollectedTagChanged;
        public event EventHandler<Exception> CommunicationProblemOccured;
        public event EventHandler<bool> ConnectionStateChanged;

        public string Name { get; set; }

        public bool IsConnected
        {
            get { return BeckhoffHelper.IsConnected(_twinCatClient); }
        }

        public double RefreshRate
        {
            get { return _refreshRate; }
            set
            {
                _pollingManagerForTagListener.PollingRate = (int)value;
                _refreshRate = value;
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
            get { return _adsPort; }
        }

        public IObservable<Tag> GetTagStream()
        {
            return _subject;
        }

        public void AddTag(Tag tag)
        {
            lock (_tagsList)
            {
                if (_tagsList.Contains(tag))
                    return;

                if (tag.AdsPort == 0)
                    tag.AdsPort = _adsPort;

                tag.ValueChanged += TagValueChanged;

            
                _tagsList.Add(tag);
                _tagsSnapshot = _tagsList.ToReadOnly();
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
            return _tagsSnapshot.ToArray();
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
                object readValue;
                if (ReadValueSynchronously(tag, out readValue))
                    tag.Value = readValue;
            }
            finally
            {
                tag.ReleaseValue();
            }
        }

        private bool ReadValueSynchronously(Tag tag, out object newValue)
        {
            lock (_lock)
            {
                try
                {
                    Connect();
                }
                catch (AdsErrorException e)
                {
                    var exception = new PlcCommunicationException("Can't read tag synchronously. Connection to PLC failed.", _adsAddress,
                                                        _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                    exception.Tag = tag;
                    throw exception;
                }
            }
            try
            {
                // try to read UDT
                if (_udtHandler.ContainsKey(tag.DataType))
                {
                    return ReadUdt(tag, out newValue);
                }

                using (var dataStream = new AdsStream(tag.BitSize == 0 ? 81 : tag.BitSize / 8))
                {
                    Address address = GetPlcAddress(tag);

                    try
                    {
                        if (address == null)
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
                        else
                            _twinCatClient.Read(address.IndexGroup, address.IndexOffset, dataStream);
                    }
                    catch (AdsErrorException ex)
                    {
                        _logger.Warn("Couldn't read " + tag.FullName() + " because " + ex.ErrorCode.ToString() + " will try again...");

                        // try again
                        try
                        {
                            if (address == null)
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
                            else
                                _twinCatClient.Read(address.IndexGroup, address.IndexOffset, dataStream);

                            _logger.Info("Second try for " + tag.FullName() + " was successful");
                        }
                        catch (AdsErrorException e)
                        {
                            var exception = new PlcCommunicationException("Can't read tag " + tag.FullName(), _adsAddress,
                                _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                            exception.Tag = tag;
                            throw exception;
                        }
                    }

                    newValue = ParseValueFromStream(tag, dataStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Warn(tag.FullName() + " value change leaded to a unhandled exception", e);

                var exception = new PlcCommunicationException("Can't read tag " + tag.FullName(), _adsAddress,
                                    _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                exception.Tag = tag;
                throw exception;
            }
        }

        public void RefreshAll()
        {
            foreach (Tag tag in _tagsSnapshot)
                ReadTagSynchronously(tag);
        }

        private Object thisLock = new object();

        internal void AssignValuesAndPumpEvents()
        {
            lock (thisLock)
            {
                List<Tag> changedTags = new List<Tag>();

                foreach (KeyValuePair<Tag, object> valueCache in _cachedValues)
                {
                    Tag tagToAssign = valueCache.Key;
                    tagToAssign.Value = valueCache.Value;

                    if (tagToAssign.ValueHasChanged)
                    {
                        changedTags.Add(tagToAssign);
                    }
                }

                if (changedTags.Count > 0)
                {
                    if (CollectedTagChanged != null)
                    {
                        CollectedTagChanged(this, changedTags);
                    }
                }
            }
        }

        internal void ThrowPollingEvent()
        {
            EventHandler handler = PollingEvent;

            if (handler != null)
            {
                handler(this,null);
            }
        }

        internal void RefreshAllForPollingManager()
        {
            _cachedValues.Clear();
            Tag lastHandledTag = null;

            try
            {
                foreach (Tag tag in _tagsSnapshot)
                {
                    if (!tag.IsActive)
                        continue;

                    lastHandledTag = tag;

                    object value;
                    if (ReadValueSynchronously(tag, out value))
                        _cachedValues.Add(tag, value);
                }
            }
            catch (Exception e)
            {
                RaiseCommunicationProblem(e);

                var exception = new PlcCommunicationException("Can't read tag " + lastHandledTag.FullName(), _adsAddress,
                                    _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                exception.Tag = lastHandledTag;
                throw exception;
            }
        }

        public void StartListening()
        {
            try
            {
                Connect();
            }
            catch (AdsErrorException)
            {
                throw new PlcCommunicationException("Can't start listening. Connection to PLC failed.",
                                                    _adsAddress, _adsPort.ToString(CultureInfo.InvariantCulture));
            }

            _pollingManagerForTagListener.Start();
        }

        /// <summary>
        /// Removes the specified <paramref name="tag"/> from observation.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        public void RemoveTag(Tag tag)
        {
            lock (_tagsList)
            {
                tag.ValueChanged -= TagValueChanged;
                _tagsList.Remove(tag);
                _tagsSnapshot = _tagsList.ToReadOnly();
            }
        }

        public void StartListening(string address, int port)
        {
            foreach (Tag tag in _tagsSnapshot)
                if (tag.AdsPort == 0)
                    tag.AdsPort = port;

            _adsAddress = address;
            _adsPort = port;
            StartListening();
        }

        public void StopListening()
        {
            _pollingManagerForTagListener.Stop();
        }

        /// <summary>
        /// In case a PLC download occured the TagListener should clear all cached addresses because it might be that the new PLC programm has different tag addresses!
        /// </summary>
        internal void ClearAddressCache()
        {
            foreach (var tag in _tagsSnapshot)
            {
                // skip tags where address is not available anyway
                if (tag.IndexGroup != -1)
                {
                    tag.IndexGroup = 0;
                    tag.IndexOffset = 0;
                }
            }
        }

        private void TagValueChanged(Tag sender, TagValueChangedEventArgs e)
        {
            _subject.OnNext(sender);

            if (TagChanged != null)
                TagChanged(this, new TagChangedEventArgs(sender));
        }

        private void TwinCatClientAdsStateChanged(object sender, AdsStateChangedEventArgs e)
        {
            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged(this, IsConnected);
            }
        }

        private void EnqueueAddressCacheAction(Action action)
        {
            lock (_enqueueTaskLock)
            {
                if (_latestAddressCacheTask == null || _latestAddressCacheTask.Status == TaskStatus.RanToCompletion)
                    _latestAddressCacheTask = Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);
                else
                    _latestAddressCacheTask = _latestAddressCacheTask.ContinueWith(tsk => action());
            }
        }

        private bool ReadUdt(Tag tag, out object newValue)
        {
            Type type = _udtHandler[tag.DataType];

            // try to read ARRAY type of UDTs
            if (StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.IsMatch(tag.DataType))
            {
                Match match = StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.Match(tag.DataType);

                int lowerBound = Convert.ToInt32(match.Groups[1].Value);
                int upperBound = Convert.ToInt32(match.Groups[2].Value);

                var arrayLength = upperBound - lowerBound;
                newValue = _twinCatClient.ParseArray(tag.FullName(), arrayLength, type, tag.ArrayCreator(), tag.StopReadingArrayPositions);
				return true;
            }

            // try to read UDT
            Address addressCacheItem = GetPlcAddress(tag);

            if (addressCacheItem == null)
            {
                int handle = _twinCatClient.CreateVariableHandle(tag.FullName());

                try
                {
                    newValue = _twinCatClient.ReadAny(handle, type);
                }
                finally
                {
                    _twinCatClient.DeleteVariableHandle(handle);
                }

                if (tag.Value == null)
                    return true;
                if (!tag.Value.CompareObjectsOnlyFields(newValue))
                    return true;
                return false;
            }
            newValue = _twinCatClient.ReadAny(addressCacheItem.IndexGroup, addressCacheItem.IndexOffset, type);

            if (tag.Value == null)
                return true;
            if (!tag.Value.CompareObjectsOnlyFields(newValue))
                return true;
            return false;
        }

        private object ParseValueFromStream(Tag tag, AdsStream dataStream)
        {
            var binaryReader = new AdsBinaryReader(dataStream);
            dataStream.Position = 0;

            // try to read standard PLC type
            var value = ReadNextDataType(tag.DataType, tag.BitSize, binaryReader);

            if (value != null)
            {
                return value;
            }

            // try to read ARRAY type
            if (StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.IsMatch(tag.DataType))
            {
                var array = ParseArrayValuesFromStream(tag, dataStream);

                var existingValues = tag.Value as object[];
                if (existingValues != null)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] == null)
                        {
                            continue;
                        }

                        if (!array[i].Equals(existingValues[i]))
                        {
                            return array;
                        }
                    }

                    return existingValues;
                }
                else
                {
                    return array;
                }
            }

            return null;
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

        /// <summary>
        /// Search for an address for a Tag and update index group and offset in Tag.
        /// </summary>
        /// <returns>Indicates whether an address was found or not.</returns>
        private Address GetPlcAddress(Tag tag)
        {
            if (tag.IndexGroup == -1)
                return null;

            if (tag.IndexGroup == 0)
            {
                // prevent duplicate cache action
                tag.IndexGroup = -1;
                tag.IndexOffset = -1;

                EnqueueAddressCacheAction(() =>
                {
                    var symbolLoader = _twinCatClientBackground.CreateSymbolInfoLoader();

                    TcAdsSymbolInfo symbol = null;
                    try
                    {
                        symbol = symbolLoader.FindSymbol(tag.FullName());
                    }
                    catch (AdsErrorException)
                    {
                    }

                    if (symbol != null)
                    {
                        tag.IndexGroup = symbol.IndexGroup;
                        tag.IndexOffset = symbol.IndexOffset;
                    }
                    else
                    {
                        tag.IndexGroup = -1;
                        tag.IndexOffset = -1;
                    }
                });

                return null;
            }

            return new Address(tag.IndexGroup, tag.IndexOffset);
        }

        private void Connect()
        {
            if (_twinCatClient.IsConnected) 
                return;

            _twinCatClient.Connect(_adsAddress, _adsPort);
            _twinCatClientBackground.Connect(_adsAddress, _adsPort);
            _twinCatClient.Timeout = Timeout;
            _twinCatClientBackground.Timeout = Timeout;

            while (_twinCatClient.ReadState().AdsState != AdsState.Run)
            {
                _logger.Warn("TagListener still not connected. Wait for ADS state RUN");
                Thread.Sleep(100);
            }
        }

        private void RaiseCommunicationProblem(Exception e)
        {
            if (CommunicationProblemOccured != null)
                CommunicationProblemOccured(this, e);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!_disposed)
            {
                StopListening();

                //_twinCatClient.Dispose();

                _disposed = true;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("BeckhoffPollingTagListener for plc '{0}:{1}'", _adsAddress, _adsPort);
        }

        private class Address
        {
            private readonly long _indexGroup, _indexOffset;

            public Address(long indexGroup, long indexOffset)
            {
                _indexGroup = indexGroup;
                _indexOffset = indexOffset;
            }

            public long IndexGroup
            {
                get { return _indexGroup; }
            }

            public long IndexOffset
            {
                get { return _indexOffset; }
            }
        }
    }
}
