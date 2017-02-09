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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationLogix.ASCommImplementation;
using VP.FF.PT.Common.PlcCommunicationLogix.IngearImplementation;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    /// <summary>
    /// Base class provides reusable tests for several ITagListener implementations.
    /// </summary>
    public abstract class TagListenerTestsBase
    {
        protected const int AdsPort = 851;
        protected ITagListener Testee;

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
            var tag = new Tag("tag", "scope");
            Testee.AddTag(tag);
            Testee.GetTagStream().Subscribe(t => tmpTagReceived = t);

            tag.Value = 1;

            tmpTagReceived.Should().Be(tag);
        }
    }

    [TestFixture]
    public class IngearTagListenerTests : TagListenerTestsBase
    {
        [SetUp]
        public void Setup()
        {
            Testee = new IngearTagListener("192.168.1.123");
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
    [Ignore("does not work because ASComm licensing stuff...")]
    public class ASCommTagListenerTests : TagListenerTestsBase
    {
        [SetUp]
        public void Setup()
        {
            Testee = new ASCommTagListener("192.168.1.123");
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
