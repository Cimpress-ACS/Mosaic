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
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class DataChannelWriterTests
    {
        private IDataChannelWriter _testee;
        private Mock<ITagListener> _tagListener;
        private Mock<ITagController> _tagController;
        private Tag _tag;

        [SetUp]
        public void Setup()
        {
            _tagListener = new Mock<ITagListener>();
            _tagController = new Mock<ITagController>();

            _testee = new DataChannelWriter(_tagListener.Object, _tagController.Object);
            _testee.PollingRate = 50;
            _tag = new Tag("NewDecision_DtChnToPlc", "");
        }

        [Test]
        public void WhenWriteWrongDataToChannel_MustRaiseErrorEvent()
        {
            _testee.MonitorEvents();

            Action act = () =>
            {
                _testee.AddAsyncWriteTask(new Tag(), 5);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
            _testee.ShouldRaise("CommunicationProblemOccured");
        }

        [Test]
        public void WhenWriteWrongDataUdtToChannel_MustThrowException()
        {
            _testee.MonitorEvents();

            Action act = () =>
            {
                var udt = new InvalidDataUdtTest();

                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
            _testee.ShouldRaise("CommunicationProblemOccured");
        }

        [Test]
        public void WhenValidDataUdtToChannel_MustNotThrowException()
        {
            // PLC sends valid handshake
            _tagListener
                .Setup(t => t.ReadTagSynchronously(It.IsAny<Tag>()))
                .Callback<Tag>(tag => { tag.Value = DataStateEnum.DataChannelFree; });

            Action act = () =>
            {
                var udt = new ValidDataUdtTest();

                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
        }

        [Test]
        public void GivenPlcSendsInvalidData_WhenWriteValidData_TryItSeveralTimes()
        {
            // PLC sends invalid data
            _tagListener
                .Setup(t => t.ReadTagSynchronously(It.IsAny<Tag>()))
                .Callback<Tag>(tag => { tag.Value = DataStateEnum.InvalidDataReceived; });
            int writeCalls = 0;
            _tagController
                .Setup(t => t.WriteTag(It.IsAny<Tag>(), It.IsAny<object>()))
                .Callback(() => writeCalls++);
            
            Action act = () =>
            {
                var udt = new ValidDataUdtTest();

                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
            writeCalls.Should().BeGreaterThan(1, "LineControl should try to repeat send data over DataChannel in case of negative feedback from PLC");
        }

        [Test]
        public void GivenPlcSendsFatalResponse_WhenWriteValidData_MustThrowException()
        {
            _testee.MonitorEvents();
            // PLC sends fatal error response
            _tagListener
                .Setup(t => t.ReadTagSynchronously(It.IsAny<Tag>()))
                .Callback<Tag>(tag => { tag.Value = DataStateEnum.InvalidDataReceivedError; });

            Action act = () =>
            {
                var udt = new ValidDataUdtTest();

                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
            _testee.ShouldRaise("CommunicationProblemOccured");
        }

        [Test]
        public void WhenSendMultipleMessages_MustQueueItAndSendOneAfterAnother()
        {
            // PLC sends valid handshake
            _tagListener
                .Setup(t => t.ReadTagSynchronously(It.IsAny<Tag>()))
                .Callback<Tag>(tag => { tag.Value = DataStateEnum.DataChannelFree; });
            int writeCalls = 0;
            _tagController
                .Setup(t => t.WriteTag(It.IsAny<Tag>(), It.IsAny<object>()))
                .Callback(() => writeCalls++);
            
            Action act = () =>
            {
                var udt = new ValidDataUdtTest();

                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
            writeCalls.Should().Be(5, "LineControl sent 5 valid messages");
        }

        [Test]
        public void GivenOneMessageIsInvalid_WhenSendMultipleMessages_MustSendAtLeastTheValidMessages()
        {
            _testee.MonitorEvents();

            // PLC sends valid handshake
            _tagListener
                .Setup(t => t.ReadTagSynchronously(It.IsAny<Tag>()))
                .Callback<Tag>(tag => { tag.Value = DataStateEnum.DataChannelFree; });
            int writeCalls = 0;
            _tagController
                .Setup(t => t.WriteTag(It.IsAny<Tag>(), It.IsAny<object>()))
                .Callback(() => writeCalls++);

            Action act = () =>
            {
                var udt = new ValidDataUdtTest();
                var invalidUdt = new InvalidDataUdtTest();

                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.AddAsyncWriteTask(_tag, invalidUdt);
                _testee.AddAsyncWriteTask(_tag, udt);
                _testee.WaitWriteComplete();
            };

            act.ShouldNotThrow("DataChannelManager must never throw an exception, but only error events");
            _testee.ShouldRaise("CommunicationProblemOccured");
            writeCalls.Should().Be(2, "2 messages are valid and 1 is invalid (should be skipped)");
        }

        /// <summary>
        /// Sample valid user defined type with DataState field at the end.
        /// </summary>
        internal struct ValidDataUdtTest
        {
            public double MyDataToSend;

            // needed by convention
            public short intDataState;
        }

        /// <summary>
        /// Sample invalid user defined type.
        /// </summary>
        internal struct InvalidDataUdtTest
        {
            // needed by convention (but at wrong place, must be the last field!)
            public short intDataState;

            public double MyDataToSend;
        }
    }
}
