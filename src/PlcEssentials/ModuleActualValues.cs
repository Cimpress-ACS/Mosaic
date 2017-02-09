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


using System.Runtime.InteropServices;

namespace VP.FF.PT.Common.PlcEssentials
{
    /*
    TYPE T_Ctrl_SIf_MOD_Cur :
    STRUCT
	    // Max module capacity calculated from all childs which implement interface IBuffer
	    dinMaxCapacity: DINT;
	    // Actual number of hangers calculated from all childs which implement interface IBuffer
	    dinNumOfHangers: DINT;
	    // Togglebit to Line Control
	    bolKeepAliveHost: BOOL;
	    bolKeepAlivePlc: BOOL;
	    // LineControl-Signal: Input Buffer is Full
	    a_bBufferIsFull: ARRAY[0..10] OF BOOL;
	
	    // LineControl-Signal: If true Module is not allowed to release hangers (but can be still in RUN state). 
	    bolStopReleaseHangers: BOOL;
    END_STRUCT
    END_TYPE
    */
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public class ModuleActualValues
    {
        public int MaxCapacity;

        public int NumberOfItems;

        [MarshalAs(UnmanagedType.I1)]
        public bool KeepAliveHost;

        [MarshalAs(UnmanagedType.I1)]
        public bool KeepAlivePlc;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I1, SizeConst = 11)] 
        public bool[] IsFull;

        [MarshalAs(UnmanagedType.I1)]
        public bool StopReleaseHangers;
    }
}
