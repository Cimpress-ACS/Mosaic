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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwinCAT.Ads;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff
{
    [Export(typeof(ITagController))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [DebuggerDisplay("IsConnected={IsConnected}")]
    public class BeckhoffTagController : ITagController
    {
        private readonly object _enqueueTaskLock = new object();
        private readonly ILogger _logger;
        private string _adsAddress;
        private int _adsPort;
        private bool _disposed;

        private Task _latestAddressCacheTask;

        private class Address
        {
            public long IndexGroup;
            public long IndexOffset;
        }

        // timeout in case of connection problems
        private const int Timeout = 2000;

        private readonly TcAdsClient _twinCatClient = new TcAdsClient();
        private readonly TcAdsClient _twinCatClientBackground = new TcAdsClient();

        private readonly IPlatformScheduler _writeTaskScheduler = new SequentialScheduler();

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffTagController"/> class.
        /// </summary>
        public BeckhoffTagController()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeckhoffTagController" /> class.
        /// </summary>
        /// <param name="adsAddress">The ads address.</param>
        /// <param name="adsPort">The ads port.</param>
        /// <param name="logger">The logger.</param>
        public BeckhoffTagController(string adsAddress, int adsPort, ILogger logger = null)
        {
            _adsAddress = adsAddress;
            _adsPort = adsPort;
            _logger = logger;
            
            if (_logger != null)
                _logger.Init(GetType());
        }

        [ImportingConstructor]
        public BeckhoffTagController(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _twinCatClient.Dispose();
                    _twinCatClientBackground.Dispose();
                }
                catch (InvalidOperationException e)
                {
                    if (!Debugger.IsAttached)
                        throw e;
                }

                _disposed = true;
            }
        }

        public void AddUdtHandler(string dataType, ConvertToLogixTagValueFunction convertToLogixTagValue)
        {
            throw new NotImplementedException("Not needed for Beckhoff");
        }

        public void StartConnection()
        {
            try
            {
                Connect();
            }
            catch (AdsErrorException e)
            {
                throw new PlcCommunicationException("Can't start connection to PLC", _adsAddress,
                                                    _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
            }
        }

        public void StartConnection(string address, int port)
        {
            _adsAddress = address;
            _adsPort = port;
            Connect();
        }

        public bool IsConnected
        {
            get
            {
                return BeckhoffHelper.IsConnected(_twinCatClient);
            }
        }

        public Task WriteTag(Tag tag, object value)
        {
            if (tag.Value == null && value == null)
                return Task.FromResult(true);

            var task = _writeTaskScheduler.Schedule(() =>
            {
                tag.LockValue();

                try
                {
                    tag.DetectDataTypeForValue(value);

                    if (!IsConnected)
                    {
                        throw new PlcCommunicationException(
                            "BeckhoffTagController can't write tag because there is no connection established!",
                            _adsAddress, _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty);
                    }

                    var address = GetPlcAddress(tag);
                    var dataStream = new AdsStream(tag.BitSize == 0 ? 81 : tag.BitSize / 8);

                    try
                    {
                        if (address == null)
                        {
                            int handle = _twinCatClient.CreateVariableHandle(tag.FullName());
                            var isDataStream = WriteToStream(dataStream, tag, value);

                            try
                            {
                                if (isDataStream)
                                    _twinCatClient.Write(handle, dataStream);
                                else
                                    _twinCatClient.WriteAny(handle, value);
                            }
                            finally
                            {
                                _twinCatClient.DeleteVariableHandle(handle);
                            }
                        }
                        else
                        {
                            var isDataStream = WriteToStream(dataStream, tag, value);
                            if (isDataStream)
                                _twinCatClient.Write(address.IndexGroup, address.IndexOffset, dataStream);
                            else
                                _twinCatClient.WriteAny(address.IndexGroup, address.IndexOffset, value);
                        }
                    }
                    catch (AdsErrorException ex)
                    {
                        if (_logger != null)
                            _logger.Warn("Couldn't write " + tag.FullName() + " because " + ex.ErrorCode.ToString() + " will try again...");

                        try
                        {
                            // try again
                            if (address == null)
                            {
                                int handle = _twinCatClient.CreateVariableHandle(tag.FullName());
                                var isDataStream = WriteToStream(dataStream, tag, value);

                                try
                                {
                                    if (isDataStream)
                                        _twinCatClient.Write(handle, dataStream);
                                    else
                                        _twinCatClient.WriteAny(handle, value);
                                }
                                finally
                                {
                                    _twinCatClient.DeleteVariableHandle(handle);
                                }
                            }
                            else
                            {
                                var isDataStream = WriteToStream(dataStream, tag, value);
                                if (isDataStream)
                                    _twinCatClient.Write(address.IndexGroup, address.IndexOffset, dataStream);
                                else
                                    _twinCatClient.WriteAny(address.IndexGroup, address.IndexOffset, value);
                            }
                        }
                        catch (AdsErrorException e)
                        {
                            throw new PlcCommunicationException("Can't write tag " + tag.FullName() + " on port " + _adsPort.ToString(CultureInfo.InvariantCulture),
                                _adsPort.ToString(CultureInfo.InvariantCulture),
                                _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                        }
                    }
                }
                finally
                {
                    tag.ReleaseValue();
                }

                // at this point we assume the value was written successfully to the PLC
                Type type;
                if (IEC61131_3_DataTypes.NetDataTypes.TryGetValue(tag.DataType, out type))
                {
                    tag.Value = Convert.ChangeType(value, type);
                }
                else
                {
                    tag.Value = value;
                }

            });

            return task;
        }

        public Task WriteTag(Tag tag, object[] values)
        {
            if (tag.Value == null && values == null)
                return Task.FromResult(true);

            var task = _writeTaskScheduler.Schedule(() =>
            {
                tag.LockValue();

                try
                {
                    if (!IsConnected)
                    {
                        throw new PlcCommunicationException(
                            "BeckhoffTagController can't write tag because there is no connection established!",
                            _adsAddress, _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty);
                    }

                    if (!StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.IsMatch(tag.DataType))
                        throw new TagException(
                            "Can't write .NET array because the Tag has no array DataType! DataType is " +
                            tag.DataType, tag);

                    Match match = StructuredTextSyntaxRegexHelper.ArrayDataTypeRegex.Match(tag.DataType);

                    int lowerBound = Convert.ToInt32(match.Groups[1].Value);
                    int upperBound = Convert.ToInt32(match.Groups[2].Value);
                    //string dataType = match.Groups[3].Value;

                    if (values.Length != upperBound - lowerBound + 1)
                    {
                        throw new TagException(
                            "Can't write .NET array because dimension mismatch! .NET array length is " +
                            values.Length +
                            " but tag array length is " + (upperBound - lowerBound + 1), tag);
                    }

                    try
                    {
                        var dataStream = new AdsStream(tag.BitSize/8);

                        foreach (var value in values)
                        {
                            WriteToStream(dataStream, tag, value);
                        }

                        int handle = _twinCatClient.CreateVariableHandle(tag.FullName());

                        try
                        {
                            _twinCatClient.Write(handle, dataStream);
                        }
                        finally
                        {
                            _twinCatClient.DeleteVariableHandle(handle);
                        }
                    }
                    catch (AdsErrorException e)
                    {
                        throw new PlcCommunicationException("Can't write array for tag " + tag.FullName(),
                            _adsAddress,
                            _adsPort.ToString(CultureInfo.InvariantCulture), string.Empty, e);
                    }
                    catch (InvalidCastException e)
                    {
                        throw new TagException(
                            "Can't write array for tag " + tag.FullName() +
                            " because DataType of at least one array value does not match", tag, e);
                    }
                }
                finally
                {
                    tag.ReleaseValue();
                }

                // at this point we assume the value was written successfully to the PLC
                tag.Value = values;
            });

            return task;
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

                // this seems to be very risky. we experienced strange behavior, DataChannels doesnt work sometimes...
                //EnqueueAddressCacheAction(() =>
                //{
                //    var symbolLoader = _twinCatClientBackground.CreateSymbolInfoLoader();

                //    TcAdsSymbolInfo symbol = null;
                //    try
                //    {
                //        symbol = symbolLoader.FindSymbol(tag.FullName());
                //    }
                //    catch (AdsErrorException)
                //    {
                //    }

                //    if (symbol != null)
                //    {
                //        tag.IndexGroup = symbol.IndexGroup;
                //        tag.IndexOffset = symbol.IndexOffset;
                //    }
                //    else
                //    {
                //        tag.IndexGroup = -1;
                //        tag.IndexOffset = -1;
                //    }
                //});

                return null;
            }

            return new Address
            {
                IndexGroup = tag.IndexGroup,
                IndexOffset = tag.IndexOffset
            };
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

        private bool WriteToStream(AdsStream dataStream, Tag tag, object value)
        {
            var binaryWriter = new AdsBinaryWriter(dataStream);

            switch (tag.DataType)
            {
                case IEC61131_3_DataTypes.Boolean:
                    if (value is string)
                        value = Boolean.Parse((string)value);

                    binaryWriter.Write((bool)value);
                    break;
                case IEC61131_3_DataTypes.Byte:
                    binaryWriter.Write((byte)value);
                    break;
                case IEC61131_3_DataTypes.SInt:
                    binaryWriter.Write(Convert.ToSByte(value));
                    break;
                case IEC61131_3_DataTypes.USInt:
                    binaryWriter.Write(Convert.ToByte(value));
                    break;
                case IEC61131_3_DataTypes.UInt:
                    binaryWriter.Write(Convert.ToUInt16(value));
                    break;
                case IEC61131_3_DataTypes.Int:
                    binaryWriter.Write(Convert.ToInt16(value));
                    break;
                case IEC61131_3_DataTypes.DInt:
                    binaryWriter.Write((int)value);
                    break;
                case IEC61131_3_DataTypes.LInt:
                    binaryWriter.Write(Convert.ToInt64(value));
                    break;
                case IEC61131_3_DataTypes.Real:
                    binaryWriter.Write((float)value);
                    break;
                case IEC61131_3_DataTypes.LReal:
                    binaryWriter.Write((double)value);
                    break;
                case IEC61131_3_DataTypes.Word:
                    binaryWriter.Write(Convert.ToUInt16(value));
                    break;
                case IEC61131_3_DataTypes.DWord:
                    binaryWriter.Write(Convert.ToUInt32(value));
                    break;
                case IEC61131_3_DataTypes.LWord:
                    binaryWriter.Write(Convert.ToUInt64(value));
                    break;
                case IEC61131_3_DataTypes.ULInt:
                    binaryWriter.Write(Convert.ToUInt64(value));
                    break;
                case IEC61131_3_DataTypes.UDInt:
                    binaryWriter.Write(Convert.ToUInt32(value));
                    break;
                default:
                    // handle STRINT DataType
                    if (StructuredTextSyntaxRegexHelper.StringDataTypeRegex.IsMatch(tag.DataType))
                    {
                        var tmpString = (string)value;
                        var length = tag.BitSize / 8;

                        if (tmpString.Length > length)
                            throw new TagException("string is exceeds max defined lenght of " + length, tag);

                        binaryWriter.WritePlcString((string)value, (int)dataStream.Length);
                        break;
                    }

                    if (BeckhoffHelper.IsEnum(tag.DataType, tag.BitSize))
                    {
                        binaryWriter.Write(Convert.ToInt16(value));
                        break;
                    }

                    switch (tag.BitSize)
                    {
                        case 1:
                            if (value is string)
                                value = Boolean.Parse((string)value);

                            binaryWriter.Write((bool)value);
                            break;
                        case 8:
                            binaryWriter.Write(Convert.ToByte(value));
                            break;
                        case 16:
                            binaryWriter.Write(Convert.ToUInt16(value));
                            break;
                        case 32:
                            binaryWriter.Write(Convert.ToUInt32(value));
                            break;
                        case 64:
                            binaryWriter.Write(Convert.ToUInt64(value));
                            break;

                        default:
                            return false;
                    }
                    break;
            }

            return true;
        }

        private void Connect()
        {
            try
            {
                _twinCatClient.Connect(_adsAddress, _adsPort);
                _twinCatClientBackground.Connect(_adsAddress, _adsPort);
                _twinCatClient.Timeout = Timeout;
                _twinCatClientBackground.Timeout = Timeout;

                while (_twinCatClient.ReadState().AdsState != AdsState.Run)
                {
                    Thread.Sleep(1000);
                    if (_logger != null)
                        _logger.Warn("TagController still not connected to " + _adsAddress + ":" + _adsPort + " . Wait for ADS state RUN");
                }
            }
            catch (AdsException adsException)
            {
                var message = new StringBuilder().AppendLine()
                    .AppendFormat("Tried to connect over twin cat client to '{0}:{1}'.", _adsAddress, _adsPort).AppendLine()
                    .AppendFormat("The twin cat client returned the message: {0}", adsException.Message).AppendLine();
                throw new PlcCommunicationException(message.ToString(), _adsAddress, adsException);
            }
        }
    }
}
