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
using System.Runtime.Serialization;

namespace VP.FF.PT.Common.PlcEssentials.PlcInformation.DTOs
{
    [DataContract]
    public class ControllerDTO
    {
        [DataMember]
        public String ActiveAlarm { get; set; }

        [DataMember]
        public List<TagDTO> ActualValues { get; set; }
            
        [DataMember]
        public List<ControllerDTO> Children { get; set; }

        [DataMember]
        public List<CommandDTO> Commands { get; set; }

        [DataMember]
        public List<TagDTO> Configurations { get; set; }

        [DataMember]
        public AlarmType ControllerState { get; set; }
       
        [DataMember]
        public String CurrentState { get; set; }

        [DataMember]
        public String CurrentSubState { get; set; }

        [DataMember]
        public Boolean EnableForcing { get; set; }

        [DataMember]
        public Int32 Id { get; set; }

        [DataMember]
        public List<TagDTO> Inputs { get; set; }

        [DataMember]
        public Boolean IsEnabled { get; set; }

        [DataMember]
        public Boolean IsSimulation { get; set; }

        [DataMember]
        public ControllerMode Mode { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String FullName { get; set; }

        [DataMember]
        public String PlcControllerPath { get; set; }

        [DataMember]
        public List<TagDTO> Outputs { get; set; }

        [DataMember]
        public List<TagDTO> Parameters { get; set; }

        [DataMember]
        public String Type { get; set; }
    }

    public enum AlarmType
    {
        Normal, Warning, Error,
    }

    [DataContract]
    public class TagDTO
    {
        [DataMember]
        public String Comment { get; set; }

        [DataMember]
        public String Key { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String Type { get; set; }

        [DataMember]
        public String Unit { get; set; }

        [DataMember]
        public String Value { get; set; }

        [DataMember]
        public List<EnumerationMemberDto> EnumerationMembers { get; set; }

        [DataMember]
        public List<TagDTO> Children { get; set; }
    }

    [DataContract]
    public class EnumerationMemberDto
    {
        [DataMember]
        public short Value { get; set; }

        [DataMember]
        public string Comment { get; set; }
    }

    [DataContract]
    public class CommandDTO
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String OptionalText { get; set; }
    }

    [DataContract]
    public class ChangeRequestDTO
    {
        [DataMember]
        public String Key { get; set; }

        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String Type { get; set; }

        [DataMember]
        public String Value { get; set; }

        public override string ToString()
        {
            return string.Format(@"ChangeRequestDTO {{ Name : '{0}', Value : '{1}', Type : '{2}' }}", Name, Value, Type);
        }
    }

    [DataContract]
    public enum ControllerMode
    {
        [EnumMember]
        Initialized = 0,

        [EnumMember]
        Manual = 12,

        [EnumMember]
        Semi = 11,

        [EnumMember]
        Auto = 10
    }
}
