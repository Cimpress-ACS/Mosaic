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
using System.Threading.Tasks;
using AutomatedSolutions.Win.Comm;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.PlcCommunication;
using Channel = AutomatedSolutions.Win.Comm.AB.Logix.Net.Channel;
using Device = AutomatedSolutions.Win.Comm.AB.Logix.Device;
using Item = AutomatedSolutions.Win.Comm.AB.Logix.Item;

namespace VP.FF.PT.Common.PlcCommunicationLogix.ASCommImplementation
{
    public class ASCommTagController : ITagController
    {
        private readonly Channel _channel;
        private readonly Device _device;

        private readonly IPlatformScheduler _writeTaskScheduler = new SequentialScheduler();

        public ASCommTagController(string ipAddress)
        {
            _channel = new Channel();
            _channel.Error += OnChannelError;
            _device = new Device(ipAddress);
            _device.Error += OnDeviceError;

            _channel.Devices.Add(_device);
        }

        public void AddUdtHandler(string dataType, ConvertToLogixTagValueFunction convertToLogixTagValue)
        {
            throw new NotImplementedException();
        }

        public void StartConnection()
        {
        }

        public void StartConnection(string address, int port)
        {
        }

        public bool IsConnected
        {
            get { throw new NotImplementedException(); }
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

                    var item = new Item(tag.FullName(), 1);

                    Result result;
                    _device.Write(item, value, out result);

                    if (!result.IsOK)
                    {
                        throw new PlcCommunicationException("Can't write value to ControlLogix.",
                            _device.RoutePath, result.EvArgs.Message);
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

            var task = _writeTaskScheduler.Schedule(() =>
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
            });

            return task;
        }

        private void OnDeviceError(object sender, DeviceEventArgs eventArgs)
        {
            throw new PlcCommunicationException("ASComm Device Error in TagController", _device.RoutePath, eventArgs.Message);
        }

        private void OnChannelError(object sender, ChannelEventArgs eventArgs)
        {
            throw new PlcCommunicationException("ASComm Channel Error in TagController", _device.RoutePath, eventArgs.Message);
        }

        public void Dispose()
        {
            _device.Dispose();
        }
    }
}
