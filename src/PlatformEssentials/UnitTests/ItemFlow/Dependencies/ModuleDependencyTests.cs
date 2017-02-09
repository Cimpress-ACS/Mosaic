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
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.ItemFlow.Dependencies
{
    [TestFixture]
    public class ModuleDependencyTests
    {
        private ModuleDependency _testee;
        private Mock<ITrigger> _trigger;
        private Mock<IAction> _action;
        private Mock<ILogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger>();

            _testee = new ModuleDependency(_logger.Object);
            _testee.IsEnabled = true;

            _trigger = new Mock<ITrigger>();
            _action = new Mock<IAction>();
        }

        [Test]
        public void WhenTriggerOccurres_ShouldExecuteAllActions()
        {
            _trigger = new Mock<ITrigger>();
            var action1 = new Mock<IAction>();
            var action2 = new Mock<IAction>();
            _testee.AddTrigger(_trigger.Object);
            _testee.AddAction(action1.Object);
            _testee.AddAction(action2.Object);

            _trigger.Raise(t => t.TriggerOccurred += (s, a) => {}, new EventArgs());

            action1.Verify(a => a.Execute(), Times.Once);
            action2.Verify(a => a.Execute(), Times.Once);
        }

        [Test]
        public void GivenDisabled_WhenTriggerOccurres_NothingHappens()
        {
            _testee.AddTrigger(_trigger.Object);
            _testee.AddAction(_action.Object);
            _testee.IsEnabled = false;

            _trigger.Raise(t => t.TriggerOccurred += (s, a) => { }, new EventArgs());

            _action.Verify(a => a.Execute(), Times.Never);
        }

        [Test]
        public void GivenConditionFalse_WhenTrigger_NothingHappens()
        {
            var condition = new Mock<ICondition>();
            condition
                .Setup(c => c.IsTrue())
                .Returns(false);
            _testee.AddTrigger(_trigger.Object);
            _testee.AddAction(_action.Object);
            _testee.AddCondition(condition.Object);

            _trigger.Raise(t => t.TriggerOccurred += (s, a) => { }, new EventArgs());

            _action.Verify(a => a.Execute(), Times.Never);
        }

        [Test]
        public void GivenConditionTrue_WhenTrigger_ShouldExecuteAction()
        {
            var condition = new Mock<ICondition>();
            condition
                .Setup(c => c.IsTrue())
                .Returns(true);
            _testee.AddTrigger(_trigger.Object);
            _testee.AddAction(_action.Object);
            _testee.AddCondition(condition.Object);

            _trigger.Raise(t => t.TriggerOccurred += (s, a) => { }, new EventArgs());

            _action.Verify(a => a.Execute(), Times.Once);
        }

        [Test]
        public void WhenTrigger_ShouldLog()
        {
            _trigger = new Mock<ITrigger>();
            _testee.AddTrigger(_trigger.Object);
            _testee.AddAction(new Mock<IAction>().Object);

            _trigger.Raise(t => t.TriggerOccurred += (s, a) => { }, new EventArgs());

            _logger.Verify(l => l.DebugFormat(It.IsAny<string>(), It.IsAny<object[]>()));
        }
    }
}
