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
using System.Linq;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    [Obsolete("use ControllerMode in VP.FF.PT.Common.PlcEssentials instead")]
    public class EnumFactory
    {
        private const String EnumPrefix = "E_";
        private static readonly Dictionary<String, Type> Xref = new Dictionary<String, Type>();

        static EnumFactory()
        {
            Xref.Add("E_Mode", typeof(EMode));
            Xref.Add("E_Spray_Mode", typeof(ESprayMode));
        }

        public static Enum CreateEnum(string enumName)
        {
            String key = enumName;

            String[] tokens = enumName.Split('.');
            if (tokens.Any())
                key = tokens.Last();

            if (Xref.ContainsKey(key))
                return Activator.CreateInstance(Xref[key]) as Enum;

            if (key.StartsWith(EnumPrefix))
                return new EUnknownWrapper();

            return null;
        }

        // //////////////////////////////////
        public enum EUnknownWrapper : ushort
        {

        }

        // //////////////////////////////////
        public enum ESprayMode : ushort
        {
            Off = 0,
            On = 1, 
            Auto = 2,
            Inverted = 3
        }

        // //////////////////////////////////
        public enum EMode : ushort
        {
            Initialized   = ControllerMode.Initialized,
            Automatic     = ControllerMode.Auto,
            SemiAutomatic = ControllerMode.Semi,
            Manual        = ControllerMode.Manual
        }
    }
}
