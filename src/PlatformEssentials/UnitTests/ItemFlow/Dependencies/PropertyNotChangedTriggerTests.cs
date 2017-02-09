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
    public class PropertyNotChangedTriggerTests
    {
        private PropertyNotChangedTrigger _testee;
        private Test _objectWithProperty;

        [SetUp]
        public void SetUp()
        {
            _objectWithProperty = new Test();
            _testee = new PropertyNotChangedTrigger(_objectWithProperty, "TestProperty", TimeSpan.FromMilliseconds(300));
            _testee.MonitorEvents();
        }

        [TearDown]
        public void TearDown()
        {
            _objectWithProperty = null;
            _testee.Dispose();
            _testee = null;
        }

        [Test]
        public void WhenObjectDoesNotExist_ShouldThrow()
        {
            Action action = () => new PropertyNotChangedTrigger(null, "TestProperty", TimeSpan.Zero);

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void WhenPropertyDoesNotExist_ShouldThrow()
        {
            Action action = () => new PropertyNotChangedTrigger(_objectWithProperty, "--propertydoesnotexist--", TimeSpan.Zero);

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void GivenJustInitialized_AfterTimeout_ShouldNotTrigger()
        {
            bool triggerOccurred = false;
            _testee.TriggerOccurred += (s, a) => triggerOccurred = true;



            Assert.That(
                () => triggerOccurred, 
                Is.EqualTo(false).After(1000), "the timeout should only start if the property changed at least once");
        }

        [Test]
        public void GivenPropertyChanged_AfterTimeout_ShouldNotTrigger()
        {
            bool triggerOccurred = false;

            _testee.TriggerOccurred += (s, a) => triggerOccurred = true;
            _objectWithProperty.TestProperty = 777;

            Assert.That(
                () => triggerOccurred,
                Is.EqualTo(true).After(1000));
        }

        [Test]
        public void AfterAlreadyTriggered_WhenPropertyChangedAgain_AfterTimeout_ShouldTrigger()
        {
            GivenPropertyChanged_AfterTimeout_ShouldNotTrigger();
            bool triggerOccurredAgain = false;
            _testee.TriggerOccurred += (s, a) => triggerOccurredAgain = true;

            _objectWithProperty.TestProperty = 1545483;

            Assert.That(
                () => triggerOccurredAgain,
                Is.EqualTo(true).After(1000));
        }

        [Test]
        public void GivenPropertyChanged_WithinTimeout_ShouldNotTrigger()
        {
            _objectWithProperty.TestProperty = 777;

            _testee.ShouldNotRaise("TriggerOccurred");
        }

        [Test]
        public void WhenApplyTriggerOnBaseClassProperty_ShouldNotThrow()
        {
            var derived = new DerivedTest();
            Action action = () => new PropertyNotChangedTrigger(derived, "TestProperty", TimeSpan.Zero);

            action.ShouldNotThrow();
        }

        private class Test
        {
            public int TestProperty { get; set; }
        }

        private class DerivedTest : Test
        {
        }
    }
}
