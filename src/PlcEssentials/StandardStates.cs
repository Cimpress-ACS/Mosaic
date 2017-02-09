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


namespace VP.FF.PT.Common.PlcEssentials
{
    /*
    VAR_GLOBAL CONSTANT
	    ///ctrl states definitions
	    cSTA_DONE: INT := cCMD_DONE;
	    cSTA_INITIALIZED: INT := cCMD_INITIALIZED;
	    cSTA_PONBusy: INT := 1;
	    cSTA_PON: INT := cCMD_PON;
	    cSTA_EOFFBusy: INT := 3;
	    cSTA_EOFF: INT := cCMD_EOFF;
	    cSTA_OFFBusy: INT := 5;
	    cSTA_OFF: INT := cCMD_OFF;
	    cSTA_INITBusy: INT := 7;
	    cSTA_INIT: INT := cCMD_INIT;
	    cSTA_RUNBusy: INT := 9;
	    cSTA_RUN: INT := cCMD_RUN;
	    cSTA_STOPBusy: INT := 11;
	    cSTA_STOP: INT := cCMD_STOP;
	    cSTA_TSTOPBusy: INT := 13;
	    cSTA_TSTOP: INT := cCMD_TSTOP;
	    cSTA_STANDBYBusy: INT := 15;
	    cSTA_STANDBY: INT := cCMD_STANDBY;
	    ///special states without corresponding cmd
	    cSTA_ERROR: INT := 150;
	    cSTA_ERRORQuit: INT := 151;
	    cSTA_SINGLE_ANSWER: INT := 152;
    END_VAR
     * */
    public enum StandardStates : short
    {
        Unknown = -999,
        Done = StandardCommands.Done,
        Initialized = StandardCommands.Initialized,
        PowerOnBusy = 1,
        PowerOn = StandardCommands.PowerOn,
        EmergencyOffBusy = 3,
        EmergencyOff = StandardCommands.EmergencyOff,
        OffBusy = 5,
        Off = StandardCommands.Off,
        InitBusy = 7,
        Init = StandardCommands.Init,
        RunBusy = 9,
        Run = StandardCommands.Run,
        StopBusy = 11,
        Stop = StandardCommands.Stop,
        TactStopBusy = 13,
        TactStop = StandardCommands.TactStop,
        StandbyBusy = 15,
        Standby = StandardCommands.Standby,
        Error = 150,
        ErrorQuit = 151,
        SingleAnswer = 152
    }
}
