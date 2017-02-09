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
using System.Threading.Tasks;
using Logix;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using Tag = VP.FF.PT.Common.PlcCommunication.Tag;

namespace VP.FF.PT.Common.PlcCommunicationLogix.IngearImplementation
{
    [Export(typeof(ITagController))]
    public class IngearTagController : ITagController
    {
        private readonly Controller _controlLogix = new Controller();
        private readonly IPlatformScheduler _writeTaskScheduler = new SequentialScheduler();

        private readonly Dictionary<string, ConvertToLogixTagValueFunction> _tagValueConverters = new Dictionary<string, ConvertToLogixTagValueFunction>();

        public IngearTagController(string ipAddress, string path = "0")
        {
            _controlLogix.Timeout = 3000;

            _controlLogix.IPAddress = ipAddress;
            _controlLogix.Path = path;
        }

        public void AddUdtHandler(string dataType, ConvertToLogixTagValueFunction convertToLogixTagValue)
        {
            _tagValueConverters.Add(dataType, convertToLogixTagValue);
        }

        public void StartConnection()
        {
            var result = _controlLogix.Connect();
            if (result != ResultCode.E_SUCCESS)
            {
                throw new PlcCommunicationException("Can't connect to ControlLogix.", 
                    _controlLogix.IPAddress, _controlLogix.Path, result.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void StartConnection(string address, int port)
        {
            _controlLogix.IPAddress = address;
            _controlLogix.Path = port.ToString(CultureInfo.InvariantCulture);
            StartConnection();
        }

        public bool IsConnected
        {
            get { return _controlLogix.IsConnected; }
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
                            "IngearTagController can't write tag because there is no connection established!",
                            _controlLogix.IPAddress, string.Empty);
                    }

                    var logixTag = new Logix.Tag(_controlLogix)
                    {
                        Name = tag.FullName(),
                        DataType = IngearHelper.ParseNetLogixDataType(tag.DataType)
                    };

                    // for some reason we must read the tag at least once
                    if (logixTag.ErrorCode != 0)
                    {
                        _controlLogix.ReadTag(logixTag);
                    }

                    // handle special DataTypes like UDTs
                    ConvertToLogixTagValueFunction convertFunction;
                    if (_tagValueConverters != null &&
                        _tagValueConverters.TryGetValue(tag.DataType, out convertFunction))
                    {
                        logixTag.Value = convertFunction(value);
                    }
                    else
                    {
                        if (tag.DataType.Equals(IEC61131_3_DataTypes.Boolean))
                        {
                            if ((bool) value)
                                logixTag.Value = "True";
                            else
                                logixTag.Value = "False";
                        }
                        else
                        {
                            logixTag.Value = value;
                        }
                    }

                    var result = _controlLogix.WriteTag(logixTag);

                    if (result != ResultCode.E_SUCCESS)
                    {
                        throw new PlcCommunicationException("Can't write value to ControlLogix.",
                            _controlLogix.IPAddress,
                            _controlLogix.Path, logixTag.ErrorString);
                    }
                }
                finally
                {
                    tag.ReleaseValue();
                }
            });

            return task;
        }

        public Task WriteTag(Tag tag, object[] values)
        {
            if (tag.Value == null && values == null)
                return Task.FromResult(true);

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

        public void Dispose()
        {
            var result = _controlLogix.Disconnect();
            if (result != ResultCode.E_SUCCESS)
            {
                throw new PlcCommunicationException("Can't disconnect from ControlLogix.", _controlLogix.IPAddress, _controlLogix.Path, _controlLogix.ErrorString);
            }
        }
    }
}
