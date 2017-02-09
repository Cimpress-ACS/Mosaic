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

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    [Serializable]
    public class RouteItem
    {
        public RouteItem()
        {
            ProcessSettings = new HashSet<Value>();
            ProcessData = new HashSet<Value>();
        }
    
        public long Id { get; set; }

        public int ModuleType { get; set; }

        public Nullable<int> OverrideModuleType { get; set; }

        public int Index { get; set; }

        public string ForceModuleInstance { get; set; }

        public int ForbiddenModuleType { get; set; }

        public long ProcessValueForeignKey { get; set; }

        public virtual ICollection<Value> ProcessSettings { get; set; }

        public virtual ICollection<Value> ProcessData { get; set; }
    }
}
