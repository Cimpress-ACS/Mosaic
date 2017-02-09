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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;
using VP.FF.PT.Common.PlcEssentials.ControllerImporting;

namespace VP.FF.PT.Common.PlcEssentials.Impl
{
    /// <summary>
    /// Provides informations of a PLC state machine controller and methods for manipulate it.
    /// Hierarchical state machines are supported and linked with Parent and Childs references.
    /// </summary>
    /// <remarks>
    /// To manipulate the controller use the Send..() methods.
    /// </remarks>
    public class Controller : IController
    {
        private readonly ITagController _tagController;
        private readonly IControllerTag _controllerTag;
        private readonly ConcurrentBag<Controller> _children;
        private readonly ConcurrentDictionary<uint, Alarm> _alarms;
        private readonly ConcurrentBag<PossibleAlarm> _possibleAlarms;
        private readonly ConcurrentBag<Tag> _parameters;
        private readonly ConcurrentBag<Tag> _configurations;
        private readonly ConcurrentBag<Tag> _actualValues;
        private readonly ConcurrentBag<Tag> _inputs;
        private readonly ConcurrentBag<Tag> _outputs;
        private readonly ConcurrentBag<Tag> _commonInterfaceTags;
        private readonly ConcurrentBag<Command> _commands;

        private readonly Dictionary<string, ConcurrentBag<Tag>> _userDefinedInterfaces = new Dictionary<string, ConcurrentBag<Tag>>();
        private readonly Dictionary<string, IEnumerable<ICommand>> _userDefinedCommands = new Dictionary<string,IEnumerable<ICommand>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Controller"/> class.
        /// </summary>
        public Controller(
            ITagController tagController,
            IControllerTag controllerTag)
        {
            _tagController = tagController;
            _controllerTag = controllerTag;
            _children = new ConcurrentBag<Controller>();
            _alarms = new ConcurrentDictionary<uint, Alarm>();
            _possibleAlarms = new ConcurrentBag<PossibleAlarm>();
            _parameters = new ConcurrentBag<Tag>();
            _configurations = new ConcurrentBag<Tag>();
            _actualValues = new ConcurrentBag<Tag>();
            _inputs = new ConcurrentBag<Tag>();
            _outputs = new ConcurrentBag<Tag>();
            _commonInterfaceTags = new ConcurrentBag<Tag>();
            _commands = new ConcurrentBag<Command>();
            AlarmAdded += () => { };
            AlarmRemoved += () => { };
            AlarmReplaced += () => { };
            CommonInformationChanged += () => { };
            GetCommonInterfaceTag().ValueChanged += (s, e) => CommonInformationChanged();
        }

        /// <summary>
        /// The <see cref="IController.AlarmAdded"/> event is raised, when ever the plc adds an alarm for this instance.
        /// </summary>
        public event Action AlarmAdded;

        /// <summary>
        /// The <see cref="IController.AlarmRemoved"/> event is raised, when ever the plc removes an alarm for this instance.
        /// </summary>
        public event Action AlarmRemoved;

        /// <summary>
        /// The <see cref="IController.AlarmReplaced"/> event is raised, when ever the plc replaces an alarm for this instance.
        /// </summary>
        public event Action AlarmReplaced;

        /// <summary>
        /// Notifies subscribers when ever the common information of this instance changes.
        /// </summary>
        public event Action CommonInformationChanged;

        /// <summary>
        /// Gets the unique controller id.
        /// </summary>
        public int Id
        {
            get { return GetCommonInterfaceValue().Info.CtrlId; }
        }

        /// <summary>
        /// Gets the specific instance name of this controller.
        /// </summary>
        public string Name
        {
            get { return GetCommonInterfaceValue().Info.Name; }
        }

        /// <summary>
        /// Gets the type (class) name which is unique.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type
        {
            get { return GetCommonInterfaceValue().Info.Type; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Controller"/> is enable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get { return GetCommonInterfaceValue().SoftwareConfig.Enable; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is in simulation mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in simulation mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsSimulation
        {
            get { return GetCommonInterfaceValue().SoftwareConfig.Simulation; }
        }

        /// <summary>
        /// Gets a value indicating whether state machine is fully initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [init done]; otherwise, <c>false</c>.
        /// </value>
        public bool IsInitialized
        {
            get { return GetCommonInterfaceValue().Info.InitDone; }
        }

        /// <summary>
        /// Gets the mode.
        /// When mode is Auto the state machine will accept commands from outside (e.g. from this class using SendCommand method).
        /// When mode is Manual the state machine will only work with internal commands.
        /// </summary>
        public ControllerMode ControllerMode
        {
            get { return (ControllerMode) GetCommonInterfaceValue().Job.Mode; }
        }

        /// <summary>
        /// Gets the current state of the state machine.
        /// </summary>
        public string CurrentState
        {
            get { return GetCommonInterfaceValue().Job.StateName; }
        }

        /// <summary>
        /// Gets the current state of teh sub state machine.
        /// </summary>
        public string CurrentSubState
        {
            get { return GetCommonInterfaceValue().Job.SubStateName; }
        }

        /// <summary>
        /// Gets a value indicating whether IO forcing is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if forcing is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableForcing
        {
            get { return GetCommonInterfaceValue().Job.EnableForcing; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance suspends all interlocks for debugging and testing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suspends all interlocks; otherwise, <c>false</c>.
        /// </value>
        public bool IsInterlockOverwrite
        {
            get { return GetCommonInterfaceValue().Job.OverwriteInterlocks; }
        }

        /// <summary>
        /// Gets the id of the parent controller.
        /// </summary>
        public int ParentId
        {
            get { return GetCommonInterfaceValue().Info.CtrlIdOfParent; }
        }

        /// <summary>
        /// Gets the path to the PLC controller path
        /// </summary>
        /// <value>
        /// The Path to the controller (ex. Normal.fbRot)
        /// </value>
        public string PlcInternalTreePath
        {
            get { return _controllerTag.GetPath(); }
        }

        /// <summary>
        /// Gets the path to the PLC controller path
        /// </summary>
        /// <value>
        /// The Path to the controller (ex. Normal.fbRot)
        /// </value>
        public string PlcControllerPath
        {
            get { return _controllerTag.GetPath(); }
        }

        /// <summary>
        /// Gets a readable name for the controller out of the CIf structure
        /// </summary>
        /// <value>
        /// FullName out of the CIf.Info structure
        /// </value>
        public string FullName
        {
            get { return GetCommonInterfaceValue().Info.FullName; }    
        }

        /// <summary>
        /// Gets the scope of the controller
        /// </summary>
        public string Scope
        {
            get { return _controllerTag.Tag.Scope; }
        }

        /// <summary>
        /// Gets the scoped plc controller path consisting of the <see cref="Scope"/> and the <see cref="PlcControllerPath"/>.
        /// </summary>
        public string ScopedControllerPath
        {
            get { return string.Format("{0}.{1}", Scope, PlcControllerPath); }
        }

        /// <summary>
        /// Gets the common interface path.
        /// </summary>
        public string PlcCIfPath
        {
            get { return _controllerTag.GetPath(); }
        }

        /// <summary>
        /// Gets a list childs controllers.
        /// </summary>
        /// <value>
        /// The childs.
        /// </value>
        public IEnumerable<IController> Childs
        {
            get { return ChildsCollection.OrderBy(c => c.Name).ToList(); }
        }

        /// <summary>
        /// Gets the childs collection, for internal use only.
        /// </summary>
        /// <value>
        /// The childs collection.
        /// </value>
        public ConcurrentBag<Controller> ChildsCollection
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets a list of all available commands of this PLC controller.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        public virtual IEnumerable<ICommand> Commands
        {
            get { return _commands; }
        }

        /// <summary>
        /// Gets or sets the active command. If no command is executing at the moment null will be returned.
        /// </summary>
        /// <value>
        /// The active command.
        /// </value>
        public ICommand ActiveCommand { get; set; }

        public IEnumerable<PossibleAlarm> PossibleAlarms
        {
            get { return _possibleAlarms; }
        }

        /// <summary>
        /// Gets the list of possible possibleAlarms.
        /// </summary>
        /// <value>
        /// The possibleAlarms.
        /// </value>
        public IEnumerable<IAlarm> Alarms
        {
            get { return _alarms.Values; }
        }

        /// <summary>
        /// Gets the active alarm. If no alarm is active null will be returned.
        /// </summary>
        /// <value>
        /// The active alarm.
        /// </value>
        public IAlarm ActiveAlarm 
        {
            get
            {
                var alarm = Alarms.FirstOrDefault(x => x.AlarmType == AlarmType.EmergencyOff);
                if (alarm == null)
                    alarm = Alarms.FirstOrDefault(x => x.AlarmType == AlarmType.Off);
                if (alarm == null)
                    alarm = Alarms.FirstOrDefault(x => x.AlarmType == AlarmType.Stop);
                if (alarm == null)
                    alarm = Alarms.FirstOrDefault(x => x.AlarmType == AlarmType.TactStop);
                if (alarm == null)
                    alarm = Alarms.FirstOrDefault(x => x.AlarmType == AlarmType.Warning);
                return alarm;
            } 
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public IEnumerable<Tag> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        public IEnumerable<Tag> Configurations
        {
            get { return _configurations; }
        }

        /// <summary>
        /// Gets the actual values.
        /// </summary>
        public IEnumerable<Tag> ActualValues
        {
            get { return _actualValues; }
        }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        public IEnumerable<Tag> Inputs
        {
            get { return _inputs; }
        }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        public IEnumerable<Tag> Outputs
        {
            get { return _outputs; }
        }

        public IEnumerable<Tag> CommonInterface
        {
            get { return _commonInterfaceTags; }
        }

        public Dictionary<string, ConcurrentBag<Tag>> UserDefinedInterfaces
        {
            get { return _userDefinedInterfaces; }
        }

        public Dictionary<string, IEnumerable<ICommand>> UserDefinedCommands
        {
            get { return _userDefinedCommands; }
        }

        private CtrlCommonInterface GetCommonInterfaceValue()
        {
            Tag commonInterfaceTag = GetCommonInterfaceTag();
            if (commonInterfaceTag != null)
            {
                var innerCommonInterface = commonInterfaceTag.Value as CtrlCommonInterface;
                if (innerCommonInterface != null)
                    return innerCommonInterface;
            }
            return new CtrlCommonInterface
            {
                Info = new CtrlInfo(),
                Job = new CtrlJob(),
                SoftwareConfig = new CtrlSoftwareConfig()
            };
        }

        private Tag GetCommonInterfaceTag()
        {
            return _controllerTag.GetCommonInterfaceTag();
        }

        /// <summary>
        /// Tries to et the command with the specified <paramref name="commandId"/>.
        /// </summary>
        /// <param name="commandId">The id of the desired command.</param>
        /// <returns>The desired command or null.</returns>
        public Command TryGetCommandById(int commandId)
        {
            return _commands.FirstOrDefault(c => c.CommandId == commandId);
        }

        /// <summary>
        /// Replaces all commands of this controller.
        /// </summary>
        /// <param name="updatedCommands">The updated commands.</param>
        public virtual void UpdateCommands(IEnumerable<Command> updatedCommands)
        {
            _commands.TryRemoveAll();
            foreach (Command command in updatedCommands)
                _commands.Add(command);
        }

        /// <summary>
        /// Replaces all commands identified by the <see cref="key"/> of this controller
        /// </summary>
        /// <param name="key">The key to identify the commands</param>
        /// <param name="updatedCommands">The updated commands</param>
        public virtual void UpdateUserDefinedCommands(string key, IList<Command> updatedCommands)
        {
            if (updatedCommands.Count > 0)
            {
                if (_userDefinedCommands.ContainsKey(key))
                {
                    _userDefinedCommands.Remove(key);
                }

                _userDefinedCommands.Add(key, updatedCommands); 
            }
        }

        /// <summary>
        /// Adds the specified <paramref name="controller"/> as a child of this controller.
        /// </summary>
        /// <param name="controller">The controller to add.</param>
        public void AddChild(Controller controller)
        {
            _children.Add(controller);
        }

        /// <summary>
        /// Tries to remove the alarm with the <paramref name="alarmId"/> from the alarm list
        /// </summary>
        /// <param name="alarmId">The alarm id</param>
        public bool TryRemoveAlarm(uint alarmId, out Alarm alarm)
        {
            bool result = _alarms.TryRemove(alarmId, out alarm);

            if (result)
                AlarmRemoved();

            return result;
        }

        /// <summary>
        /// Adds the specified <paramref name="alarm"/> to this controller.
        /// </summary>
        /// <param name="alarm">An <see cref="Alarm"/> instance.</param>
        public bool TryAddAlarm(Alarm alarm)
        {
            bool result = _alarms.TryAdd(alarm.Id, alarm);

            if (result)
                AlarmAdded();

            return result;
        }

        /// <summary>
        /// Replaces the specified <paramref name="alarm"/> within this controller.
        /// </summary>
        /// <param name="alarm">An <see cref="Alarm"/> instance.</param>
        public bool TryReplaceAlarm(Alarm alarm)
        {
            bool result = false;

            if (_alarms.ContainsKey(alarm.Id) && alarm != _alarms[alarm.Id])
            {
                _alarms[alarm.Id] = alarm;
                AlarmReplaced();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Resets the alarm manager of the module this controller is in.
        /// </summary>
        public void ResetModuleAlarms()
        {
            var resetAlarmsTag = new Tag(
                TagName.AlarmManager(parent: null).AlarmCommand(),
                TagName.Global(),
                "INT");
            _tagController.WriteTag(resetAlarmsTag, (short)StandardAlarmCommands.ResetAll);
        }

        /// <summary>
        /// Adds the specified <paramref name="parameters"/> tags to this controller.
        /// </summary>
        /// <param name="parameters">An <see cref="IEnumerable{T}"/> of Tag instances.</param>
        public void AddParameters(IEnumerable<Tag> parameters)
        {
            AddTagsToTagCollection(parameters, _parameters);
        }

        /// <summary>
        /// Adds the specified <paramref name="configurations"/> tags to this controller.
        /// </summary>
        /// <param name="configurations">An <see cref="IEnumerable{T}"/> of Tag instances.</param>
        public void AddConfigurations(IEnumerable<Tag> configurations)
        {
            AddTagsToTagCollection(configurations, _configurations);
        }

        /// <summary>
        /// Adds the specified <paramref name="actualValues"/> tags to this controller.
        /// </summary>
        /// <param name="actualValues">An <see cref="IEnumerable{T}"/> of Tag instances.</param>
        public void AddActualValues(IEnumerable<Tag> actualValues)
        {
            AddTagsToTagCollection(actualValues, _actualValues);
        }

        /// <summary>
        /// Adds the specified <paramref name="inputs"/> tags to this controller.
        /// </summary>
        /// <param name="inputs">An <see cref="IEnumerable{T}"/> of Tag instances.</param>
        public void AddInputs(IEnumerable<Tag> inputs)
        {
            AddTagsToTagCollection(inputs, _inputs);
        }

        /// <summary>
        /// Adds the specified <paramref name="outputs"/> tags to this controller.
        /// </summary>
        /// <param name="outputs">An <see cref="IEnumerable{T}"/> of Tag instances.</param>
        public void AddOutputs(IEnumerable<Tag> outputs)
        {
            AddTagsToTagCollection(outputs, _outputs);
        }

        /// <summary>
        /// Adds the specified <paramref name="commonInterfaceTags"/> tags to this controller.
        /// </summary>
        /// <param name="commonInterfaceTags">An <see cref="IEnumerable{T}"/> of Tag instances.</param>
        public void AddCommonInterfaceTags(IEnumerable<Tag> commonInterfaceTags)
        {
            AddTagsToTagCollection(commonInterfaceTags, _commonInterfaceTags);
        }

        public void AddPossibleAlarms(IEnumerable<PossibleAlarm> possibleAlarms)
        {
            foreach (var possibleAlarm in possibleAlarms)
            {
                _possibleAlarms.Add(possibleAlarm);
            }
        }

        /// <summary>
        /// Adds tags of a user defined interface to this controller.
        /// </summary>
        /// <param name="key">The name of the user defined interface</param>
        /// <param name="userDefinedInterfaceTags">An <see cref="IEnumerable{T}"/> of Tag instances</param>
        public void AddUserDefinedInterface(string key, IEnumerable<Tag> userDefinedInterfaceTags)
        {
            var tagCollection = new ConcurrentBag<Tag>();

            _userDefinedInterfaces.Add(key, tagCollection);
            AddTagsToTagCollection(userDefinedInterfaceTags, tagCollection);
        }

        /// <summary>
        /// Gets the tags providing overall information about the controllers state.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Tag"/> instances.</returns>
        public IEnumerable<Tag> GetOverallInformationTags()
        {
            return new[] {GetCommonInterfaceTag()};
        }

        /// <summary>
        /// Gets all the tags associated with this controller recursively.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Tag"/>.</returns>
        public IEnumerable<Tag> GetAllAssociatedTags()
        {
            foreach (Tag tag in GetConcatedTags())
                foreach (Tag childTag in tag.GetThisWithDescendantsFlat())
                    yield return childTag;
        }

        private IEnumerable<Tag> GetConcatedTags()
        {
            return new Tag[0]
                .Concat(_parameters)
                .Concat(_configurations)
                .Concat(_actualValues)
                .Concat(_inputs)
                .Concat(_outputs);
        }

        /// <summary>
        /// Sends a new mode to PLC.
        /// </summary>
        /// <param name="newControllerMode">The new mode.</param>
        public void SendMode(ControllerMode newControllerMode)
        {
            //ControllerMode = newControllerMode;
            //            var modeTag = new Tag(PlcCIfPath + "." + NamingConventions.CommonInterfaceControllerMode, string.Empty, "INT");
            var modeTag = new Tag(PlcControllerPath + "." + NamingConventions.CommonInterface + "." + NamingConventions.CommonInterfaceControllerMode, Scope, "INT");
            _tagController.WriteTag(modeTag, (int)newControllerMode);
        }

        /// <summary>
        /// Sends a new mode to PLC.
        /// </summary>
        /// <param name="newControllerMode">The new mode.</param>
        private void SendMode(ControllerMode newControllerMode, Tag modeTag)
        {
            _tagController.WriteTag(modeTag, (int)newControllerMode);
        }

        protected void SetAllController2AutoMode()
        {
            SendMode(ControllerMode.Auto);

            foreach (var childController in ChildsCollection)
                childController.SetAllController2AutoMode();
        }

        /// <summary>
        /// Sends the specified <paramref name="isSimulated"/> state to the plc.
        /// </summary>
        /// <param name="isSimulated">The new is simulated state.</param>
        /// <param name="propagate">True = Set all subcontrollers also to Simulation / False = Clear the simulation for all sub controllers</param>
        public void SendIsSimulated(bool isSimulated, bool propagate)
        {
            string tagname;

            if (propagate)
            {
                tagname = string.Format("{0}.{1}.{2}", PlcControllerPath, NamingConventions.CommonInterface,
                    NamingConventions.CommonInterfaceSimulationAll);
            }
            else
            {
                tagname = string.Format("{0}.{1}.{2}", PlcControllerPath, NamingConventions.CommonInterface,
                    NamingConventions.CommonInterfaceSimulation);
            }

            var simulationTag = new Tag(tagname, Scope, IEC61131_3_DataTypes.Boolean);
            _tagController.WriteTag(simulationTag, isSimulated);
        }

        /// <summary>
        /// Sends a new forcing enable state.
        /// </summary>
        /// <param name="enable"></param>
        public void SendEnableForcing(bool enable)
        {
            if (enable == EnableForcing)
                return;

            //var enableTag = new Tag(PlcCIfPath + "." + NamingConventions.CommonInterfaceEnable, string.Empty, "BOOL");
            var enableForcingTag = new Tag(PlcCIfPath + "." + NamingConventions.CommonInterfaceEnableForcing, string.Empty, "BOOL");
            _tagController.WriteTag(enableForcingTag, enable);

            // TODO: replace this with online update
            //EnableForcing = enable;
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="ControllerException">Can't send command when it is not in list of available commands for this controller.</exception>
        public void SendCommand(ICommand command)
        {
            var userDefinedCommand = UserDefinedCommands.FirstOrDefault(kvp => kvp.Value.Contains(command));

            if (!Commands.Contains(command) && userDefinedCommand.Key == null && userDefinedCommand.Value == null)
                throw new ControllerException("Can't send command " + command.Name + " to controller " + Name + " because it is not available for state machine " + Type, this);

            // 1.) set mode to manual
            if (command.HasCommandAndModeTag)
            {
                SendMode(ControllerMode.Manual, command.ModeTag);
            }
            else
            {
                SendMode(ControllerMode.Manual); 
            }

            // 2.) fire command            
            Tag commandTag;

            if (command.HasCommandAndModeTag)
                commandTag = command.CommandTag;
            else
            {
                // var commandTag = new Tag(PlcCIfPath + "." + NamingConventions.CommonInterfaceManualCommandChannel, string.Empty, "INT");
                commandTag = new Tag(PlcControllerPath + "." + NamingConventions.CommonInterface + "." + NamingConventions.CommonInterfaceManualCommandChannel, Scope, "INT");
            }

            _tagController.WriteTag(commandTag, command.CommandId);
        }

        /// <summary>
        /// Sends a new parameter value.
        /// </summary>
        /// <param name="newParameter">The new parameter.</param>
        /// <exception cref="ControllerException">Can only send parameter which are supported by this controller.</exception>
        public void SendParameter(Tag newParameter)
        {
            Tag tag = FindTagInTagCollection(newParameter.NestedName, Parameters);

            if (tag == null)
                throw new ControllerException("Can't send parameter " + newParameter.NestedName + " to controller " + Name + " because it is not supported by this controller.", this);

            _tagController.WriteTag(tag, newParameter.Value);
        }

        /// <summary>
        /// Sends the parameter value to PLC.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The new value.</param>
        public void SendParameter(string parameterName, object value)
        {
            Tag tag = FindTagInTagCollection(parameterName, Parameters);

            if (tag == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to send parameter (name:'{0}', value:'{1}') to controller '{2}'", parameterName, value, Name).AppendLine()
                    .AppendFormat("Unfortunately, there is no parameter with the provided name.").AppendLine()
                    .AppendFormat("Please try to set only parameters that exist.").ToString());

            _tagController.WriteTag(tag, value);
        }

        /// <summary>
        /// Sends the configuration to PLC.
        /// </summary>
        /// <param name="newConfiguration">The new configuration.</param>
        /// <exception cref="ControllerException">Can only send configuration which are supported by this controller.</exception>
        public void SendConfiguration(Tag newConfiguration)
        {
            Tag tag = FindTagInTagCollection(newConfiguration.NestedName, Configurations);

            if (tag == null)
                throw new ControllerException("Can't send configuration " + newConfiguration.NestedName + " to controller " + Name + " because it is not supported by this controller.", this);

            _tagController.WriteTag(tag, newConfiguration.Value);
        }

        /// <summary>
        /// Sends the configuration value to PLC.
        /// </summary>
        /// <param name="configurationName">Name of the configuration.</param>
        /// <param name="value">The new value.</param>
        public void SendConfiguration(string configurationName, object value)
        {
            Tag tag = FindTagInTagCollection(configurationName, Configurations);

            if (tag == null)
                throw new InvalidOperationException(new StringBuilder().AppendLine()
                    .AppendFormat("Tried to send configuration (name:'{0}', value:'{1}') to controller '{2}'", configurationName, value, Name).AppendLine()
                    .AppendFormat("Unfortunately, there is no configuration with the provided name.").AppendLine()
                    .AppendFormat("Please try to set only configurations that exist.").ToString());

            _tagController.WriteTag(tag, value);
        }

        private void AddTagsToTagCollection(IEnumerable<Tag> tags, ConcurrentBag<Tag> tagCollection)
        {
            foreach (Tag tag in tags)
                tagCollection.Add(tag);
        }

        public Tag FindTag(string tagName, Tag tag)
        {
            return tag.GetThisWithDescendantsFlat().FirstOrDefault(childTag => string.Equals(childTag.NestedName, tagName));
        }

        private Tag FindTagInTagCollection(string tagName, IEnumerable<Tag> inTagCollection)
        {
            return inTagCollection.Select(tag => FindTag(tagName, tag)).FirstOrDefault(foundTag => foundTag != null);
        }

        public override string ToString()
        {
            return Name + ", " + Type;
        }
    }
}
