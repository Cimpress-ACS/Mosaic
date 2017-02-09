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
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.TCPManager
{
    public class Sender<TData> : ISender<TData>
    {

        private readonly ConcurrentQueue<TData> _queueSend;
        private readonly ILogger _logger;
        private readonly Socket _socket;
        private readonly IDataConverter<TData> _converter;
        private readonly AutoResetEvent _areSend = new AutoResetEvent(false);
        private readonly TimeSpan _waitTimeout = TimeSpan.FromMilliseconds(500); // wait timeout should be smaller than the dummy send cyle time
        public event EventHandler Error;
        private readonly object _lockObjErrorEvt = new object();
        private readonly CancellationToken _ct;

        public Sender(ILogger logger, IDataConverter<TData> converter, Socket socket, CancellationToken ct)
        {
            _logger = logger;
            _queueSend = new ConcurrentQueue<TData>();
            _converter = converter;
            _socket = socket;
            _converter.Init();
            _ct = ct;
        }

        public void Send(TData item)
        {
            if (_queueSend != null)
            {
                _queueSend.Enqueue(item);

                // Signal that a value has been written.
                _areSend.Set();
            }
        }

        public void Start()
        {
            try
            {
                while (!_ct.IsCancellationRequested)
                {
                    // timeout prevents deadlock if cancellation request is TRUE
                    _areSend.WaitOne(_waitTimeout);

                    TData item;
                    while (_queueSend.TryDequeue(out item))
                    {
                        //_logger.Info("SERVER Transmitting {0}", _queueSend.Peek());
                        _socket.Send(_converter.GetBytes(item));
                    }
                }

                _logger.Info("Send aborted by task");
            }
            catch (Exception ex)
            {
                _logger.Error("-------------------------------------------------");
                _logger.Error("Send Error");
                _logger.Error("-------------------------------------------------");
                _logger.Error(ex.Message + "\n" + ex.StackTrace);

                if (!_ct.IsCancellationRequested)
                {
                    OnError();
                }
            }
            _logger.Info("sending stopped!");
        }

        protected virtual void OnError()
        {
            if (Error != null)
            {
                Task.Run(() =>
                {
                    lock (_lockObjErrorEvt)
                    {
                        Error(this, EventArgs.Empty);
                    }
                });
            }
        }


    }
}
