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


namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    public class SimulatedLogixCsvFile
    {
        private static int _row;

        public static void Reset()
        {
            _row = 0;
        }

        /// <summary>
        /// Retruns a well defines Rockwell CSV file which holds some exported tags.
        /// Version 20.0.
        /// </summary>
        public static string GetNextLine()
        {
            string[] csv = 
            {
                "remark,\"CSV-Import-Export\"",
                "remark,\"Date = Tue May 15 16:35:23 2012\"",
                "remark,\"Version = RSLogix 5000 v20.00\"",
                "remark,\"Owner = Rolf Englputzeder\"",
                "remark,\"Company = Vistaprint\"",
                "0.3",
                "TYPE,SCOPE,NAME,DESCRIPTION,DATATYPE,SPECIFIER,ATTRIBUTES",
                "TAG,,DistributedEthernetIO:1:C,\"\",\"AB:1738_DI8:C:0\",\"\",\"(ExternalAccess := Read/Write)\"",
                "ALIAS,,test_alias_tag,\"\",\"\",\"DistributedEthernetIO:2:O.0\",\"(RADIX := Decimal, ExternalAccess := Read/Write)\"",
                "TYPE,SCOPE,ROUTINE,COMMENT,OWNING_ELEMENT,LOCATION",
                "RCOMMENT,\"FolderModule\",\"KeepAlive\",\"========== State off ===========\",\"OTL(State_KeepAlive_runBusy)\",\"0\""
            };

            if (_row >= csv.Length)
            {
                return null;
            }

            return csv[_row++];
        }
    }
}
