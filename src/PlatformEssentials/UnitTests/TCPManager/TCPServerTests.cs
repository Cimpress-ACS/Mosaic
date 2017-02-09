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
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.TCPManager;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.TCPManager
{
    [Ignore("The tests are not reliable on the build server. They can be triggered manually.")]
    [TestFixture]
    [NCrunch.Framework.ExclusivelyUses("IpPort_13580")]
    public class TCPServerTests
    {
        private readonly Mock<ILogger> _logger = new Mock<ILogger>(MockBehavior.Loose);
        private IPAddress ip;
        private int port;
        private Random _random;

        private readonly object _lock = new object(); 

        [SetUp]
        public void SetUp()
        {
            ip = IPAddress.Loopback;
            port = 13580;
            _random = new Random();

            Monitor.Enter(_lock);
        }

        [TearDown]
        public void TearDown()
        {
            Monitor.Exit(_lock);
        }

        [Test]
        public void MultipleConnectionsPossible()
        {
            using (var server = CreateServer())
            {
                var connectionEvent = new ManualResetEvent(false);
                var receiveEvent = new ManualResetEvent(false);
                TcpServerConnection<int> data = null;
                server.ConnectionStarted += (sender, serverData) =>
                {
                    data = serverData;
                    data.Receiver.DataReceived += (o, args) => receiveEvent.Set();
                    connectionEvent.Set();
                };

                var tcpClient1 = Connect(connectionEvent);
                var data1 = data;
                connectionEvent.Reset();
                var tcpClient2 = Connect(connectionEvent);
                var data2 = data;

                SendRandomData(data1);
                ReceiveToTcpClient(tcpClient1);

                SendRandomData(data2);
                ReceiveToTcpClient(tcpClient2);

                var sentData1 = SendFromTcpClient(tcpClient1);
                WaitOne(receiveEvent).Should().BeTrue();
                ValidateReceive(data1, sentData1);

                receiveEvent.Reset();

                var sentData2 = SendFromTcpClient(tcpClient2);
                WaitOne(receiveEvent).Should().BeTrue();
                ValidateReceive(data2, sentData2);
            }
        }

        [Test]
        public void SendingDatawithTCPServerIsReceivedCorrectly()
        {
            using (var server = CreateServer())
            {
                var resetEvent = new ManualResetEvent(false);
                TcpServerConnection<int> data = null;
                server.ConnectionStarted += (sender, serverData) =>
                {
                    data = serverData;
                    resetEvent.Set();
                };

                var tcpClient = Connect(resetEvent);

                SendRandomData(data);
                ReceiveToTcpClient(tcpClient);
            }
        }

        [Test]
        public void DataSentIsReceivedCorrectlyByTCPServer()
        {
            using (var server = CreateServer())
            {
                var resetEvent = new ManualResetEvent(false);
                var receiveEvent = new ManualResetEvent(false);
                TcpServerConnection<int> data = null;
                server.ConnectionStarted += (sender, serverData) =>
                {
                    data = serverData;
                    data.Receiver.DataReceived += (o, args) => receiveEvent.Set();
                    resetEvent.Set();
                };

                var tcpClient = new TcpClient(ip.ToString(), port);
                var sentData = SendFromTcpClient(tcpClient);
                WaitOne(receiveEvent).Should().BeTrue();
                ValidateReceive(data, sentData);
            }
        }

        [Test]
        public void MultipleDataSentIsReceivedCorrectlyByTCPServer()
        {
            using (var server = CreateServer())
            {
                var tcpClient = new TcpClient(ip.ToString(), port);

                var resetEvent = new ManualResetEvent(false);
                var receiveEvent = new ManualResetEvent(false);
                TcpServerConnection<int> data = null;
                server.ConnectionStarted += (sender, serverData) =>
                {
                    data = serverData;
                    data.Receiver.DataReceived += (o, args) => receiveEvent.Set();
                    resetEvent.Set();
                };

                WaitOne(resetEvent).Should().BeTrue();

                var dataToBeSent = new List<int>();
                for (int i = 0; i < 50; i++)
                {
                    dataToBeSent.Add(SendFromTcpClient(tcpClient));
                }

                WaitOne(receiveEvent).Should().BeTrue();

                for (int i = 0; i < 50; i++)
                {
                    int val;
                    if (!data.TryReceive(out val))
                    {
                        WaitOne(receiveEvent).Should().BeTrue();
                        receiveEvent.Reset();
                        i--;
                        continue;
                    }
                    val.Should().Be(dataToBeSent[i]);
                }
            }
        }

        private TcpClient Connect(ManualResetEvent resetEvent)
        {
            var tcpClient = new TcpClient(ip.ToString(), port);

            var signalReceived = WaitOne(resetEvent);
            signalReceived.Should().BeTrue();
            return tcpClient;
        }

        private int SendRandomData(TcpServerConnection<int> data)
        {
            var itemToBeSent = _random.Next(0, 1000);
            data.Send(itemToBeSent);
            return itemToBeSent;
        }

        private static bool WaitOne(ManualResetEvent receiveEvent)
        {
            return receiveEvent.WaitOne(TimeSpan.FromSeconds(5));
        }

        private int ReceiveToTcpClient(TcpClient listener)
        {
            byte[] array = new byte[sizeof (int)];
            listener.Client.Receive(array, sizeof (int), SocketFlags.None);

            var parser = new Parser<int>(_logger.Object, new DataConverter());

            foreach (byte b in array)
            {
                parser.AddData(b);
            }

            parser.Parse();

            int itemReceived;
            Assert.That(parser.Dequeue(out itemReceived));
            return itemReceived;
        }

        private static void ValidateReceive(TcpServerConnection<int> serverData, int expectedValue)
        {
            int val;
            serverData.TryReceive(out val).Should().BeTrue();
            expectedValue.Should().Be(val);
        }

        private TcpServer<int> CreateServer()
        {
            var server = new TcpServer<int>(_logger.Object, new DataConverter());
            server.Start(ip.ToString(), port);
            return server;
        }

        private int SendFromTcpClient(TcpClient tcpClient)
        {
            var itemToBeSent = _random.Next(0, 1000);
            tcpClient.Client.Send(BitConverter.GetBytes(itemToBeSent));
            return itemToBeSent;
        }
    }
}
