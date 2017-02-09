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
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow.Dependencies
{
    [TestFixture]
    public class EventNotTriggeredTests
    {
        private EventNotRaisedTrigger _testee;
        private Test _objectWithEvent;

        [SetUp]
        public void SetUp()
        {
            _objectWithEvent = new Test();
            _testee = new EventNotRaisedTrigger(_objectWithEvent, "TestEvent", TimeSpan.FromMilliseconds(300));
            _testee.MonitorEvents();
        }
        
        [TearDown]
        public void TearDown()
        {
            _objectWithEvent = null;
            _testee.Dispose();
            _testee = null;
        }

        [Test]
        public void WhenObjectDoesNotExist_ShouldThrow()
        {
            Action action = () => new EventNotRaisedTrigger(null, "TestEvent", TimeSpan.Zero);

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void WhenEventDoesNotExist_ShouldThrow()
        {
            Action action = () => new EventNotRaisedTrigger(_objectWithEvent, "--eventdoesnotexist--", TimeSpan.Zero);

            action.ShouldThrowExactly<DependencyException>();
        }

        [Test]
        public void GivenJustInitialized_AfterTimeout_ShouldNotTrigger()
        {
            bool triggerOccurred = false;
            _testee.TriggerOccurred += (s, a) => triggerOccurred = true;

            Thread.Sleep(1000);

            triggerOccurred.Should().BeFalse("the timeout should only start after the event raised at least once");
        }

        [Test]
        public void GivenEventRaised_AfterTimeout_ShouldNotTrigger()
        {
            bool triggerOccurred = false;
            _testee.TriggerOccurred += (s, a) => triggerOccurred = true;
            _objectWithEvent.RaiseEvent();
            
            Thread.Sleep(1000);

            triggerOccurred.Should().BeTrue();
        }

        [Test]
        public void AfterAlreadyTriggered_WhenEventRaisedAgain_AfterTimeout_ShouldTrigger()
        {
            GivenEventRaised_AfterTimeout_ShouldNotTrigger();
            bool triggerOccurredAgain = false;
            _testee.TriggerOccurred += (s, a) => triggerOccurredAgain = true;

            _objectWithEvent.RaiseEvent();

            Assert.That(
                () => triggerOccurredAgain,
                Is.EqualTo(true).After(1000));
        }

        [Test]
        public void GivenEventRaised_WithinTimeout_ShouldNotTrigger()
        {
            _objectWithEvent.RaiseEvent();

            _testee.ShouldNotRaise("TriggerOccurred");
        }

        [Test]
        public void GivenCombinedWithEventTrigger_WhenEventOccures_ShouldOnlyTriggerEvent()
        {
            bool eventNotRaisedTriggerOccurred = false;
            bool eventTriggerOccurred = false;

            var eventTrigger = new EventTrigger(_objectWithEvent, "TestEvent");
            eventTrigger.TriggerOccurred += (sender, args) => eventTriggerOccurred = true;
            _testee.TriggerOccurred += (sender, args) => eventNotRaisedTriggerOccurred = true;

            _objectWithEvent.RaiseEvent();

            eventTriggerOccurred.Should().BeTrue();
            eventNotRaisedTriggerOccurred.Should().BeFalse();
        }

        private class Test
        {
            public event EventHandler<EventArgs> TestEvent;

            public void RaiseEvent()
            {
                if (TestEvent != null)
                    TestEvent(this, new EventArgs());
            }
        }
    }
}
