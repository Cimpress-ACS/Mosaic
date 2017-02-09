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
    public class DataChannelListenerTests
    {
        private IDataChannelListener<ValidDataUdtTest> _testee;
        private Mock<ITagListener> _tagListener;
        private Mock<ITagController> _tagController;
        private Tag _tag;

        [SetUp]
        public void Setup()
        {
            _tagListener = new Mock<ITagListener>();
            _tagController = new Mock<ITagController>();

            _testee = new DataChannelListener<ValidDataUdtTest>(_tagListener.Object, _tagController.Object);
            _tag = new Tag("NewDecision_DtChnToLine", "");
        }

        [Test]
        public void WhenSetInvalidDataType_MustThrowAnException()
        {
            var invalidTestee = new DataChannelListener<InvalidDataUdtTest>(_tagListener.Object, _tagController.Object);

            Action act = () => invalidTestee.SetChannel(new Tag());

            act.ShouldThrow<PlcCommunicationException>("DataType does not follow DataChannel convention, type must contain data status flag for handshake");
        }

        [Test]
        [Ignore("TODO")]
        public void WhenTagChanges_MustRaiseEvent()
        {
            var tag = new Tag("name", "scope", "T_ValidDataUDT");
            _testee.MonitorEvents();
            _testee.SetChannel(tag);

            // TODO: trigger TagListener tag changed with MOQ

            _testee.ShouldRaise("DataReceived");
        }

        /// <summary>
        /// Sample valid user defined type with DataState field at the end.
        /// </summary>
        internal struct ValidDataUdtTest
        {
            public int MyDataToSend;

            // needed by convention
            public short intDataState;
        }

        /// <summary>
        /// Sample invalid user defined type.
        /// </summary>
        internal struct InvalidDataUdtTest
        {
            // needed by convention (but at wrong place, must be the last field!)
            public byte intDataState;

            public double MyDataToSend;
        }
    }
}
