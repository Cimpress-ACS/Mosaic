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
    public class ModuleParameterConfig : ConfigurationElement
    {
        [ConfigurationProperty("adsPort", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string PlcPort
        {
            get { return (string)base["adsPort"]; }
            set { base["adsPort"] = value; }
        }

        [ConfigurationProperty("plcRootController", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string PlcRootController
        {
            get { return (string)base["plcRootController"]; }
            set { base["plcRootController"] = value; }
        }

        [ConfigurationProperty("initialParametersFilePath", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string InitialParametersFilePath
        {
            get { return (string) base["initialParametersFilePath"]; }
            set { base["initialParametersFilePath"] = value; }
        }

        [ConfigurationProperty("plcAddress", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string PlcAddress
        {
            get { return (string)base["plcAddress"]; }
            set { base["plcAddress"] = value; }
        }

        [ConfigurationProperty("enableStateLogging", DefaultValue = "False", IsKey = false, IsRequired = false)]
        public string EnableStateLogging
        {
            get { return (string)base["enableStateLogging"]; }
            set { base["enableStateLogging"] = value; }
        }

        [ConfigurationProperty("enableSubStateLogging", DefaultValue = "False", IsKey = false, IsRequired = false)]
        public string EnableSubStateLogging
        {
            get { return (string)base["enableSubStateLogging"]; }
            set { base["enableSubStateLogging"] = value; }
        }

        [ConfigurationProperty("moduleNbr", DefaultValue = "1", IsKey = false, IsRequired = false)]
        public string ModuleNbr
        {
            get { return (string)base["moduleNbr"]; }
            set { base["moduleNbr"] = value; }
        }

        [ConfigurationProperty("plannedThroughputHour", DefaultValue = "0", IsKey = false, IsRequired = false)]
        public string PlannedThroughputPerHour
        {
            get { return (string)base["plannedThroughputHour"]; }
            set { base["plannedThroughputHour"] = value; }
        }
    }
}
