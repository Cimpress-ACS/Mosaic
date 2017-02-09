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


using System.Text.RegularExpressions;

namespace VP.FF.PT.Common.PlcCommunication.Infrastructure
{
    public static class StructuredTextSyntaxRegexHelper
    {
        // checks syntax for: ARRAY [0..9] OF INT  with 3 groups.
        public static readonly Regex ArrayDataTypeRegex = new Regex(@"\bARRAY[ ]?\[(\d+)..(\d+)\][ ]?OF (\w+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // checks syntax for: STRING or STRINT (80) with 1 group for string length (optional)
        public static readonly Regex StringDataTypeRegex = new Regex(@"STRING(?:[ ]?\((\d+)\))?");

        // checks syntax for: POINTER TO with 1 group for string length (optional)
        public static readonly Regex PointerTypeRegex = new Regex("^POINTER\\sTO\\s(?<pointerType>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // checks syntax for an array element in form of --> ElementName [12]
        public static readonly Regex ArrayElementTypeRegex = new Regex(@"(\w+)\[[0-9]+\]$");
    }
}
