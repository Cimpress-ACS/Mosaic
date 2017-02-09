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
using System.Globalization;

namespace VP.FF.PT.Common.PlatformEssentials.ModuleWiringConfigSection
{
    public class ModuleReferenceConfig : ConfigurationElement
    {
        [ConfigurationProperty("name", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("port", DefaultValue = "0", IsKey = true, IsRequired = false)]
        public int Port
        {
            get { return (int)base["port"]; }
            set { base["port"] = value.ToString(CultureInfo.InvariantCulture); }
        }

        [ConfigurationProperty("targetPort", DefaultValue = "0", IsKey = false, IsRequired = false)]
        public int TargetPort
        {
            get { return (int)base["targetPort"]; }
            set { base["targetPort"] = value.ToString(CultureInfo.InvariantCulture); }
        }
    }
}
