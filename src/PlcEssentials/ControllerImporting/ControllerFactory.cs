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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcEssentials.ControllerImporting
{
    /// <summary>
    /// The <see cref="ControllerFactory"/> is capable of creating
    /// <see cref="Controller"/> instances.
    /// </summary>
    [Export(typeof(ICreateController))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ControllerFactory : ICreateController
    {
        private const short NoControllerId = 0;

        /// <summary>
        /// Creates a new <see cref="Controller"/> instance.
        /// </summary>
        /// <param name="tagController">The tag controller used by the created controller.</param>
        /// <param name="controllerTag">The tag, the new controller is dependend on.</param>
        /// <param name="tagListener">A tag listener instance.</param>
        /// <returns>A new <see cref="Controller"/> instance.</returns>
        public Controller Create(ITagController tagController, IControllerTag controllerTag, ITagListener tagListener)
        {
            Controller controller = CreateControllerInstance(tagController, controllerTag);
            IEnumerable<Impl.Command> commands = CreateCommands(controllerTag, controller);
            controller.UpdateCommands(commands);
            controller.AddCommonInterfaceTags(CommonInterfaceTagsFrom(controllerTag));
            controller.AddParameters(ChildTagsFromPath(controllerTag, TagName.SpecificInterface(parent: null).Parameters()));
            controller.AddActualValues(ChildTagsFromPath(controllerTag, TagName.SpecificInterface(parent: null).ActualValues()));
            controller.AddConfigurations(ChildTagsFromPath(controllerTag, TagName.SpecificInterface(parent: null).Configurations()));
            controller.AddOutputs(ChildTagsFromPath(controllerTag, TagName.SpecificInterface(parent: null).Outputs()));
            controller.AddInputs(ChildTagsFromPath(controllerTag, TagName.SpecificInterface(parent: null).Inputs()));
            controller.AddPossibleAlarms(ReadPossibleAlarms(controllerTag, tagListener));

            return controller;
        }

        /// <summary>
        /// Creates a new <see cref="Controller"/> instance.
        /// </summary>
        /// <param name="tagController">The tag controller used by the created controller.</param>
        /// <param name="controllerTag">The tag, the new controller is dependend on.</param>
        /// <param name="userDefinedInterfaces">User defined interfaces</param>
        /// <param name="tagListener">A tag listener instance</param>
        /// <returns>A new <see cref="Controller"/> instance.</returns>
        public Controller Create(
            ITagController tagController, 
            IControllerTag controllerTag, 
            IList<string> userDefinedInterfaces,
            ITagListener tagListener)
        {
            var controller = Create(tagController, controllerTag, tagListener);

            if (userDefinedInterfaces != null)
            {
                tagListener.AddUdtHandler<Command>(CmdList.PlcArrayDataType());

                foreach (var userDefinedInterface in userDefinedInterfaces.Where(value => !string.IsNullOrWhiteSpace(value)))
                {
                    var path = TagName.SpecificInterface(parent: null).AppendTagName(userDefinedInterface);

                    var userDefinedInterfaceTags = ChildTagsFromPath(controllerTag, path);
                    controller.AddUserDefinedInterface(path, userDefinedInterfaceTags);
                    
                    // create the commands
                    foreach (var userDefinedInterfaceTag in userDefinedInterfaceTags)
                    {
                        var commands = CreateCommands(controllerTag, controller, userDefinedInterfaceTag, tagListener);
                        controller.UpdateUserDefinedCommands(userDefinedInterfaceTag.NestedName, commands.ToList());
                    }
                } 
            }

            return controller;
        }

        private Controller CreateControllerInstance(ITagController tagController, IControllerTag controllerTag)
        {
            if (controllerTag.GetParentControllerId() == NoControllerId)
                return new RootController(tagController, controllerTag);
            return new Controller(tagController, controllerTag);
        }

        private IEnumerable<Impl.Command> CreateCommands(
            IControllerTag controllerTag, 
            Controller controller, 
            Tag userDefinedInterfaceTag,
            ITagListener tagListener)
        {
            var commandsTag = controller.FindTag(TagName.CmdList(parent: null), userDefinedInterfaceTag);

            if (commandsTag != null)
            {
                Tag modeTag = null;
                Tag commandTag = null;

                // assuming that the job interface is defined in the parent tag of the commandsTag
                var jobTag = commandsTag.Parent;

                // get the mode tag
                modeTag = controller.FindTag(TagName.Mode(parent: null), jobTag);
                commandTag = controller.FindTag(TagName.ManualCommand(parent: null), jobTag);

                if (jobTag.NestedName != TagName.Job(parent: null) ||
                    modeTag == null || commandTag == null)
                    throw new InvalidOperationException(new StringBuilder()
                        .AppendFormat("The {0} is not defined inside a {1} structure for the controller {2} or ",
                                      TagName.CmdList(parent: null),
                                      TagName.Job(parent: null),
                                      controller.Name)
                        .AppendFormat("the {0} or {1} was not defined in the {2} structure. ",
                                      TagName.Mode(parent: null),
                                      TagName.ManualCommand(parent: null),
                                      TagName.Job(parent: null))
                        .AppendLine()
                        .AppendFormat("Set the {0} as a subelement of a valid {1} structure.",
                                      commandsTag.Name, TagName.Job(parent: null)).ToString());
                
                tagListener.ReadTagSynchronously(commandsTag);
                var commands = commandsTag.ArrayValues<Command>();
                int commandIndex = 0;

                if (commands.Any(c => c == null))
                    throw new InvalidOperationException(new StringBuilder()
                        .AppendFormat("The commands {0} of the controller {1} could not be read. ", 
                                      commandsTag.Name,  
                                      controller.Name)
                        .AppendLine()
                        .AppendFormat("Expected array datatype {0}.", CmdList.PlcArrayDataType())
                        .AppendLine()
                        .AppendFormat("Current array datatype {0}.", commandsTag.DataType).ToString());

                foreach (Command command in commands.Where(c => c != null && !c.Name.IsNullOrEmpty()))
                {
                    Tag tag = controller.FindTag(TagName.CmdList(null, commandIndex), commandsTag);
                    if (tag == null)
                        continue;
                    var commandImpl = new Impl.Command(controller, command, tag.Childs.ToReadOnly(), modeTag, commandTag);
                    yield return commandImpl;
                    commandIndex++;
                }

                // remove CmdListArray-Tag in order to prevent that the same tags exists more 
                // than once --> For example the tag "bolAvailable"
                jobTag.Childs.Remove(commandsTag);
            }
        }

        private IEnumerable<Impl.Command> CreateCommands(IControllerTag controllerTag, Controller controller)
        {
            int commandIndex = 0;
            foreach (Command command in controllerTag.GetCommands())
            {
                string tagPath = TagName.CommonInterface(parent: null).Job().CmdList().CmdList(commandIndex);
                Tag tag = controllerTag.Tag.GetChildTagUnderPath(tagPath);
                if (tag == null)
                    continue;
                var commandImpl = new Impl.Command(controller, command, tag.Childs.ToReadOnly());
                yield return commandImpl;
                commandIndex++;
            }
        }

        private IEnumerable<PossibleAlarm> ReadPossibleAlarms(IControllerTag controllerTag, ITagListener tagListener)
        {
            // TODO: we assume that the order is right here (ToArray) but the interface should be changed from IEnumerable to IList
            Tag[] alarmTags = ChildTagsFromPath(controllerTag, TagName.SpecificInterface(parent: null).Alarms()).ToArray();

            var alarms = new Collection<PossibleAlarm>();
            for (int i = 0; i < alarmTags.Count(); i++)
            {
                var alarmTag = alarmTags[i];

                var alarm = new PossibleAlarm();

                var almText = alarmTag.Childs.First(c => c.NestedName == "strText");
                tagListener.ReadTagSynchronously(almText);
                alarm.Text = (string)almText.Value;

                if (string.IsNullOrEmpty(alarm.Text))
                    continue;

                var almType = alarmTag.Childs.First(c => c.NestedName == "eAlmClass");
                tagListener.ReadTagSynchronously(almType);
                alarm.AlarmType = (short)almType.Value;

                alarm.FullControllerPath = controllerTag.GetScopedPath();

                Tag machineNumberTag = new Tag(NamingConventions.PathMachineNumber, NamingConventions.Global, "UDINT");
                tagListener.ReadTagSynchronously(machineNumberTag);

                // Alarm-ID calculation, see FB_Alm_Man.setAlarm
                // InAlmElement.udiNr := 100000*InAlmElement.udiMachineNr + INT_TO_UDINT(100*InAlmElement.intCtrlId) + InAlmElement.udiAlarmNr;
                var alarmId = 100000*(uint) machineNumberTag.Value + 100*controllerTag.GetControllerId() + i;
                alarm.AlarmId = (uint)alarmId;

                alarms.Add(alarm);
            }

            return alarms;
        }

        private IEnumerable<Tag> CommonInterfaceTagsFrom(IControllerTag controllerTag)
        {
            string jobTagName = TagName.CommonInterface(parent:null).Job();
            Tag jobTag = controllerTag.Tag.GetChildTagUnderPath(jobTagName);
            if (jobTag == null)
                yield break;
            foreach (Tag tag in jobTag.Childs.Where(t => !string.Equals(t.NestedName, NamingConventions.CmdList)))
                yield return tag;
        }

        private IEnumerable<Tag> ChildTagsFromPath(IControllerTag controllerTag, string path)
        {
            Tag tagUnderPath = controllerTag.Tag.GetChildTagUnderPath(path);
            if (tagUnderPath == null)
                yield break;
            foreach (Tag child in tagUnderPath.Childs)
                yield return child;
        }
    }
}
