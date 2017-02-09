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
using System.ComponentModel.Composition;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcCommunicationSimulator.Behavior;
using VP.FF.PT.Common.PlcEssentials;
using VP.FF.PT.Common.PlcEssentials.ControllerImporting;
using VP.FF.PT.Common.PlcEssentials.Impl;
using Command = VP.FF.PT.Common.PlcEssentials.Impl.Command;

namespace VP.FF.PT.Common.PlcCommunicationSimulator
{
    [Export(typeof(IControllerTreeImporter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SimulatedControllerTreeImporter : IControllerTreeImporter, IPartImportsSatisfiedNotification
    {
        private readonly ControllerTree _controllerTree;
        private readonly ITagController _tagController;

        [Import]
        internal ISimulatedBehaviorManagerInternal SimulatedBehaviorManager = null;

        [ImportingConstructor]
        public SimulatedControllerTreeImporter(ITagController tagController)
        {
            _tagController = tagController;
            _controllerTree = new ControllerTree();
        }

        public IAlarmsImporter AlarmsImporter
        {
            get { return new SimulatedAlarmsImporter(); }
        }

        public IControllerTree ControllerTree
        {
            get { return _controllerTree; }
        }

        public void Initialize(ITagListener tagListener, string path, int port = 0, string rootController = "")
        {
        }

        public void ImportAllControllers()
        {
            ImportAllControllers(null);
        }

        public void ImportAllControllers(IList<string> userDefinedInterfaces)
        {
            ControllerTree.Initialize(
                 Controller(1, "SimulatedController",
                                Controller(21, "Level2Simulated",
                                                Controller(31, "Level3Simulated")),
                                Controller(22, "Level2Simulated II")));
        }

        public void UpdateImportedControllers()
        {
        }

        public IControllerTree ImportControllerTree(IList<string> userDefinedInterfaces)
        {
            ImportAllControllers();
            return _controllerTree;
        }

        public IControllerTree ImportControllerTree()
        {
            return ImportControllerTree(null);
        }

        private Controller Controller(short id, string name, params Controller[] children)
        {
            var commonInterface = new CtrlCommonInterface
            {
                Info = new CtrlInfo
                {
                    CtrlId = id,
                    Name = name,
                    Type = "Simulated Controller",
                },
                SoftwareConfig = new CtrlSoftwareConfig
                {
                    Enable = true,
                    Simulation = true,
                },
                Job = new CtrlJob
                {
                    EnableForcing = true,
                    Mode = (short) ControllerMode.Auto,
                    StateName = "cSTA_Running",
                    SubStateName = "cSTA_Simulated",
                }
            };
            var controllerTag = new Tag(name, "Simulation");
            var commonInterfaceTag = new Tag(name.CommonInterface(), "Simulation", "Ctr_Int") { Value = commonInterface };
            controllerTag.Childs.Add(commonInterfaceTag);
            var controllerTagWrapper = new ControllerTagWrapper(controllerTag);
            var controller = new Controller(_tagController, controllerTagWrapper);

            foreach (Controller child in children)
                controller.AddChild(child);
            controller.UpdateCommands(new[] {StopCommand()});
            controller.AddActualValues(new[] {ThroughputValue()});
            controller.AddConfigurations(new[] {VelocityParameter(), RunningTag()});
            controller.AddInputs(new[] {RunningTag()});
            controller.AddOutputs(new[] {RunningTag()});
            controller.AddParameters(new[] { VelocityParameter() });
            return controller;
        }

        private static Alarm WarningAlarm()
        {
            var innerStruct = new PlcAlarmStruct {Id = 123, AlarmClass = (short) AlarmType.Warning, Text = "Warning Alarm"};
            return new Alarm(innerStruct, DateTime.Now);
        }

        private static Command StopCommand()
        {
            var command = new PlcEssentials.Command
            {
                Available = true,
                CommandId = 1,
                Name = "Stop"
            };
            return new Command(null, command, new Tag[0]);
        }

        private static Tag ThroughputValue()
        {
            var throughputValue = new Tag
            {
                DataType = IEC61131_3_DataTypes.Int,
                Name = "Throughput",
                Value = 2140,
                MetaData =
                {
                    UnitForUser = "Shirts per minute",
                    Comment = "Measures the shirt throughput of this controller."
                }
            };
            return throughputValue;
        }

        private static Tag VelocityParameter()
        {
            return new Tag
            {
                DataType = IEC61131_3_DataTypes.USInt,
                Name = "Velocity",
                MetaData =
                {
                    UnitForUser = "m/s",
                    Comment = "The velocity of the whatever",
                },
                Value = 12
            };
        }

        private static Tag RunningTag()
        {
            return new Tag
            {
                DataType = IEC61131_3_DataTypes.Boolean,
                Name = "Running",
                MetaData =
                {
                    UnitForUser = null,
                    Comment = "Specifies if the system is running.",
                },
                Value = true
            };
        }

        public void OnImportsSatisfied()
        {
            SimulatedBehaviorManager.AddControllerTreeImporter(this);
        }
    }
}
