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
    public class PropertyChangedTriggerTests
    {
        private PropertyChangedTrigger _testee;
        private Test _objectWithProperty;

        [SetUp]
        public void SetUp()
        {
            _objectWithProperty = new Test();
            _testee = new PropertyChangedTrigger(_objectWithProperty, "TestProperty");
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
            Action action = () => new PropertyChangedTrigger(null, "TestProperty");

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void WhenPropertyDoesNotExist_ShouldThrow()
        {
            Action action = () => new PropertyChangedTrigger(_objectWithProperty, "--propertydoesnotexist--");

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void GivenPropertyChanged_ShouldTrigger()
        {
            bool triggerOccurred = false;
            _objectWithProperty.TestProperty = 1.6;
            _testee.TriggerOccurred += (sender, args) => triggerOccurred = true;

            Assert.That(
                () => triggerOccurred,
                Is.True.After(500));
        }

        private class Test
        {
            public double TestProperty { get; set; }
        }
    }
}
