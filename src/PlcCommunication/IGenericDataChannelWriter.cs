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
    /// <summary>
    /// The data channel manager is designed to safely write data or just send commands to PLC.
    /// It implements a handshake mechanism and the commands will be executed in order (queue), one after another.
    /// </summary>
    public interface IGenericDataChannelWriter
    {
        /// <summary>
        /// Occurs when [communication problem occured].
        /// Normally the event contains a PlcCommunicationException.
        /// </summary>
        /// <remarks>
        /// This event replaces exceptions: The AddAsyncWriteTask method should never throw an exception. All exceptions will be "converted" into this event.
        /// </remarks>
        event EventHandler<Exception> CommunicationProblemOccured;

        /// <summary>
        /// Gets or sets the polling rate.
        /// </summary>
        /// <value>
        /// The polling rate.
        /// </value>
        int PollingRate { get; set; }

        /// <summary>
        /// Gets or sets the timeout for every task.
        /// </summary>
        /// <value>
        /// The timeout for write operations. In case of timeout an event will be raised to notify the calling thread.
        /// </value>
        int Timeout { get; set; }

        /// <summary>
        /// Writes the specified tag value and waits for a handshake until timeout.
        /// If timeout occurs an event will be raised and the DataChannelManager will continue with the next task in the queue.
        /// </summary>
        /// <param name="value">The new value to write. Note: This UDT must follow the naming convention. The last element must be intDataState of type int.
        /// See T_Data_DtChn structure in PLN.</param>
        /// <remarks>
        /// The channel manager takes care about sending tags in sequence. This is also true for different tags (they will be queued).
        /// If it is needed to send tags in parallel, a second DateChannelManager instance could be used.
        /// </remarks>
        void AddAsyncWriteTask(object value);

        /// <summary>
        /// Waits until all write operations are complete.
        /// This call will block the current thread until all tasks are finished.
        /// </summary>
        void WaitWriteComplete();

        /// <summary>
        /// Initializes a channel to listening for. It will start listening as soon as the underlying TagListener is started.
        /// </summary>
        /// <remarks>
        /// Attention: This method could throw an exception as this is used for initialization. But afterward only CommunicationProblemOccured events will be raised.
        /// </remarks>
        /// <param name="fbName">Generic Plc to Mosaic Datachannel Function Object Block Name</param>
        /// <param name="scope">Scope</param>
        /// <param name="typeName">PLC Struct name to load</param>
        /// <exception cref="PlcCommunicationException">Data UDT must follow convention! It must contain a intDataState field and connection to PLC must be possible!</exception>
        void SetChannel(string fbName, string scope, string typeName);
    }
}
