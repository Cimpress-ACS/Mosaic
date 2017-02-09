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


using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    [TestFixture]
    public class TagTests
    {
        [Test]
        public void Equals_WhenTagsHaveSameScopeAndName_ItIsEqual()
        {
            var tagA = new Tag {Name = "SensorA", Scope = "ProgramA"};
            var tagB = new Tag {Name = "SensorA", Scope = "ProgramA"};

            tagA.Equals(tagB).Should().BeTrue("tags have same scope and names");
        }

        [Test]
        public void Equals_WhenTagsSameNameOnly_ItIsEqual()
        {
            var tagA = new Tag {Name = "SensorA", Scope = "ProgramB"};
            var tagB = new Tag {Name = "SensorA"};

            tagA.Equals(tagB).Should().BeFalse("tags have same names but different scopes");
        }

        [Test]
        public void OperatorEqual_WhenTagsHaveSameScopeAndName_ItShouldBeTrue()
        {
            var tagA = new Tag { Name = "SensorA", Scope = "ProgramA" };
            var tagB = new Tag { Name = "SensorA", Scope = "ProgramA" };

            bool equal = tagA == tagB;

            equal.Should().BeTrue("tags have same scope and names");
        }

        [Test]
        public void OperatorNotEqual_WhenTagsHaveSameScopeAndName_ItShouldBeFalse()
        {
            var tagA = new Tag { Name = "SensorA", Scope = "ProgramA" };
            var tagB = new Tag { Name = "SensorA", Scope = "ProgramA" };

            bool equal = tagA != tagB;

            equal.Should().BeFalse("tags have same scope and names");
        }

        [Test]
        public void GivenDataTypeAlreadySet_WhenSetValue_DoNotAutomaticallyDetectDatatype()
        {
            var tag = new Tag { DataType = "myCustomUDT" };

            tag.Value = true;

            tag.DataType.Should().Be("myCustomUDT", "manually set DataType should not be overriden automatically");
        }

        [Test]
        public void Event_WhenValueChaged_ShouldRaiseTagValueChangedEvent()
        {
            var testee = new Tag("tag", "scope");
            testee.MonitorEvents();

            testee.Value = true;

            testee
                .ShouldRaise("ValueChanged")
                .WithSender(testee)
                .WithArgs<TagValueChangedEventArgs>(a => (bool)a.Value == true);
        }
    }
}
