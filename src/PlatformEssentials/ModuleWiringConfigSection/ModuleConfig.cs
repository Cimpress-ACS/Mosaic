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


using System.Configuration;

namespace VP.FF.PT.Common.PlatformEssentials.ModuleWiringConfigSection
{
    public class ModuleConfig : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("type", DefaultValue = "0", IsKey = false, IsRequired = true)]
        public int ModuleTypeId
        {
            get { return (int)base["type"]; }
            set { base["type"] = value; }
        }

        [ConfigurationProperty("contractType", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string ContractType
        {
            get { return (string)base["contractType"]; }
            set { base["contractType"] = value; }
        }

        [ConfigurationProperty("parameter")]
        public ModuleParameterConfig Config
        {
            get { return (ModuleParameterConfig)base["parameter"]; }
            set { base["parameter"] = value; }
        }

        [ConfigurationProperty("nextModules")]
        public ModuleReferencesConfigCollection NextModules
        {
            get { return (ModuleReferencesConfigCollection) base["nextModules"]; }
        }

        [ConfigurationProperty("dependencies")]
        public ModuleDependenciesConfigCollection Dependencies
        {
            get { return (ModuleDependenciesConfigCollection)base["dependencies"]; }
        }
    }
}
