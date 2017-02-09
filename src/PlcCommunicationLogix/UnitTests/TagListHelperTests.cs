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


using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    public static class TestTags
    {
        private const string Scope = "TestModule";

        public static readonly Tag TestTag = new Tag("TestTag", Scope, "BOOL");

        public static class SubComponent
        {
            public static readonly Tag TestTag = new Tag("SubComponent.TestTag", Scope, "BOOL");

            public static class SubSubComponent
            {
                public static readonly Tag TestTag = new Tag("SubComponent.SubComponent.TestTag", Scope, "BOOL");
            }
        }

        public static readonly Tag AnotherTestTag = new Tag("AnotherTestTag", Scope, "BOOL");
    }

    [TestFixture]
    public class TagListHelperTests
    {
        [Test]
        public void WhenParsingTestTags_MustHaveCount_4()
        {
            var result = TagListHelper.ParseTags(typeof(TestTags));

            result.Should().HaveCount(4, "4 Tags are defined in TestTags class (including all sub components)");
        }

        [Test]
        public void WhenParsingTestTags_TagObjectsMustBeOfSameInstance()
        {
            var result = TagListHelper.ParseTags(typeof(TestTags));

            object testTag = (from r in result
                              where r.Name == "TestTag"
                              select r).First();

            testTag.Should().BeSameAs(TestTags.TestTag, "just a copy of the Tag is not good enough");
        }

        [Test]
        public void WhenParsingTestTags_DeepNestedObjectMustBeOfSameInstance()
        {
            var result = TagListHelper.ParseTags(typeof(TestTags));

            object nestedTestTag = (from r in result
                                    where r.Name == "SubComponent.SubComponent.TestTag"
                                    select r).First();

            nestedTestTag.Should().BeSameAs(TestTags.SubComponent.SubSubComponent.TestTag, "just a copy of the Tag is not good enough");
        }
    }
}
