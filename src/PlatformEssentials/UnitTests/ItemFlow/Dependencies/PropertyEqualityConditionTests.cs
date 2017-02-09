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
using System.Xml;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;
using VP.FF.PT.Common.PlatformEssentials.UnitTests.StateTestNamespace;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow.Dependencies
{
    [TestFixture]
    public class PropertyEqualityConditionTests
    {
        private PropertyEqualityCondition _testee;
        private Test _objectToCheck;

        [SetUp]
        public void SetUp()
        {
            _objectToCheck = new Test();
            _testee = new PropertyEqualityCondition(_objectToCheck, "PropertyTest", "hello");
        }

        [Test]
        public void GivenConditionTrue_WhenCheck_ShouldBeTrue()
        {
            _objectToCheck.PropertyTest = "hello";

            _testee.IsTrue().Should().BeTrue();
        }

        [Test]
        public void GivenConditionFalse_WhenCheck_ShouldBeFalse()
        {
            _objectToCheck.PropertyTest = "--unexpected property value test--";

            _testee.IsTrue().Should().BeFalse();
        }

        [Test]
        public void GivenPropertyNotExisting_WhenCheck_ShouldThrow()
        {
            Action action = () => new PropertyEqualityCondition(_objectToCheck, "--propertydoesnotexist--", null);

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void WhenCheckPrivateProperty_ShouldBeTrue()
        {
            _objectToCheck.SetPrivateInt(7);

            var testee = new PropertyEqualityCondition(_objectToCheck, "PrivateIntTest", 7);

            testee.IsTrue().Should().BeTrue();
        }

        [Test]
        public void GivenEnumProperty_WhenCheckValue_ShouldBeTrue()
        {
            _objectToCheck.EnumStateTest = State.Run;

            var testee = new PropertyEqualityCondition(_objectToCheck, "EnumStateTest", State.Run);

            testee.IsTrue().Should().BeTrue();
        }

        [Test]
        public void GivenEnumProperty_WhenCheckValueAsString_ShouldBeTrue()
        {
            _objectToCheck.EnumStateTest = State.Run;

            var testee = new PropertyEqualityCondition(_objectToCheck, "EnumStateTest", "Run");

            testee.IsTrue().Should().BeTrue();
        }

        [Test]
        public void GivenEnumProperty_GivenDifferentValue_WhenCheckValueAsString_ShouldBeFalse()
        {
            _objectToCheck.EnumStateTest = State.Run;

            var testee = new PropertyEqualityCondition(_objectToCheck, "EnumStateTest", "Off");

            testee.IsTrue().Should().BeFalse();
        }

        [Test]
        public void MustSupportEmptyString()
        {
            _objectToCheck.PropertyTest = "";
            var testee = new PropertyEqualityCondition(_objectToCheck, "PropertyTest", "");

            Action action = () => testee.IsTrue();

            action.ShouldNotThrow();
        }

        [Test]
        public void WhenApplyConditionToDerivedClass_ShouldNotThrow()
        {
            var derived = new DerivedTest();
            derived.PropertyIntTest = 1;
            var testee = new PropertyEqualityCondition(derived, "PropertyIntTest", 1);

            Action action = () => testee.IsTrue();

            action.ShouldNotThrow();
        }

        [Test]
        public void WhenApplyConditionToDerivedClassWithProtectedVirtualReadonlyProperty_ShouldNotThrow()
        {
            var derived = new DerivedTest();
            var testee = new PropertyEqualityCondition(derived, "AbstractProtectedPropertyTest", 99);
            derived.SetTestValue(99);

            Action action = () => testee.IsTrue();

            action.ShouldNotThrow();
            testee.IsTrue().Should().BeTrue();

            derived.SetTestValue(10);
            testee.IsTrue().Should().BeFalse();
        }
        
        private class Test
        {
            public Test()
            {
                EnumStateTest = State.Off;
            }

            public string PropertyTest { get; set; }

            public int PropertyIntTest { get; set; }

            private int PrivateIntTest { get; set; }

            public void SetPrivateInt(int num)
            {
                PrivateIntTest = num;
            }

            protected virtual int AbstractProtectedPropertyTest { get; private set; }

            public State EnumStateTest { get; set; }
        }

        private class DerivedTest : Test
        {
            private int _abstractProtectedPropertyTest;

            public void SetTestValue(int value)
            {
                _abstractProtectedPropertyTest = value;
            }

            protected override int AbstractProtectedPropertyTest
            {
                get
                {
                    return _abstractProtectedPropertyTest;
                }
            }
        }
    }
}

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.StateTestNamespace
{
    public enum State
    {
        Run,
        Off
    }
}
