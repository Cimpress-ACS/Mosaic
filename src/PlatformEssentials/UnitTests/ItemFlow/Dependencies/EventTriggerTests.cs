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
    public class EventTriggerTests
    {
        private EventTrigger _testee;
        private Test<EventArgs> _objectWithEvent;

        [SetUp]
        public void SetUp()
        {
            _objectWithEvent = new Test<EventArgs>();
            _testee = new EventTrigger(_objectWithEvent, "TestEvent");
            _testee.MonitorEvents();
        }

        [Test]
        public void GivenStandardEvent_WhenEventOccures_TriggerShouldFire()
        {
            _objectWithEvent.RaiseTestEvent();

            _testee.ShouldRaise("TriggerOccurred").WithSender(_testee);
        }

        [Test]
        public void WhenEventDoesNotExist_ShouldThrow()
        {
            Action action = () => new EventTrigger(_objectWithEvent, "doesnotexist");

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void WhenObjectDoesNotExist_ShouldThrow()
        {
            Action action = () => new EventTrigger(null, "TestEvent");

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void GivenCustomEvent_WhenEventOccurres_TriggerShouldFire()
        {
            var objectWithEvent = new Test<CustomEventArgs>();
            var testee = new EventTrigger(objectWithEvent, "TestEvent");
            testee.MonitorEvents();

            objectWithEvent.RaiseTestEvent();

            testee.ShouldRaise("TriggerOccurred").WithSender(testee);
        }

        [Test]
        public void WhenEventOccurresMultipleTimes_TriggerShouldFireAsOften()
        {
            int triggerOccurred = 0;
            var objectWithEvent = new Test<CustomEventArgs>();
            var testee = new EventTrigger(objectWithEvent, "TestEvent");
            testee.TriggerOccurred += (sender, args) => triggerOccurred++;

            objectWithEvent.RaiseTestEvent();
            objectWithEvent.RaiseTestEvent();
            objectWithEvent.RaiseTestEvent();

            triggerOccurred.Should().Be(3);
        }
    }

    public class Test<TEventArgs> where TEventArgs : new()
    {
        public event EventHandler<TEventArgs> TestEvent;

        public void RaiseTestEvent()
        {
            if (TestEvent != null)
                TestEvent(this, new TEventArgs());
        }
    }

    public class CustomEventArgs : EventArgs
    {
    }
}
