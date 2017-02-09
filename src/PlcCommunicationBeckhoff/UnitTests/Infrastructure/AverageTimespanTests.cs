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
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests.Infrastructure
{
    [TestFixture]
    public class AverageTimespanTests
    {
        private AverageTimespan _testee;

        [SetUp]
        public void SetUp()
        {
            _testee = new AverageTimespan(3);
        }

        [Test]
        public void WhenAddingValues_AverageCalculationMustBeCorrect()
        {
             _testee.AddTimestampProbe(CreateDateTime(10));
             _testee.AddTimestampProbe(CreateDateTime(20));

            _testee.Average().Should().Be(10);
        }

        [Test]
        public void WhenAddingDifferentValues_AverageCalculationMustBeCorrect()
        {
            _testee.AddTimestampProbe(CreateDateTime(10));
            _testee.AddTimestampProbe(CreateDateTime(20));
            _testee.AddTimestampProbe(CreateDateTime(40));

            _testee.Average().Should().Be(15, "first timespan is 10ms and second timespan is 20ms");
        }

        [Test]
        public void WhennAddingEnoughValues_EventWithCorrectCalculationMustBeRaised()
        {
            _testee.MonitorEvents();

            _testee.AddTimestampProbe(CreateDateTime(10));
            _testee.AddTimestampProbe(CreateDateTime(20));
            _testee.AddTimestampProbe(CreateDateTime(30));
            _testee.AddTimestampProbe(CreateDateTime(40));

            _testee
                .ShouldRaise("AverageChanged", "added four values which means three timespans and testee was initialized with 3")
                .WithSender(_testee)
                .WithArgs<AverageTimespan.AverageTimespanEventArgs>(args => args.Average == 10);
        }

        private DateTime CreateDateTime(int milliseconds)
        {
            var dateTime = new DateTime();
            return dateTime.AddMilliseconds(milliseconds);
        }
    }
}
