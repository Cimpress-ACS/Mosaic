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

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    public static class ConstantsAlarmManager
    {
        public const int MaxAlarmElements = 99;
    }

    // this type is used in global AlarmManager

    /*
    TYPE T_AlmElement :
    STRUCT
	    ///unique alarm number := 100000*udiMachineNr + intCtrlId*100 + udiAlarmNr
	    udiNr: UDINT;
	    ///additional information - better string???
	    dinInfo: DINT;
	    ///corresponding alarm class
	    eClass: E_AlmClass;
	    ///clear, resetable
	    intState: E_AlmState;
	    ///entry time
	    strDTEntry: STRING;
	    ///from machine (standard = 1)
	    udiMachineNr: UDINT;
	    ///from controller
	    intCtrlId: INT;
	    ///local alarm number
	    udiAlarmNr: UDINT;
	    // for debugging string alarm controller-alarm
	    strText: STRING;
    END_STRUCT
    END_TYPE
     * */
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public class PlcAlarmStruct
    {
        public uint Id;

        public int Info;

        public short AlarmClass; // see AlarmType enum

        public short AlarmState;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        // YYYY-MM-DD-hh:mm:ss.xxx
        public string EntryTime = "";

        public uint MachineNumber;

        public short SourceId;

        public uint AlarmNumber;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string Text = "";
    }

    // this type is used in available alarm list in controller
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public class AvailableAlarm
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool Available;

        public short AlarmClass; // see AlarmType enum

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
        public string Text = "";
    }
}
