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

namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// The tag controller writes new tag values to the PLC.
    /// It does not wait for a handshake.
    /// </summary>
    public interface ITagController : IDisposable
    {
        /// <summary>
        /// Starts the connection.
        /// The connection closes automatically when disposing an instance of this class.
        /// </summary>
        void StartConnection();

        /// <summary>
        /// Starts the connection.
        /// </summary>
        /// <param name="address">The address to PLC.</param>
        /// <param name="path">The path (in case of Beckhoff ads port).</param>
        void StartConnection(string address, int path);

        /// <summary>
        /// Determines whether this instance is connected and running.
        /// </summary>
        /// <remarks>
        /// Note for Beckhoff implementation: If PLC is in CONFIG mode it will return false. If PLC is in RUN mode but STOPPED it will return false as well.
        /// It will return true if PLC is in RUN mode and STARTED.
        /// </remarks>
        /// <returns>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </returns>
        bool IsConnected { get; }

        /// <summary>
        /// Writes a tag value.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The value to write.</param>
        Task WriteTag(Tag tag, object value);

        /// <summary>
        /// Writes a tag array.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="values">The values.</param>
        Task WriteTag(Tag tag, object[] values);

        /// <summary>
        /// Adds a UDT (user defined type) handler.
        /// This extends the standard type convertion for BOOL, INT, etc. with custom types.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="convertToLogixTagValue">
        /// Function delegate for converting a custom object into a well defined bool array (CIP = Common Industrial Protocol) for the PLC.
        /// </param>
        void AddUdtHandler(string dataType, ConvertToLogixTagValueFunction convertToLogixTagValue);
    }

    /// <summary>
    /// Converts a custom value object to a byte array.
    /// </summary>
    /// <param name="tagValueObject">Input tag value to convert.</param>
    /// <returns>Byte array is needed by Ingear for example.</returns>
    public delegate byte[] ConvertToLogixTagValueFunction(object tagValueObject);
}
