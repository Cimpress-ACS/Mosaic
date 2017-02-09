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

namespace VP.FF.PT.Common.PlcCommunicationLogix.IngearImplementation
{
    public static class IngearHelper
    {
        public static Logix.Tag.ATOMIC ParseNetLogixDataType(string dataType)
        {
            Logix.Tag.ATOMIC parsed;

            if (dataType == string.Empty)
                parsed = Logix.Tag.ATOMIC.OBJECT;   // could be an Alias...
            else if (dataType.Equals("BOOL", StringComparison.InvariantCultureIgnoreCase))
                parsed = Logix.Tag.ATOMIC.BOOL;
            else if (dataType.Equals("DINT", StringComparison.InvariantCultureIgnoreCase))
                parsed = Logix.Tag.ATOMIC.DINT;
            else if (dataType.Equals("SINT", StringComparison.InvariantCultureIgnoreCase))
                parsed = Logix.Tag.ATOMIC.SINT;
            else if (dataType.Equals("LINT", StringComparison.InvariantCultureIgnoreCase))
                parsed = Logix.Tag.ATOMIC.LINT;
            else if (dataType.Equals("STRING", StringComparison.InvariantCultureIgnoreCase))
                parsed = Logix.Tag.ATOMIC.STRING;
            else if (dataType.Equals("REAL", StringComparison.InvariantCultureIgnoreCase))
                parsed = Logix.Tag.ATOMIC.REAL;
            else
                parsed = Logix.Tag.ATOMIC.OBJECT;

            return parsed;
        }
    }
}
