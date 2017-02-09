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
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    /// <summary>
    /// Base class provides reusable tests for several ITagListener implementations.
    /// </summary>
    public abstract class TagListenerTestsBase
    {
        protected const int AdsPort = 851;
        protected ITagListener Testee;

        /// <summary>
        /// Each tag must be unique.
        /// In case of Beckhoff PLC system the combination of tag scope, name and ads-port is required for identification.
        /// Therefore the tag listener should add the ads-port information to the tag, if not already set.
        /// </summary>
        public virtual void GivenTagWithoutAdsPort_WhenAddingTag_TagListenerMustAssignAdsPort()
        {
            var tag = new Tag("name", "scope", "BOOL");

            Testee.AddTag(tag);

            tag.AdsPort.Should().Be(AdsPort, "TagListener musst assign ads port to tag if not set, to have unique tags everywhere");
        }

        public virtual void WhenAddEqualTagTwice_TagListenerShouldOnlyContainOne()
        {
            var tag1 = new Tag("name", "scope", "BOOL", 850);
            var tag2 = new Tag("name", "scope", "BOOL", 850);

            Testee.AddTag(tag1);
            Testee.AddTag(tag2);

            Testee.GetTags().Count.Should().Be(1);
        }

        public virtual void WhenAddTagsRecursively_AllChildsMustAvailable()
        {
            var rootTag = new Tag("root", "scope");
            rootTag.Childs.Add(new Tag("child1", "scope"));
            rootTag.Childs.Add(new Tag("child2", "scope"));

            Testee.AddTagsRecursively(rootTag);

            Testee.GetTags().Count.Should().Be(3);
        }

        public virtual void GetTagStream_ReactiveExtensionsTest()
        {
            Tag tmpTagReceived = null;
            var tag = new Tag();
            Testee.AddTag(tag);
            Testee.GetTagStream().Subscribe(t => tmpTagReceived = t);

            tag.Value = 1;

            tmpTagReceived.Should().Be(tag);
        }
    }

    [TestFixture]
    public class BeckhoffTagListenerTests : TagListenerTestsBase
    {
        [SetUp]
        public void Setup()
        {
            Testee = new BeckhoffTagListener("192.168.1.123.1.1", AdsPort);
        }

        [Test]
        public override void GivenTagWithoutAdsPort_WhenAddingTag_TagListenerMustAssignAdsPort()
        {
            base.GivenTagWithoutAdsPort_WhenAddingTag_TagListenerMustAssignAdsPort();
        }

        [Test]
        public override void WhenAddEqualTagTwice_TagListenerShouldOnlyContainOne()
        {
            base.WhenAddEqualTagTwice_TagListenerShouldOnlyContainOne();
        }

        [Test]
        public override void WhenAddTagsRecursively_AllChildsMustAvailable()
        {
            base.WhenAddTagsRecursively_AllChildsMustAvailable();
        }
        
        [Test]
        public override void GetTagStream_ReactiveExtensionsTest()
        {
            base.GetTagStream_ReactiveExtensionsTest();
        }
    }

    [TestFixture]
    public class BeckhoffPollingTagListenerTests : TagListenerTestsBase
    {
        [SetUp]
        public void Setup()
        {
            Testee = new BeckhoffPollingTagListener("192.168.1.123.1.1", AdsPort, new GlobalLock());
        }

        [Test]
        public override void GivenTagWithoutAdsPort_WhenAddingTag_TagListenerMustAssignAdsPort()
        {
            base.GivenTagWithoutAdsPort_WhenAddingTag_TagListenerMustAssignAdsPort();
        }

        [Test]
        public override void WhenAddEqualTagTwice_TagListenerShouldOnlyContainOne()
        {
            base.WhenAddEqualTagTwice_TagListenerShouldOnlyContainOne();
        }

        [Test]
        public override void WhenAddTagsRecursively_AllChildsMustAvailable()
        {
            base.WhenAddTagsRecursively_AllChildsMustAvailable();
        }

        [Test]
        public override void GetTagStream_ReactiveExtensionsTest()
        {
            base.GetTagStream_ReactiveExtensionsTest();
        }
    }
}
