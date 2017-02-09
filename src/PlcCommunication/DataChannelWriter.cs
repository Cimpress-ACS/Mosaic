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
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunication
{
    public class DataChannelWriter : IDataChannelWriter
    {
        private readonly object _sync = new object();

        private readonly ITagListener _tagListener;
        private readonly ITagController _tagController;

        private Task _latestTask;

        private int _errorCount;
        private int _timeoutCount;

        private const int DefaultTimeout = 10000;
        private const int DefaultPollingRate = 20;
        private const int MaxRepeatSend = 5;

        private const string UdtHandshakeFieldName = "intDataState";

        public DataChannelWriter(ITagListener tagListener, ITagController tagController)
        {
            _tagListener = tagListener;
            _tagController = tagController;

            PollingRate = DefaultPollingRate;
            Timeout = DefaultTimeout;
        }

        public event EventHandler<Exception> CommunicationProblemOccured;

        public int PollingRate { get; set; }
        public int Timeout { get; set; }

        public void AddAsyncWriteTask(Tag tag, object value)
        {
            EnqueueAction(() =>
            {
                _timeoutCount = 0;
                _errorCount = 0;

                var dataStateTag = new Tag(tag.Name + "." + UdtHandshakeFieldName, tag.Scope, IEC61131_3_DataTypes.Int);

                if (SendData(tag, value) < 0)
                    return;

                // wait for PLC handshake
                while (true)
                {
                    // PLC does not send handshake, timeout
                    Task.Delay(PollingRate).Wait();
                    
                    if (_timeoutCount++ * PollingRate > Timeout)
                    {
                        RaiseCommunicationProblem(
                            new PlcCommunicationException("Timeout. Can't write data to DataChannel " + tag.Name,
                                                          _tagListener.AddressAndPath, ""));
                        break;  // continue with next task
                    }

                    try
                    {
                        _tagListener.ReadTagSynchronously(dataStateTag);
                    }
                    catch (Exception e)
                    {
                        RaiseCommunicationProblem(
                            new PlcCommunicationException("Can't read DataChannels state tag, will continue with next Job", 
                                                          _tagListener.AddressAndPath, e));
                        break;  // continue with next task
                    }

                    // PLC send handshake successfull, end task
                    if ((short)dataStateTag.Value == (short)DataStateEnum.DataChannelFree)
                        break;  // continue with next task

                    // PLC received invalid data, try send again
                    if ((short)dataStateTag.Value == (short)DataStateEnum.InvalidDataReceived)
                    {
                        if (_errorCount++ > MaxRepeatSend)
                        {
                            RaiseCommunicationProblem(
                                new PlcCommunicationException(
                                    "Negative response from PLC: Invalid data (tried it 5 times)",
                                    _tagListener.AddressAndPath, DataStateEnum.InvalidDataReceived.ToString()));
                            break;  // continue with next task
                        }

                        Console.WriteLine("DataChannel invalid data, try sending again...");
                        if (SendData(tag, value) < 0)
                            break;  // continue with next task
                    }

                    // PLC can't process data, error state
                    if ((short)dataStateTag.Value == (short)DataStateEnum.InvalidDataReceivedError)
                    {
                        RaiseCommunicationProblem(new PlcCommunicationException("Negative response from PLC: Error",
                                                                                _tagListener.AddressAndPath,
                                                                                DataStateEnum.InvalidDataReceivedError
                                                                                             .ToString()));
                        break;  // continue with next task
                    }
                }
            });
        }

        public void WaitWriteComplete()
        {
            if (_latestTask != null)
                Task.WaitAll(_latestTask);
        }

        private void EnqueueAction(Action action)
        {
            lock (_sync)
            {
                if (_latestTask == null || _latestTask.Status == TaskStatus.RanToCompletion)
                    _latestTask = Task.Factory.StartNew(action);
                else
                    _latestTask = _latestTask.ContinueWith(tsk => action());
            }
        }

        private int SendData(Tag tag, object value)
        {
            // make sure that intDataState is set to 1
            var fields = value.GetType().GetFields();

            if (fields.Length == 0 || fields[fields.Length - 1].Name != UdtHandshakeFieldName)
            {
                RaiseCommunicationProblem(
                    new PlcCommunicationException(
                        "Cannot send data over DataChannel because Data UDT does not follow convention! Must have variable " +
                        UdtHandshakeFieldName + " as last field. Tag: " + tag.Name, _tagListener.AddressAndPath, ""));

                return -1;
            }

            fields[fields.Length - 1].SetValue(value, (short)DataStateEnum.DataWritten);

            try
            {
                _tagController.WriteTag(tag, value);
            }
            catch (Exception e)
            {
                RaiseCommunicationProblem(e);
                return -1;
            }

            return 0;
        }

        private void RaiseCommunicationProblem(Exception e)
        {
            if (CommunicationProblemOccured != null)
                CommunicationProblemOccured(this, e);
        }
    }
}
