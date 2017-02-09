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
    public interface IDataChannelListener<T>
    {
        /// <summary>
        /// Occurs when new data from PLC over channeld received. All handshakes are already done at this point.
        /// </summary>
        event EventHandler<T> DataReceived;

        /// <summary>
        /// Occurs when [communication problem occured].
        /// Normally the event contains a PlcCommunicationException.
        /// </summary>
        /// <remarks>
        /// This event replaces exceptions: The AddAsyncWriteTask method should never throw an exception. All exceptions will be "converted" into this event.
        /// </remarks>
        event EventHandler<Exception> CommunicationProblemOccured;

        /// <summary>
        /// Initializes a channel to listening for. It will start listening as soon as the underlying TagListener is started.
        /// </summary>
        /// <remarks>
        /// Attention: This method could throw an exception as this is used for initialization. But afterward only CommunicationProblemOccured events will be raised.
        /// </remarks>
        /// <param name="channelToLineTag">The PLC tag.</param>
        /// <exception cref="PlcCommunicationException">Data UDT must follow convention! It must contain a intDataState field and connection to PLC must be possible!</exception>
        void SetChannel(Tag channelToLineTag);
    }
}
