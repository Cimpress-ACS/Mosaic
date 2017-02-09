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

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class TagHelperTests
    {
        private readonly Tag _rootTag = new Tag("root", "scope");
        private readonly Tag _child1 = new Tag("root.child1", "scope");
        private readonly Tag _child2 = new Tag("root.child2", "scope");
        private readonly Tag _nestedChild = new Tag("root.child1.nestedChild", "scope");

        [SetUp]
        public void Setup()
        {
            _rootTag.Childs.Add(_child1);
            _rootTag.Childs.Add(_child2);
            _child1.Childs.Add(_nestedChild);
        }

        [Test]
        public void Test_GetChildTag()
        {
            var result = TagHelper.GetChildTag(_rootTag, "child2");

            result.Should().Be(_child2);
        }

        [Test]
        public void Test_SearchChildTags()
        {
            var root = new Tag("root", "scope");
            var child1 = new Tag("child1", "scope");
            var child2 = new Tag("child2", "scope");
            var child3 = new Tag("child3", "scope");
            root.Childs.Add(child1);
            root.Childs.Add(child2);
            root.Childs.Add(child3);
            var childchild1 = new Tag("childchild", "scope");
            var childchild2 = new Tag("childchild", "scope");
            child2.Childs.Add(childchild1);
            child3.Childs.Add(childchild2);

            var result = TagHelper.SearchChildTags(root, "child1");
            result.Should().Contain(child1);

            var result1 = TagHelper.SearchChildTags(root, "childchild");
            result1.Should().HaveCount(2, "there are two tags in the tree named childchild (with different scope)");
            result1.First().Should().Be(childchild1);
            result1.Last().Should().Be(childchild2);

            var result2 = TagHelper.SearchChildTags(child1, "root");
            result2.Should().BeEmpty("search works top down, not bottom up");
        }

        [Test]
        public void Test_NestedFullPath_GetChildTag()
        {
            var result = TagHelper.GetChildTag(_rootTag, "child1.nestedChild");

            result.Should().Be(_nestedChild);
        }
    }
}
