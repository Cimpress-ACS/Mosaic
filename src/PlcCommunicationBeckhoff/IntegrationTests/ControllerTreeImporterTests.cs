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
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication;
using VP.FF.PT.Common.PlcEssentials;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class ControllerTreeImporterTests
    {
        const int RootCtrlId = 1; // FB_MOD
        const int CylinderCtrlId = 2; // FB_CYL

        private ITagListener _tagListener;
        private ITagImporter _tagImporter;
        private IControllerTreeImporter _testee;

        [SetUp]
        public void SetUp()
        {
            _tagListener = new BeckhoffPollingTagListener(Global.AdsAddress, Global.AdsPort, new GlobalLock());
            _tagImporter = new BeckhoffOnlineTagImporter(Global.AdsAddress, Global.AdsPort);
            _testee = new BeckhoffOnlineControllerTreeImporter(_tagImporter);

            _tagListener.StartListening();
            _testee.Initialize(_tagListener, Global.AdsAddress, Global.AdsPort);
        }

        [TearDown]
        public void TearDown()
        {
            _tagImporter.Dispose();
            _tagListener.Dispose();
            _testee = null;
        }

        [Test]
        public void ImportAllControllersTest()
        {
            IControllerTree controllerTree = _testee.ImportControllerTree(null);

            controllerTree.RootController.Should().NotBeNull();
            var rootController = controllerTree.RootController;

            rootController.Type.Should().Be("FB_MOD");
            rootController.Name.Should().Be("fbMOD_2");
            rootController.Childs.Should().NotBeEmpty();
        }

        [Test]
        public void WhenGetSpecificController_ShouldReturnInstance()
        {
            IControllerTree controllerTree = _testee.ImportControllerTree(null);

            var modController = controllerTree.TryGetController(1);

            modController.Should().NotBeNull("controller id 1 is fbMOD_2");
        }

        [Test]
        public void WhenSendControllerParameter_ShouldWriteToPlc()
        {
            IControllerTree controllerTree = _testee.ImportControllerTree(null);
            var modController = controllerTree.TryGetController(RootCtrlId);
            var parameterTag = modController.Parameters.First(t => t.NestedName == "udiWaitPick_ms");

            modController.SendParameter(new Tag("udiWaitPick_ms", string.Empty, "UDINT") { Value = 1000 }); // long syntax
            Thread.Sleep(500);
            _tagListener.ReadTagSynchronously(parameterTag);
            parameterTag.Value.Should().Be((uint)1000);

            modController.SendParameter("udiWaitPick_ms", 3000); // short syntax
            Thread.Sleep(500);
            _tagListener.ReadTagSynchronously(parameterTag);
            parameterTag.Value.Should().Be((uint)3000);
        }

        [Test]
        public void InteractWithControllerCommandTest()
        {
            IControllerTree controllerTree = _testee.ImportControllerTree(null);
            var cylinderController = controllerTree.TryGetController(CylinderCtrlId);
            cylinderController.SendParameter("udiSimExtendTime_ms", 10); // simulate faster to speed up unit test
            cylinderController.SendParameter("udiSimRetractTime_ms", 10);
            Thread.Sleep(700);

            // start cylinder controller
            var extendCommand = (from c in cylinderController.Commands
                                where c.Name.Equals("Extend", StringComparison.InvariantCultureIgnoreCase)
                                select c).First();

            extendCommand.Fire();
            Thread.Sleep(700);
            controllerTree = _testee.ImportControllerTree(null);
            cylinderController = controllerTree.TryGetController(CylinderCtrlId);
            cylinderController.CurrentState.Should().Be("cSTA_EXTEND");

            // stop cylinder controller
            var retractCommand = (from c in cylinderController.Commands
                                where c.Name.Equals("Retract", StringComparison.InvariantCultureIgnoreCase)
                                select c).First();
            retractCommand.Fire();
            Thread.Sleep(500);
            controllerTree = _testee.ImportControllerTree(null);
            cylinderController = controllerTree.TryGetController(CylinderCtrlId);
            cylinderController.CurrentState.Should().Be("cSTA_RETRACT");
        }
    }
}
