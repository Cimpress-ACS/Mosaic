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
	    ///ctrl commands definitions
	    ///general commands 2-200 - every command has to be a even number
	    cCMD_DONE: INT := -1;
	    cCMD_INITIALIZED: INT := 0;
	    cCMD_PON: INT := 2;
	    cCMD_EOFF: INT := 4;
	    cCMD_OFF: INT := 6;
	    cCMD_INIT: INT := 8;
	    cCMD_RUN: INT := 10;
	    cCMD_STOP: INT := 12;
	    cCMD_TSTOP: INT := 14;
	    cCMD_STANDBY: INT := 16;
    END_VAR
     * */
    public enum StandardCommands : short
    {
        Done = -1,
        Initialized = 0,
        PowerOn = 2,
        EmergencyOff = 4,
        Off = 6,
        Init = 8,
        Run = 10,
        Stop = 12,
        TactStop = 14,
        Standby = 16
    }
}
