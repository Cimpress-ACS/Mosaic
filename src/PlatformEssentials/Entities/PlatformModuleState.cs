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


using System.Runtime.Serialization;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    [DataContract]
    public enum PlatformModuleState : int
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Initializing = 2,
        [EnumMember]
        Initialized = 1,
        [EnumMember]
        NotInitialized = -1,
        [EnumMember]
        OffBusy = 8,
        [EnumMember]
        Off = 3,
        [EnumMember]
        RunBusy = 7,
        [EnumMember]
        Run = 4,
        [EnumMember]
        StandbyBusy = 10,
        [EnumMember]
        Standby = 9,
        [EnumMember]
        Error = 5,
        [EnumMember]
        Disabled = 6
    }
}
