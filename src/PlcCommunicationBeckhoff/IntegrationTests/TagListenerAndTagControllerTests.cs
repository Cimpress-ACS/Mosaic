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
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class TagListenerAndTagControllerTests
    {
        private ITagListener _tagListener;
        private ITagController _tagController;
        private Tag _tag;

        [SetUp]
        public void SetUp()
        {
            _tag = new Tag("fbAGS_1.udiAlarmId", "MiddlePRG_1", "UDINT", Global.AdsPort);
            _tagController = new BeckhoffTagController(Global.AdsAddress, Global.AdsPort);
            _tagController.StartConnection();
            _tagListener = new BeckhoffPollingTagListener(Global.AdsAddress, Global.AdsPort, new GlobalLock());
            _tagListener.StartListening();

            try
            {
                _tagController.WriteTag(_tag, 0).Wait(1000);
            }
            catch (Exception e)
            {
                Assert.Fail("Cannot setup test because writing initial values to PLC failed. " + e.Message);
            }
        }

        [TearDown]
        public void TearDown()
        {
            _tagListener.Dispose();
            _tagController.Dispose();
        }

        [Test]
        public void WhenWriteAndReadValue_TagShouldRaiseEvent()
        {
            _tag.MonitorEvents();

            var writeAction = new Action(() => _tagController.WriteTag(_tag, 5).Wait());
            var readAction = new Action(() => _tagListener.ReadTagSynchronously(_tag));

            writeAction.ShouldNotThrow();
            readAction.ShouldNotThrow();
            _tag.ShouldRaise("ValueChanged");
            _tag.Value.Should().Be((uint)5);
        }

        [Test]
        public void GivenTagInTagListener_WhenWriteAndReadValue_TagListenerShouldRaiseEvent()
        {
            _tagListener.AddTag(_tag);
            _tagListener.MonitorEvents();

            var writeAction = new Action(() => _tagController.WriteTag(_tag, 10).Wait());
            var readAction = new Action(() => _tagListener.ReadTagSynchronously(_tag));

            writeAction.ShouldNotThrow();
            readAction.ShouldNotThrow();
            _tagListener.ShouldNotRaise("CommunicationProblemOccured");
            _tagListener.ShouldRaise("TagChanged");
            _tag.Value.Should().Be((uint)10);
        }

        [Test]
        public void WhenAddTypeContainingMultipleTags_ShouldAddAllTagsToTagListener()
        {
            _tagListener.AddTags(typeof(TagsContainerType));

            _tagListener.GetTags().Should().HaveCount(3);
            _tagListener.GetTags().Should().Contain(TagsContainerType.AgsAlarmIdTag);
        }

        [Test]
        public void WhenReadSynchronously_ShouldReadValue()
        {
            var tag = new Tag("fbAGS_1." + NamingConventions.CommonInterfaceAutoCmdChannel, "MiddlePRG_1", "INT");

            _tagListener.ReadTagSynchronously(tag);

            tag.Value.Should().NotBeNull();
            tag.Value.Should().Be((short) -1);
        }

        [Test]
        public void GivenTagControllerCreatedOnDifferentThread_WhenDispose_ShouldNotThrow()
        {
            ITagController tagController = null;

            var thread = new Thread(() =>
            {
                tagController = new BeckhoffTagController(Global.AdsAddress, Global.AdsPort);
                tagController.StartConnection();
            });

            thread.Start();
            thread.Join();

            var action = new Action(() => tagController.Dispose());
            
            action.ShouldNotThrow();

            thread.Abort();
        }

        // TagController and TagListener consistency check
        [Test]
        public void GivenTagControllerWroteValue_WhenTagListenerReadsIt_TagValueShouldBeSame()
        {
            _tagController.WriteTag(_tag, 5).Wait(500);
            Type typeAfterTagController = _tag.Value.GetType();
            object valueAfterTagController = _tag.Value.Clone();
            
            Thread.Sleep(200);
            _tagListener.ReadTagSynchronously(_tag);
            Type typeAfterTagListener = _tag.Value.GetType();
            object valueAfterTagListener = _tag.Value.Clone();

            typeAfterTagController.Should().Be(typeAfterTagListener);
            valueAfterTagController.Should().Be(valueAfterTagListener);
        }

        // TagController and TagListener consistency check
        [Test]
        public void ConsistencyCheck_GivenTagControllerWroteArrayValue_WhenTagListenerReadsIt_TagValueShouldNotBeDifferent()
        {
            var arrayTag = new Tag("fbAGS_1.aTestArray", "MiddlePRG_1", "ARRAY[0..10] OF DINT", Global.AdsPort);
            _tagController.WriteTag(arrayTag, new []{1,0,0,0,0,0,0,0,0,0,0}).Wait(500);
            object firstValueAfterTagController = arrayTag.ArrayValues<int>().First().Clone();
            Type typeAfterTagController = firstValueAfterTagController.GetType();

            Thread.Sleep(500);
            _tagListener.ReadTagSynchronously(arrayTag);
            object firstValueAfterTagListener = arrayTag.ArrayValues<int>().First().Clone();
            Type typeAfterTagListener = firstValueAfterTagListener.GetType();

            typeAfterTagController.Should().Be(typeAfterTagListener);
            firstValueAfterTagController.Should().Be(firstValueAfterTagListener);
        }

    }
}
