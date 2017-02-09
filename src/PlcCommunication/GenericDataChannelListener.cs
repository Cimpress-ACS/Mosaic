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

namespace VP.FF.PT.Common.PlcCommunication
{
    public class GenericDataChannelListener<TDataType> : IGenericDataChannelListener<TDataType>
    {
        private readonly ITagListener _tagListener;
        private readonly ITagController _tagController;
        private Tag _channelToLineTag;
        private Tag _dataChannelStateTag;

        private const string UdtDataFieldName = "m_pOutputData^";
        private const string UdtHandshakeFieldName = "m_iDchConfirm";

        public GenericDataChannelListener(ITagListener tagListener, ITagController tagController)
        {
            _tagListener = tagListener;
            _tagController = tagController;
        }

        public event EventHandler<Exception> CommunicationProblemOccured;

        public event EventHandler<TDataType> DataReceived;

        /// <summary>
        /// Creates and sets the channel
        /// </summary>
        /// <param name="fbName">Generic Plc to Mosaic Datachannel Function Object Block Name</param>
        /// <param name="scope">Scope</param>
        /// <param name="typeName">PLC Struct name to load</param>
        /// <exception><cref>VP.FF.PT.Common.PlcCommunication.PlcCommunicationException</cref></exception>
        public void SetChannel(string fbName, string scope, string typeName)
        {
            _dataChannelStateTag = new Tag(fbName + "." + UdtHandshakeFieldName, scope, "INT", _tagListener.Port);
            _dataChannelStateTag.ValueChanged += DataChannelStateTagValueChanged;

            // initialize DataChannel on PLC side
            _tagController.WriteTag(_dataChannelStateTag, (short)DataStateEnum.DataChannelFree);

            _channelToLineTag = new Tag(fbName + "." + UdtDataFieldName, scope, typeName, _tagListener.Port);
            _tagListener.AddUdtHandler<TDataType>(_channelToLineTag.DataType);

            // try to read value for the first time (might throw an exception if symbol does not exist)
            _tagListener.ReadTagSynchronously(_dataChannelStateTag);
            _tagListener.AddTag(_dataChannelStateTag);
        }

        /// <summary>
        /// It is possible to clear the datachannel buffer on the PLC
        /// </summary>
        public void ClearBufferOnPlc()
        {
            try
            {
                if (_dataChannelStateTag != null)
                {
                    _tagController.WriteTag(_dataChannelStateTag, (short)DataStateEnum.ClearFifoBuffer);
                }
            }
            catch (PlcCommunicationException e)
            {
                RaiseCommunicationProblem(e);
            }
        }

        /// <summary>
        /// Callback from the Beckhoff Listener that the Datachannel changed
        /// </summary>
        /// <param name="dataChannelStateTag"></param>
        /// <param name="args"></param>
        private void DataChannelStateTagValueChanged(Tag dataChannelStateTag, TagValueChangedEventArgs args)
        {
            if ((short)dataChannelStateTag.Value == (short)DataStateEnum.DataWritten)
            {
                if (DataReceived != null)
                {
                    int iIdx = 0;
                        try
                        {
                            _channelToLineTag.ClearValue();
                            _tagListener.ReadTagSynchronously(_channelToLineTag);
                            iIdx++;
                        }
                        catch (PlcCommunicationException e)
                        {
                            RaiseCommunicationProblem(e);
                        }

                    if (DataReceived != null)
                    {
                        if (_channelToLineTag.Value == null)
                            RaiseCommunicationProblem(new PlcCommunicationException("Can't listen to DataChannel " + _channelToLineTag.Name + " because value is null for DataType " + _channelToLineTag.DataType, _tagListener.AddressAndPath, ""));

                        DataReceived(this, (TDataType) _channelToLineTag.Value);
                    }
                }

                try
                {
                    _tagController.WriteTag(dataChannelStateTag, (short)DataStateEnum.DataChannelFree);
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

