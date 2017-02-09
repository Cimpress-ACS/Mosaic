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
    public static class TagName
    {
        public static string Global()
        {
            return NamingConventions.Global;
        }

        public static string AlarmManager(this string parent)
        {
            return parent.AppendTagName(NamingConventions.PathAlarmManager);
        }

        public static string AlarmCommand(this string parent)
        {
            return parent.AppendTagName(NamingConventions.AlarmManagerCommandChannel);
        }

        public static string ControllerArray(this string parent)
        {
            return AppendTagName(parent, NamingConventions.PathGlobalControllerArray);
        }

        public static string CommonInterface(this string parent)
        {
            return AppendTagName(parent, NamingConventions.CommonInterface);
        }

        public static string Job(this string parent)
        {
            return AppendTagName(parent, NamingConventions.CommonInterfaceJob);
        }

        public static string Mode(this string parent)
        {
            return AppendTagName(parent, NamingConventions.ControllerMode);
        }

        public static string ManualCommand(this string parent)
        {
            return AppendTagName(parent, NamingConventions.ManualCommand);
        }

        public static string State(this string parent)
        {
            return AppendTagName(parent, NamingConventions.StateString);
        }

        public static string SubState(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SubStateString);
        }

        public static string CmdList(this string parent, int index = -1)
        {
            string cmdListSegment = NamingConventions.CmdList;
            if (index > -1)
                cmdListSegment = string.Format("{0}[{1}]", cmdListSegment, index);
            return AppendTagName(parent, cmdListSegment);
        }

        public static string SpecificInterface(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterface);
        }

        public static string Parameters(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterfaceParameter);
        }

        public static string ActualValues(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterfaceActualValues);
        }
        
        public static string Configurations(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterfaceConfigurartion);
        }

        public static string Outputs(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterfaceOutputs);
        }

        public static string Inputs(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterfaceInputs);
        }

        public static string Alarms(this string parent)
        {
            return AppendTagName(parent, NamingConventions.SpecificInterfacePossibleAlarms);
        }

        public static string AppendTagName(this string parent, string child)
        {
            if (parent == null)
                return child;
            if (child == null)
                return parent;
            return string.Format("{0}.{1}", parent, child);
        }
    }
}
