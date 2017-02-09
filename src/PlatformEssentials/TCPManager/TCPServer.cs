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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.TCPManager
{
    public class TcpServer<TData> : ITcpServer<TData>
    {
        private TcpListener _tcpListener;
        private CancellationTokenSource _cts;
        private readonly ILogger _logger;
        private readonly IDataConverter<TData> _converter;

        private Task _acceptTask;
        private readonly IList<TcpServerConnection<TData>> _serverData;

        public event EventHandler<TcpServerConnection<TData>> ConnectionStarted;

        /// <summary>
        /// Constructor
        /// </summary>
        public TcpServer(ILogger logger, IDataConverter<TData> converter)
        {
            _logger = logger;
            _converter = converter;
            _serverData = new List<TcpServerConnection<TData>>();
        }

        public void Start(string ip, int port)
        {
            Stop();

            _tcpListener = new TcpListener(IPAddress.Parse(ip), port);
            _tcpListener.Start();
            
            _logger.Info(String.Format("The server is running at port {0}...", port));
            _logger.Info("The local endpoint is: " + _tcpListener.LocalEndpoint);
            
            _cts = new CancellationTokenSource();

            _acceptTask = Task.Run(async () =>
            {
                Thread.CurrentThread.Name = "TCPServer";
                
                while (!_cts.IsCancellationRequested)
                {
                    _logger.Info("Waiting for connections.....");

                    var connection = new TcpServerConnection<TData>();
                    connection.Socket = await _tcpListener.AcceptSocketAsync();
                    connection.Sender = new Sender<TData>(_logger, _converter, connection.Socket, _cts.Token);
                    connection.Receiver = new Receiver<TData>(_logger, _converter, connection.Socket, _cts.Token);
                    
                    _logger.Info("Connection accepted from " + connection.Socket.RemoteEndPoint);

                    // create RECEIVE task                
                    connection.TaskReceive = new Task(connection.Receiver.Start, _cts.Token, TaskCreationOptions.LongRunning);
                    connection.TaskReceive.Start();

                    // create SEND task                
                    connection.TaskSend = new Task(connection.Sender.Start, _cts.Token, TaskCreationOptions.LongRunning);
                    connection.TaskSend.Start();

                    var handler = ConnectionStarted;
                    if (handler != null)
                    {
                        handler(this, connection);
                    }

                    _serverData.Add(connection);
                }
                _logger.Info("Stop waiting for incoming connections.");
            }, _cts.Token);
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                }

                foreach (var sd in _serverData)
                {
                    // Socket
                    if (sd.Socket != null)
                    {
                        sd.Socket.Close();
                        sd.Socket.Dispose();
                        sd.Socket = null;
                    }

                    // Receive
                    if (sd.TaskReceive != null)
                    {
                        try
                        {
                            sd.TaskReceive.Wait();
                        }
                        catch (AggregateException ae)
                        {
                            ae.Handle(ce => true);
                        }
                        sd.TaskReceive.Dispose();
                        sd.TaskReceive = null;
                    }

                    // Send
                    if (sd.TaskSend != null)
                    {
                        try
                        {
                            sd.TaskSend.Wait();
                        }
                        catch (AggregateException ae)
                        {
                            ae.Handle(ce => true);
                        }
                        sd.TaskSend.Dispose();
                        sd.TaskSend = null;
                    }
                }

                // Listener
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                    _tcpListener = null;
                }

                try
                {
                    _acceptTask.Wait();
                }
                catch (AggregateException ae)
                {
                    ae.Handle(ce => true);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Dispose failed", ex);
            }
        }
    }
}
