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
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow.Dependencies
{
    [TestFixture]
    public class ExecuteMethodActionTests
    {
        private InvokeMethodAction _testee;
        private Test _targetObject;

        [SetUp]
        public void SetUp()
        {
            _targetObject = new Test();
            _testee = new InvokeMethodAction(_targetObject, "TestMethod");
        }

        [Test]
        public void WhenInitialize_ShouldCallNothing()
        {
            _targetObject.TestMethodCalledCount.Should().Be(0);
        }

        [Test]
        public void WhenExecuteAction_ShouldCallMethod()
        {
            _testee.Execute();

            _targetObject.TestMethodCalledCount.Should().Be(1);
        }

        [Test]
        public void GivenMethodNotExisting_WhenExecuteAction_ShouldThrow()
        {
            Action  action = () => new InvokeMethodAction(_targetObject, "methoddoesnotexist");

            action.ShouldThrowExactly<DependencyException>();
        }

        public class Test
        {
            public int TestMethodCalledCount { get; private set; }

            public void TestMethod()
            {
                TestMethodCalledCount++;
            }
        }
    }
}
