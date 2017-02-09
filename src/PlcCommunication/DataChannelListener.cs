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
using System.Threading;

namespace VP.FF.PT.Common.PlcCommunication
{
    public class DataChannelListener<TDataType> : IDataChannelListener<TDataType>
    {
        private readonly ITagListener _tagListener;
        private readonly ITagController _tagController;
        private Tag _channelToLineTag;

        private const string UdtHandshakeFieldName = "intDataState";

        public DataChannelListener(ITagListener tagListener, ITagController tagController)
        {
            _tagListener = tagListener;
            _tagController = tagController;
        }

        public event EventHandler<Exception> CommunicationProblemOccured;

        public event EventHandler<TDataType> DataReceived;

        /// <summary>
        /// Sets the channel.
        /// </summary>
        /// <param name="channelToLineTag">The channel automatic line tag.</param>
        /// <exception cref="PlcCommunicationException">Data UDT must follow convention! It must contain a intDataState field and connection to PLC must be possible!</exception>
        public void SetChannel(Tag channelToLineTag)
        {
            // make sure that intDataState is set to 1
            var fields = typeof(TDataType).GetFields();

            if (fields.Length == 0 || fields[fields.Length - 1].Name != UdtHandshakeFieldName)
            {
                throw new PlcCommunicationException(
                        "Cannot set DataChannel " + channelToLineTag.Name + " because Data UDT does not follow convention! DataType Must contain a variable " +
                        UdtHandshakeFieldName + " as last field. Tag: " + channelToLineTag.Name, _tagListener.AddressAndPath, "");
            }

            var dataChannelStateTag = new Tag(channelToLineTag.Name + "." + UdtHandshakeFieldName, channelToLineTag.Scope, "INT", channelToLineTag.AdsPort);
            dataChannelStateTag.ValueChanged += DataChannelStateTagValueChanged;

            // initialize DataChannel on PLC side
            _tagController.WriteTag(dataChannelStateTag, (short)DataStateEnum.DataChannelFree);

            _tagListener.AddUdtHandler<TDataType>(channelToLineTag.DataType);
            _tagListener.AddTag(dataChannelStateTag);
            _channelToLineTag = channelToLineTag;
        }

        private void DataChannelStateTagValueChanged(Tag dataChannelStateTag, TagValueChangedEventArgs args)
        {
            if ((short) dataChannelStateTag.Value == (short) DataStateEnum.DataWritten)
            {
                try
                {
                    _tagListener.ReadTagSynchronously(_channelToLineTag);
                }
                catch (PlcCommunicationException e)
                {
                    RaiseCommunicationProblem(e);
                }

                if (_channelToLineTag.Value == null)
                {
                    Thread.Sleep(50);

                    try
                    {
                        // try to read UDT again
                        _tagListener.ReadTagSynchronously(_channelToLineTag);
                    }
                    catch (PlcCommunicationException e)
                    {
                        RaiseCommunicationProblem(e);
                    }

                    if (_channelToLineTag.Value == null)
                    {
                        RaiseCommunicationProblem(new PlcCommunicationException(
                                                      "Can't listen to DataChannel " + _channelToLineTag.Name +
                                                      " because value is null for DataType " +
                                                      _channelToLineTag.DataType + " (tried it two times)",
                                                      _tagListener.AddressAndPath, ""));
                    }
                }

                if (DataReceived != null && _channelToLineTag.Value != null)
                    DataReceived(this, (TDataType) _channelToLineTag.Value);

                try
                {
                    _tagController.WriteTag(dataChannelStateTag, (short) DataStateEnum.DataChannelFree);
                }
                catch (PlcCommunicationException e)
                {
                    RaiseCommunicationProblem(e);
                }
            }
        }

        private void RaiseCommunicationProblem(Exception e)
        {
            if (CommunicationProblemOccured != null)
                CommunicationProblemOccured(this, e);
        }
    }
}

