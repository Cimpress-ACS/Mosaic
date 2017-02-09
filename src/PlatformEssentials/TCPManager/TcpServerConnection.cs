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


using System.Net.Sockets;
using System.Threading.Tasks;

namespace VP.FF.PT.Common.PlatformEssentials.TCPManager
{
    /// <summary>
    /// Holds details about a TCP connection that was initiated by the TCP server.
    /// </summary>
    /// <typeparam name="TData">The type of data that is being sent and received.</typeparam>
    public class TcpServerConnection<TData>
    {
        /// <summary>
        /// The sender which is sending the data to the remote endpoint.
        /// </summary>
        public ISender<TData> Sender { get; set; }

        /// <summary>
        /// The receiver which is receiving data from the remote endpoing, and eventually delegating the handling of the data to an IParser.
        /// </summary>
        public IReceiver<TData> Receiver { get; set; }

        /// <summary>
        /// The underlying socket.
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// The task that is used for the receiver to run in it's separate task/thread.
        /// </summary>
        public Task TaskReceive { get; set; }

        /// <summary>
        /// The task that is used for the sender to run in it's separate task/thread.
        /// </summary>
        public Task TaskSend { get; set; }

        /// <summary>
        /// Whether the underlying socket is connected.
        /// </summary>
        public bool Connected
        {
            get { return Socket != null && Socket.Connected; }
        }

        /// <summary>
        /// Try to receive data.
        /// </summary>
        /// <param name="item">The data to send.</param>
        /// <returns>True if the data could be sent.</returns>
        public bool TryReceive(out TData item)
        {
            return Receiver.TryReceive(out item);
        }

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="item">The data to send.</param>
        public void Send(TData item)
        {
            Sender.Send(item);
        }
    }
}
