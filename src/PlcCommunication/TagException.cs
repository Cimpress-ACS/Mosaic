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

namespace VP.FF.PT.Common.PlcCommunication
{
    /// <summary>
    /// Occurs when something Tag related is wrong. E.g. wrong DataType, BitSize or Array dimension mismatch.
    /// </summary>
    public class TagException : Exception
    {
        public TagException(string message, Tag tag)
            :base(message)
        {
            Tag = tag;
        }

        public TagException(string message, Tag tag, Exception innerException)
            : base(message, innerException)
        {
            Tag = tag;
        }

        public Tag Tag { get; private set; }
    }
}
