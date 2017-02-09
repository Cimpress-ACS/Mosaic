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


using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using Command = VP.FF.PT.Common.PlcCommunication.Impl.Command;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void TestControllerSampleGenerator()
        {
            AlarmManager alarmManager;
            Controller rootController;

            ControllerSampleGenerator.GenerateSampleControllerAndAlarmManager(out rootController, out alarmManager);

            alarmManager.Alarms.Should().NotBeEmpty();
            rootController.Childs.Should().NotBeEmpty();
        }
    }

    /// <summary>
    /// Generates sample controller tree and alarm manager:
    /// 
    ///          FB_MOD
    ///          /    \
    ///      FB_CYL  FB_CYL
    /// 
    /// With two warnings.
    /// </summary>
    public static class ControllerSampleGenerator
    {
        public static void GenerateSampleControllerAndAlarmManager(out Controller root, out AlarmManager alarmManager)
        {
            alarmManager = GenerateSampleAlarmManager();
            root = GenerateSampleControllerTree();

            // wire up alarms
            alarmManager.AlarmsCollection.ToArray()[0].Controller = root.ChildsCollection.ToArray()[0];
            alarmManager.AlarmsCollection.ToArray()[1].Controller = root.ChildsCollection.ToArray()[1];

            root.ChildsCollection.ToArray()[0].ActiveAlarm = alarmManager.AlarmsCollection.ToArray()[0];
            root.ChildsCollection.ToArray()[0].ActiveAlarm = alarmManager.AlarmsCollection.ToArray()[1];
        }

        public static AlarmManager GenerateSampleAlarmManager()
        {
            var alarmManager = new AlarmManager(null);

            alarmManager.AlarmsCollection.Add(new Alarm(1, AlarmType.Stop, "timeout extend - sensor extend not reached", null));
            alarmManager.AlarmsCollection.Add(new Alarm(1, AlarmType.Stop, "interlock extend - movement stopped", null));

            return alarmManager;
        }

        public static Controller GenerateSampleControllerTree()
        {
            var controller = new Controller(null)
            {
                CurrentState = "RUN",
                CurrentSubState = "WAIT",
                IsEnabled = true,
                EnableForcing = false,
                Type = "FB_MOD",
                Name = "MOD_2"
            };

            controller.CommandsCollection.Add(new Command(controller)
                                    {
                                        CommandId = 8,
                                        IsAvailable = true,
                                        Name = "Init"
                                    });

            controller.CommandsCollection.Add(new Command(controller)
            {
                CommandId = 10,
                IsAvailable = true,
                Name = "Run"
            });

            controller.ParametersCollection.Add(new Tag
                                          {
                                              Name = "udiWaitPick_ms",
                                              DataType = "UDINT"
                                          });


            controller.ChildsCollection.Add(CreateCylinder("HOR_2"));
            controller.ChildsCollection.Add(CreateCylinder("VER_2"));

            return controller;
        }

        private static Controller CreateCylinder(string name)
        {
            var controller = new Controller(null)
                                 {
                                     CurrentState = "RUN",
                                     CurrentSubState = "WAIT",
                                     IsEnabled = true,
                                     EnableForcing = false,
                                     Type = "FB_CYL",
                                     Name = name
                                 };

            controller.CommandsCollection.Add(new Command(controller)
                                    {
                                        CommandId = 12,
                                        IsAvailable = true,
                                        Name = "STOP"
                                    });

            controller.CommandsCollection.Add(new Command(controller)
                                    {
                                        CommandId = 202,
                                        IsAvailable = true,
                                        Name = "Retract"
                                    });

            controller.CommandsCollection.Add(new Command(controller)
                                    {
                                        CommandId = 204,
                                        IsAvailable = true,
                                        Name = "Extend"
                                    });

            controller.AlarmsCollection.Add(new Alarm(1, AlarmType.TactStop, "sensor retract always on", controller));
            controller.AlarmsCollection.Add(new Alarm(2, AlarmType.TactStop, "timeout extend - sensor extend not reached", controller));
            controller.AlarmsCollection.Add(new Alarm(3, AlarmType.TactStop, "sensor retract always on", controller));
            controller.AlarmsCollection.Add(new Alarm(4, AlarmType.TactStop, "timeout extend cylinder - sensor retract not reached", controller));
            controller.AlarmsCollection.Add(new Alarm(5, AlarmType.Stop, "interlock extend - movement stopped", controller));
            controller.AlarmsCollection.Add(new Alarm(6, AlarmType.Stop, "interlock retract - movement stopped", controller));

            controller.ParametersCollection.Add(new Tag
                                          {
                                              Name = "udiExtendTime_ms",
                                              DataType = "UDINT"
                                          });

            controller.ParametersCollection.Add(new Tag
                                        {
                                            Name = "udiRetractTime_ms",
                                            DataType = "UDINT"
                                        });

            controller.ParametersCollection.Add(new Tag
                                        {
                                            Name = "udiSimExtendTime_ms",
                                            DataType = "UDINT"
                                        });

            controller.ParametersCollection.Add(new Tag
                                    {
                                        Name = "udiSimRetractTime_ms",
                                        DataType = "UDINT"
                                    });

            return controller;
        }
    }
}
