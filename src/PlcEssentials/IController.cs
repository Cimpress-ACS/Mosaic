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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcEssentials
{
    /// <summary>
    /// Provides informations of a PLC state machine controller and methods for manipulate it.
    /// Hierarchical state machines are supported and linked with Parent and Childs references.
    /// </summary>
    /// <remarks>
    /// To manipulate the controller use the Send..() methods.
    /// </remarks>
    public interface IController
    {
        /// <summary>
        /// The <see cref="AlarmAdded"/> event is raised, when ever the plc adds an alarm to this instance.
        /// </summary>
        event Action AlarmAdded;

        /// <summary>
        /// The <see cref="IController.AlarmRemoved"/> event is raised, when ever the plc removes an alarm from this instance.
        /// </summary>
        event Action AlarmRemoved;

        /// <summary>
        /// The <see cref="IController.AlarmReplaced"/> event is raised, when ever the plc replaces an alarm within this instance.
        /// </summary>
        event Action AlarmReplaced;

        /// <summary>
        /// Notifies subscribersa when ever the common information of this instance changes.
        /// </summary>
        event Action CommonInformationChanged;

        /// <summary>
        /// Gets the unique controller id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets the specific instance name of this controller.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the type (class) name which is unique.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IController"/> is enable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enable; otherwise, <c>false</c>.
        /// </value>
        bool IsEnabled { get; }


        /// <summary>
        /// Gets a value indicating whether this instance is in simulation mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in simulation mode; otherwise, <c>false</c>.
        /// </value>
        bool IsSimulation { get; }
        
        /// <summary>
        /// Gets a value indicating whether state machine is fully initialized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [init done]; otherwise, <c>false</c>.
        /// </value>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Gets the mode.
        /// When mode is Auto the state machine will accept commands from outside (e.g. from this class using SendCommand method).
        /// When mode is Manual the state machine will only work with internal commands.
        /// </summary>
        ControllerMode ControllerMode { get; }

        /// <summary>
        /// Gets the current state of the state machine.
        /// </summary>
        string CurrentState { get; }

        /// <summary>
        /// Gets the current state of teh sub state machine.
        /// </summary>
        string CurrentSubState { get; }

        /// <summary>
        /// Gets a value indicating whether IO forcing is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if forcing is enabled; otherwise, <c>false</c>.
        /// </value>
        bool EnableForcing { get; }

        /// <summary>
        /// Gets a value indicating whether this instance suspends all interlocks for debugging and testing.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is suspends all interlocks; otherwise, <c>false</c>.
        /// </value>
        bool IsInterlockOverwrite { get; }

        /// <summary>
        /// Gets the path to the PLC controller path
        /// </summary>
        /// <value>
        /// The Path to the controller (ex. Normal.fbRot)
        /// </value>
        string PlcInternalTreePath { get; }

        /// <summary>
        /// Gets the path to the PLC controller path
        /// </summary>
        /// <value>
        /// The Path to the controller (e.g. MiddlePRG.fbLDS_1)
        /// </value>
        string PlcControllerPath { get; }

        /// <summary>
        /// Gets a readable name for the controller out of the CIf structure
        /// </summary>
        /// <value>
        /// FullName out of the CIf.Info structure
        /// </value>
        string FullName { get; }

            /// <summary>
        /// Gets the scope of the controller
        /// </summary>
        string Scope { get; }

        /// <summary>
        /// Gets a list childs controllers.
        /// </summary>
        /// <value>
        /// The childs.
        /// </value>
        IEnumerable<IController> Childs { get; }        

        /// <summary>
        /// Gets a list of all available commands of this PLC controller.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        IEnumerable<ICommand> Commands { get; }

        /// <summary>
        /// Gets or sets the active command. If no command is executing at the moment null will be returned.
        /// </summary>
        /// <value>
        /// The active command.
        /// </value>
        ICommand ActiveCommand { get; }

        /// <summary>
        /// Gets the list of possible alarms a controller can have.
        /// </summary>
        /// <value>
        /// A list of possible alarms.
        /// </value>
        IEnumerable<PossibleAlarm> PossibleAlarms { get; }

        /// <summary>
        /// Gets the list of present alarms.
        /// </summary>
        /// <value>
        /// The alarms.
        /// </value>
        IEnumerable<IAlarm> Alarms { get; }

        /// <summary>
        /// Gets the most important active alarm. If no alarm is active null will be returned.
        /// </summary>
        /// <value>
        /// The active alarm.
        /// </value>
        IAlarm ActiveAlarm { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        IEnumerable<Tag> Parameters { get; }

        /// <summary>
        /// Gets the configurations.
        /// </summary>
        IEnumerable<Tag> Configurations { get; }

        /// <summary>
        /// Gets the actual values.
        /// </summary>
        IEnumerable<Tag> ActualValues { get; }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        IEnumerable<Tag> Inputs { get; }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        IEnumerable<Tag> Outputs { get; }

        /// <summary>
        /// Get the common interface
        /// </summary>
        IEnumerable<Tag> CommonInterface { get; }

        /// <summary>
        /// Get all the user defined interfaces
        /// </summary>
        Dictionary<string, ConcurrentBag<Tag>> UserDefinedInterfaces { get; }

        /// <summary>
        /// Get all the user defined commands
        /// </summary>
        Dictionary<string, IEnumerable<ICommand>> UserDefinedCommands { get; }
        
        /// <summary>
        /// Gets the scoped plc controller path consisting of the <see cref="Scope"/> and the <see cref="PlcControllerPath"/>.
        /// </summary>
        string ScopedControllerPath { get; }

        /// <summary>
        /// Gets all the tags associated with this controller recursively.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Tag"/>.</returns>
        IEnumerable<Tag> GetAllAssociatedTags();

        /// <summary>
        /// Resets the alarm manager of the module this controller is in.
        /// </summary>
        void ResetModuleAlarms();

        /// <summary>
        /// Sends a new mode to PLC.
        /// </summary>
        /// <param name="newControllerMode">The new mode.</param>
        void SendMode(ControllerMode newControllerMode);

        /// <summary>
        /// Sends the specified <paramref name="isSimulated"/> state to the plc.
        /// </summary>
        /// <param name="isSimulated">The new is simulated state.</param>
        /// <param name="propagate">True = Set all subcontrollers also to Simulation / False = Clear the simulation for all sub controllers</param>
        void SendIsSimulated(bool isSimulated, bool propagate);

        /// <summary>
        /// Sends a new forcing enable state to PLC.
        /// </summary>
        /// <param name="enable"></param>
        void SendEnableForcing(bool enable);

        /// <summary>
        /// Sends the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="ControllerException">Can't send command when it is not in list of available commands for this controller.</exception>
        void SendCommand(ICommand command);

        /// <summary>
        /// Sends a new parameter value to PLC.
        /// </summary>
        /// <param name="newParameter">The new parameter.</param>
        /// <exception cref="ControllerException">Can only send parameter which are supported by this controller.</exception>
        void SendParameter(Tag newParameter);

        /// <summary>
        /// Sends the parameter value to PLC.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">The new value.</param>
        void SendParameter(string parameterName, object value);

        /// <summary>
        /// Sends the configuration to PLC.
        /// </summary>
        /// <param name="newConfiguration">The new configuration.</param>
        /// <exception cref="ControllerException">Can only send configuration which are supported by this controller.</exception>
        void SendConfiguration(Tag newConfiguration);

        /// <summary>
        /// Sends the configuration value to PLC.
        /// </summary>
        /// <param name="configurationName">Name of the configuration.</param>
        /// <param name="value">The new value.</param>
        void SendConfiguration(string configurationName, object value);
        
        /// <summary>
        /// Tries to remove the alarm with the <paramref name="alarmId"/> from the alarm list
        /// </summary>
        /// <param name="alarmId">The alarm id</param>
        bool TryRemoveAlarm(uint alarmId, out Alarm alarm);
        
        /// <summary>
        /// Adds the specified <paramref name="alarm"/> to this controller.
        /// </summary>
        /// <param name="alarm">An <see cref="Alarm"/> instance.</param>
        bool TryAddAlarm(Alarm alarm);

        /// <summary>
        /// Replaces the specified <paramref name="alarm"/> within this controller.
        /// </summary>
        /// <param name="alarm">An <see cref="Alarm"/> instance.</param>
        bool TryReplaceAlarm(Alarm alarm);
    }
}
