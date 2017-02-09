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
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationSimulator;
using VP.FF.PT.Common.PlcEssentials.ControllerImporting;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcEssentials.UnitTests.Impl
{
    [TestFixture]
    public class ControllerTreeTests
    {
        [Test]
        public void WhenUpdatingAlarms_GivenEmptyListAndNoAlarms_DoesNotThrowError()
        {
            var tree = SetupControllerTree();

            Assert.DoesNotThrow(() => tree.UpdateAlarms(Enumerable.Empty<Alarm>()));
        }

        [Test]
        public void WhenUpdatingAlarms_GivenEmptyList_DeletesAlarmsFromControllerAndTree()
        {
            var tree = SetupControllerTree();

            var controller = tree.GetAllControllers().First();
            var alarm = new Alarm(new PlcAlarmStruct(), DateTime.Now);
            controller.TryAddAlarm(alarm);

            bool removedFromTree = false;
            tree.AlarmsRemoved += alarms => removedFromTree = alarms.Single() == alarm;
            
            bool removedFromController = false;
            controller.AlarmRemoved += () => removedFromController = true;

            // coming up with an empty list of alarms makes it delete the alarm at the controller since that's the "diff"
            tree.UpdateAlarms(Enumerable.Empty<Alarm>());

            removedFromTree.Should().BeTrue();
            removedFromController.Should().BeTrue();
        }

        [Test]
        public void WhenUpdatingAlarms_GivenNewAlarm_AddsAlarmToControllerAndTree()
        {
            var tree = SetupControllerTree();

            var controller = tree.GetAllControllers().First();
            var alarm = new Alarm(new PlcAlarmStruct {SourceId = 1, Id = 1}, DateTime.Now);
            
            bool addedToTree = false;
            tree.AlarmsAdded += alarms => addedToTree = alarms.Single() == alarm;

            bool addedToController = false;
            controller.AlarmAdded += () => addedToController = true;

            // coming up with an empty list of alarms makes it delete the alarm at the controller since that's the "diff"
            tree.UpdateAlarms(new List<Alarm> {alarm});

            addedToTree.Should().BeTrue();
            addedToController.Should().BeTrue();
        }

        private static ControllerTree SetupControllerTree()
        {
            var tree = new ControllerTree();
            var tag = new Tag();
            tag.Childs.Add(new Tag(NamingConventions.CommonInterface, "MAIN") {Value = new CtrlCommonInterface() {Info = new CtrlInfo() {CtrlId = 1}}});
            var controller = new Controller(new SimulatedTagController(null), new ControllerTagWrapper(tag));
            tree.Initialize(controller);
            return tree;
        }
    }
}
