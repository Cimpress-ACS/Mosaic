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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationSimulator
{
    public static class TagHelper
    {
        public static void SetDefaultValue(Tag tag)
        {
            if (tag.Value != null || tag.DataType == null)
                return;

            var dataType = tag.DataType;

            Type netType;
            if (IEC61131_3_DataTypes.NetDataTypes.TryGetValue(dataType, out netType) && netType.IsValueType)
            {
                tag.Value = Activator.CreateInstance(netType);
            }
            else if (dataType == IEC61131_3_DataTypes.String && tag.Value == null)
            {
                tag.Value = "<simulated>";
            }
        }

        public static void CopyTagMetadata(Tag sourceTag, Tag targetTag)
        {
            targetTag.DataType = sourceTag.DataType;
            targetTag.Name = sourceTag.Name;
            targetTag.Scope = sourceTag.Scope;
            targetTag.AdsPort = sourceTag.AdsPort;
            targetTag.Type = sourceTag.Type;
            targetTag.Parent = sourceTag.Parent;
            foreach (var child in sourceTag.Childs)
                targetTag.Childs.Add(child);
        }
    }
}
