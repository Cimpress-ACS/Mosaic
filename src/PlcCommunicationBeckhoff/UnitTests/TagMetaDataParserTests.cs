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
using NUnit.Framework;
using FluentAssertions;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class TagMetaDataParserTests
    {
        private ITagMetaDataParser _testee;
        private const string PtrMetadataSkeleton = "   {{readptr}}      {0}       ";
        private const string Comment = "This is a comment.";
        private string _ptrRawMetaData;

        [SetUp]
        public void SetUp()
        {
            _testee = new TagMetaDataParser();
             _ptrRawMetaData = string.Format(PtrMetadataSkeleton, Comment);
        }

        [TearDown]
        public void TearDown()
        {
            _testee = null;
        }

        [Test]
        public void ExtractComment()
        {
            var metadata = _testee.Parse(_ptrRawMetaData);

            metadata.Comment.Should().Be(Comment);
        }

        [Test]
        public void ExtractReadPointer()
        {
            var metadata = _testee.Parse(_ptrRawMetaData);

            metadata.ReadPointer.Should().BeTrue();
        }

        [Test]
        public void WhenRawStringIsEmpty_ShouldSetNoMetadata()
        {
            var metadata = _testee.Parse(string.Empty);

            metadata.Comment.Should().Be(string.Empty);
            metadata.ReadPointer.Should().BeFalse();
            metadata.Minimum.Should().BeNull();
            metadata.Maximum.Should().BeNull();
        }

        [Test]
        public void WhenRawStringIsNull_ShouldIgnore()
        {
            TagMetaData metadata = null;
            var act = new Action(() => metadata = _testee.Parse(null));

            act.ShouldNotThrow();
            metadata.Comment.Should().BeEmpty();
        }

        [Test]
        public void WhenRawStringIsEmpty_ShouldIgnore()
        {
            TagMetaData metadata = null;
            var act = new Action(() => metadata = _testee.Parse(string.Empty));

            act.ShouldNotThrow();
            metadata.Comment.Should().BeEmpty();
        }

        [Test]
        public void ExtractEnumerationMembers()
        {
            var metadata = _testee.Parse(Comment + "  {enum 0=Start;1=Pause;2=Stop}  ");

            metadata.Comment.Should().Be(Comment);
            metadata.EnumerationMembers.Should().HaveCount(3);
            metadata.EnumerationMembers[0].Value.Should().Be(0);
            metadata.EnumerationMembers[0].Comment.Should().Be("Start");
            metadata.EnumerationMembers[1].Value.Should().Be(1);
            metadata.EnumerationMembers[1].Comment.Should().Be("Pause");
            metadata.EnumerationMembers[2].Value.Should().Be(2);
            metadata.EnumerationMembers[2].Comment.Should().Be("Stop");
        }

        [Test]
        public void ExtractEnumerationMembers_Minimum_Maximum()
        {
            var metadata = _testee.Parse("  {enum 1=min  ;   5=max;}  ");

            metadata.Minimum.Should().Be((short)1);
            metadata.Maximum.Should().Be((short)5);
        }

        [Test]
        public void ExtractEnumerationMembers_Negatives()
        {
            var metadata = _testee.Parse("{enum -1=neg;0=zero;1=one}");

            metadata.EnumerationMembers.Should().HaveCount(3);
            metadata.EnumerationMembers[0].Value.Should().Be(-1);
            metadata.EnumerationMembers[0].Comment.Should().Be("neg");
            metadata.Minimum.Should().Be((short)-1);
        }

        // the ordering of enumeration value must not necessarily match the value
        [Test]
        public void ExtractEnumerationMembers_MixedOrder()
        {
            var metadata = _testee.Parse("{enum 1=Start;0=Stop}");

            metadata.EnumerationMembers.Should().HaveCount(2);
            metadata.EnumerationMembers.First().Comment.Should().Be("Start");
            metadata.EnumerationMembers.Last().Comment.Should().Be("Stop");
        }

        [Test]
        public void ExtractEnumerationMembers_SpecialFormatting()
        {
            var metadata = _testee.Parse(
@"{enum 
    0 = Start;
    1 = Pause;
    2 = Stop;}");

            metadata.EnumerationMembers.Should().HaveCount(3);
        }

        [Test]
        public void ExtractEnumerationMembers_WithWhitespaces()
        {
            var metadata = _testee.Parse(@"{enum 0= Start Module ;1=Pause Module;2=Stop Module;}");

            metadata.EnumerationMembers.Should().HaveCount(3);
            metadata.EnumerationMembers.First().Comment.Should().Be("Start Module");
        }
    }
}
