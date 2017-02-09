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


namespace VP.FF.PT.Common.PlcCommunication
{
    public static class NamingConventions
    {
        public const string Global = "Global";
        public const string PathCIfArray = "g_a_pCtrlCIf";
        public const string PathGlobalCIfArray = Global + "." + PathCIfArray;
        public const string PathGlobalControllerArray = "g_a_pCtrlInst";
        public const string PathAlarmManager = "g_p_fbAlm_Man^";
        public const string PathGlobalControllerCount = "g_iMaxControllerCount";
        public const string PathMachineNumber = PathAlarmManager + "." + "c_udiMachineNr";

        public const string ManualCommand = "intMCmd";
        public const string AutomaticCommand = "intACmd";
        public const string ControllerMode = "eMode";
        public const string StateNumber = "intState";
        public const string StateString = "strState";
        public const string SubStateString = "strSubState";

        public const string CommonInterface = "CIf";
        public const string ModuleInterfaceActualValues = "SIf_MOD.Cur^";
        public const string ModuleInterfaceConfigValues = "SIf_MOD.Cfg^";
        public const string ModuleInterfaceActualValuesType = "T_Ctrl_SIf_MOD_Cur";
        public const string CommonInterfaceManualCommandChannel = "Job.intMCmd";
        public const string CommonInterfaceJob = "Job";
        public const string CommonInterfaceSwConfig = "SwCfg";
        public const string ManualCommandChannel = CommonInterfaceJob + "." + ManualCommand;
        public const string AutomaticCommandChannel = CommonInterfaceJob + "." + AutomaticCommand;
        public const string StateChannel = CommonInterfaceJob + "." + StateNumber;
        public const string CommonInterfaceEnable = CommonInterfaceSwConfig + ".bolEnable";
        public const string CommonInterfaceSimulation = CommonInterfaceSwConfig + ".bolSimulation";
        public const string CommonInterfaceSimulationAll = CommonInterfaceSwConfig + ".bolAllSimulation";
        public const string CommonInterfaceEnableForcing = CommonInterfaceJob + ".bolIoForce";
        public const string CommonInterfaceControllerMode = CommonInterfaceJob + "." + ControllerMode;
        
        public const string CommonInterfaceAutoCmdChannel = CommonInterface + "." + AutomaticCommandChannel;
        public const string CommonInterfaceManualCmdChannel = CommonInterface + "." + ManualCommandChannel;

        public const string CommonInterfaceState = CommonInterface + "." + CommonInterfaceJob + "." + StateNumber;
        public const string CommonInterfaceStateName = CommonInterface + "." + CommonInterfaceJob + "." + StateString;
        public const string CommonInterfaceSubStateName = CommonInterface + "." + CommonInterfaceJob + "." + SubStateString;

        public const string SpecificInterface = "SIf";
        public const string SpecificInterfaceParameter = "Par";
        public const string SpecificInterfaceActualValues = "Cur";
        public const string SpecificInterfaceConfigurartion = "Cfg";
        public const string SpecificInterfacePossibleAlarms = "Alm";
        public const string SpecificInterfaceOutputs = "Out";
        public const string SpecificInterfaceInputs = "In";

        public const string CmdList = "aCmdList";
        public const string IsCmdAvailable = "bolAvailable";

        public const string AlarmManagerAlarmArray = "aAlm";
        public const string AlarmManagerCommandChannel = "intAlmCmd";
        public const string AlarmManagerMaxAlarms = "c_uinMaxAlmElements";

        public static class AlarmManagerCommands
        {
            public const int ResetAll = 2;
            // reset state=RST, reset state CLR
            public const int ResetRst = 4;
            // reset state CLR -> c_uinModeAlarm = 1, automatic reset will be done on PLC
            public const int ResetClr = 6;
        }

        public const string DateTimeString = "yyyy-MM-dd-HH:mm:ss.fff";
    }
}
