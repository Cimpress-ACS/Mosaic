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
    /// Occurs if communication to PLC fails. 
    /// </summary>
    public class PlcCommunicationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlcCommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="address">The address to PLC controller (ads address in case of Beckhoff).</param>
        /// <param name="innerException">The inner exception.</param>
        public PlcCommunicationException(string message, string address, Exception innerException)
            : base(message, innerException)
        {
            Address = address;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcCommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="address">The address to PLC controller (ads address in case of Beckhoff).</param>
        /// <param name="reason">The reason.</param>
        public PlcCommunicationException(string message, string address, string reason)
            : base(message)
        {
            Address = address;
            Reason = reason;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcCommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>        
        /// <param name="address">The address to PLC controller (ads address in case of Beckhoff).</param>
        /// <param name="path">The path.</param>
        /// <param name="reason">The reason.</param>
        public PlcCommunicationException(string message, string address, string path, string reason)
             :base(message)
        {
            Address = address;
            Path = path;
            Reason = reason;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlcCommunicationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="address">The address to PLC controller (ads address in case of Beckhoff).</param>
        /// <param name="path">The path.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="innerException">The inner exception.</param>
        public PlcCommunicationException(string message, string address, string path, string reason, Exception innerException)
            : base(message, innerException)
        {
            Address = address;
            Path = path;
            Reason = reason;
        }

        /// <summary>
        /// Address to PLC controller.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Slot number of I/O module in the chassis (in case of Rockwell).
        /// Port on PLC controller (in case of Beckhoff).
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Result code.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Tag which leaded to this problem.
        /// </summary>
        /// <value>
        /// Null, if Tag was not the route cause of communication problem.
        /// </value>
        public Tag Tag { get; set; }
    }
}
