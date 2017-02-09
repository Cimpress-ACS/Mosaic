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

namespace VP.FF.PT.Common.PlcCommunication.Infrastructure
{
    /// <summary>
    /// Supported standard data types defined in the IEC61131-3 standard.
    /// </summary>
    public static class IEC61131_3_DataTypes
    {
        public static readonly Dictionary<string, int> BitSizes = new Dictionary<string, int>
            {
                {Boolean, 8},
                {Byte, 8},
                {SInt, 8},
                {USInt, 8},
                {UInt, 16},
                {Int, 16},
                {DInt, 32},
                {LInt, 64},
                {String, 8}, // for one single character
                {Real, 32},
                {LReal, 64},
                {Word, 16},
                {DWord, 32},
                {LWord, 64},
                {ULInt, 64},
                {UDInt, 32},
            };

        public static readonly Dictionary<string, Type> NetDataTypes = new Dictionary<string, Type>
            {
                {Boolean, typeof (bool)},
                {Byte, typeof (byte)},
                {SInt, typeof (sbyte)},
                {USInt, typeof(byte)},
                {UInt, typeof(ushort)},
                {Int, typeof (short)},
                {DInt, typeof (int)},
                {LInt, typeof(long)},
                {String, typeof (string)},
                {Real, typeof (float)},
                {LReal, typeof (double)},
                {Word, typeof (ushort)},
                {DWord, typeof (uint)},
                {LWord, typeof (ulong)},
                {ULInt, typeof (ulong)},
                {UDInt, typeof (uint)},
            };

        public const string Boolean = "BOOL";       // TRUE, FALSE.  8 bit
        public const string Byte = "BYTE";          // 0 - 255
        public const string SInt = "SINT";          // -128 - 127
        public const string USInt = "USINT";        // 0 - 255, 8 bit
        public const string UInt = "UINT";          // 0 - 65535, 16 bit
        public const string Int = "INT";            // -32768 - 32767
        public const string DInt = "DINT";          // -2147483648 - 2147483647
        public const string LInt = "LINT";          // -2e63 - 2e63-1
        public const string String = "STRING";      // default size of 1 + 80 byte
        public const string Real = "REAL";          // 1.175494351e-38 - 3.402823466e+38
        public const string LReal = "LREAL";        // 2.2250738585072014e-308 - 1.7976931348623158e+308
        public const string Word = "WORD";          // 0 - 65535, 16 bit
        public const string DWord = "DWORD";        // 0 - 4294967295, 32 bit
        public const string LWord = "LWORD";        // 0 - 2e64, 64 bit
        public const string ULInt = "ULINT";        // 0 - 2e64, 64 bit
        public const string UDInt = "UDINT";        // 0 - 4294967295, 32 bit
    }
}
