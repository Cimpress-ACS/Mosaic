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
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.TCPManager
{
    public class Receiver<TData> : IReceiver<TData>
    {
        private readonly ILogger _logger;
        private const int BufferSize = 100;
        private readonly Socket _socket;
        private readonly object _lockObjErrorEvt = new object();
        private readonly Parser<TData> _parser;
        private readonly CancellationToken _ct;

        public event EventHandler Error;
        public event EventHandler DataReceived;

        public Receiver(ILogger logger, IDataConverter<TData> converter, Socket socket, CancellationToken ct)
        {
            _logger = logger;
            _socket = socket;
            _parser = new Parser<TData>(logger, converter);
            _ct = ct;
        }

        /// <summary>
        /// Receive Thread
        /// </summary>
        public void Start()
        {
            try
            {
                while (!_ct.IsCancellationRequested)
                {
                    string debugData = string.Empty;
                    var buffer = new byte[BufferSize];

                    int countData = _socket.Receive(buffer);
                    if (countData == 0)
                    {
                        // getting 0 data means the client has disconnected
                        // see http://stackoverflow.com/questions/5868893/why-socket-reads-0-bytes-when-more-was-available
                        _logger.InfoFormat("Received 0 bytes. Connection looks aborted. Closing TCP session.");
                        return;
                    }

                    for (int i = 0; i < countData; i++)
                    {
                        _parser.AddData(buffer[i]);
                        debugData += buffer[i] + "/";
                    }
                    _parser.Parse();

                    var handler = DataReceived;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }

                _logger.Info("Receive aborted by task");
            }
            catch (Exception ex)
            {
                _logger.Error("Receiver failed", ex);

                if (!_ct.IsCancellationRequested)
                {
                    OnError();
                }
            }

            _logger.Info("receiving stopped!");
        }

        protected virtual void OnError()
        {
            var handler = Error;
            Task.Run(() =>
            {
                if (handler != null)
                    lock (_lockObjErrorEvt)
                    {
                        handler(this, EventArgs.Empty);
                    }
            });
        }

        public bool TryReceive(out TData item)
        {
            return _parser.Dequeue(out item);
        }

    }
}
