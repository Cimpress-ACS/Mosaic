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
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement
{
    [TestFixture]
    public class AlarmByIdAndSourceComparerTests
    {
        [Test]
        public void Equals_WithSameAlarmInstance_ShouldReturnTrue()
        {
            Alarm first = CreateAlarm();
            Alarm second = first;
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeTrue();
        }

        [Test]
        public void Equals_WithAlarm1FromLDS2_WithAlarm1FromLDS2_ShouldReturnTrue()
        {
            Alarm first = CreateAlarm(id: 1, source: "LDS2");
            Alarm second = CreateAlarm(id: 1, source: "LDS2");
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeTrue();
        }

        [Test]
        public void Equals_WithAlarm1FromLDS2_WithAlarm2FromLDS2_ShouldReturnFalse()
        {
            Alarm first = CreateAlarm(id: 1, source: "LDS2");
            Alarm second = CreateAlarm(id: 2, source: "LDS2");
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeFalse();
        }

        [Test]
        public void Equals_WithAlarm13FromLD2_WithAlarm452FromLDS2_ShouldReturnFalse()
        {
            Alarm first = CreateAlarm(id: 13, source: "LDS2");
            Alarm second = CreateAlarm(id: 452, source: "LDS2");
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeFalse();
        }

        [Test]
        public void Equals_WithAlarm1FromLDS2_WithAlarm1FromJET1_ShouldReturnFalse()
        {
            Alarm first = CreateAlarm(id: 1, source: "LDS2");
            Alarm second = CreateAlarm(id: 1, source: "JET_1");
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeFalse();
        }

        [Test]
        public void Equals_WithNull_WithAlarm1FromLDS1_ShouldReturnFalse()
        {
            Alarm first = null;
            Alarm second = CreateAlarm();
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeFalse();
        }

        [Test]
        public void Equals_WithAlarm1FromLDS1_WithNull_ShouldReturnFalse()
        {
            Alarm first = CreateAlarm();
            Alarm second = null;
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeFalse();
        }

        [Test]
        public void Equals_WithNull_WithNull_ShouldReturnTrue()
        {
            Alarm first = null;
            Alarm second = null;
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Equals(first, second).Should().BeTrue();
        }

        [Test]
        public void GetHashCode_OnSameAlarmInstance_ShouldEqual()
        {
            Alarm first = CreateAlarm();
            Alarm second = first;
            AssertEqualHashCodes(first, second);
        }

        [Test]
        public void GetHashCode_OnAlarm1OnLDS2_AndAlarm3OnJET1_ShouldNotEqual()
        {
            Alarm first = CreateAlarm(id: 1, source: "LDS2");
            Alarm second = CreateAlarm(id: 3, source: "JET1");
            AssertNotEqualHashCodes(first, second);
        }

        [Test]
        public void GetHashCode_OnAlarm1OnLDS2_AndAlarm1OnLDS2_ShouldEqual()
        {
            Alarm first = CreateAlarm(id: 1, source: "LDS2");
            Alarm second = CreateAlarm(id: 1, source: "LDS2");
            AssertEqualHashCodes(first, second);
        }

        [Test]
        public void GetHashCode_OnNull_ShouldThrowArgumentNullException()
        {
            Alarm first = null;
            IEqualityComparer<Alarm> comparer = CreateComparer();
            comparer.Invoking(c => c.GetHashCode(first)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void GetHashCode_OnAlarm1WithNullSource_AndAlarm2WithNullSource_ShouldNotEqual()
        {
            Alarm first = CreateAlarm(id: 1, source: null);
            Alarm second = CreateAlarm(id: 2, source: null);
            AssertNotEqualHashCodes(first, second);
        }

        [Test]
        public void GetHashCode_OnAlarm12WithNullSource_AndAlarm12WithNullSource_ShouldNotEqual()
        {
            Alarm first = CreateAlarm(id: 12, source: null);
            Alarm second = CreateAlarm(id: 12, source: null);
            AssertEqualHashCodes(first, second);
        }

        private void AssertEqualHashCodes(Alarm first, Alarm second)
        {
            IEqualityComparer<Alarm> comparer = CreateComparer();
            int secondHashCode = comparer.GetHashCode(second);
            int fistHashCode = comparer.GetHashCode(first);
            fistHashCode.Should().Be(secondHashCode);
        }

        private void AssertNotEqualHashCodes(Alarm first, Alarm second)
        {
            IEqualityComparer<Alarm> comparer = CreateComparer();
            int secondHashCode = comparer.GetHashCode(second);
            int fistHashCode = comparer.GetHashCode(first);
            fistHashCode.Should().NotBe(secondHashCode);
        }

        private IEqualityComparer<Alarm> CreateComparer()
        {
            return new AlarmByIdAndSourceComparer();
        }

        private Alarm CreateAlarm(int id = 1, string source = "LDS_1")
        {
            return new Alarm { AlarmId = id, Source = source };
        }
    }
}
