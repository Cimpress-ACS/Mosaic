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
    [ConfigurationCollection(typeof(ActionConfigBase), AddItemName = "invokeMethod")]
    public class DependencyActionCollectionConfig : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ActionConfigBase();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            if (elementName == "invokeMethod")
            {
                return new InvokeMethodActionConfig();
            }

            throw new ConfigurationErrorsException(string.Format("elementName {0} not implemented!", elementName));
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (ActionConfigBase)element;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMapAlternate; }
        }

        protected override string ElementName
        {
            get { return string.Empty; }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "invokeMethod";
        }
    }

    public class ActionConfigBase : ConfigurationElement
    {
        
    }

    public class InvokeMethodActionConfig : ActionConfigBase
    {
        [ConfigurationProperty("methodName", IsRequired = true)]
        public string MethodName
        {
            get { return (string)base["methodName"]; }
            set { base["methodName"] = value; }
        }

        [ConfigurationProperty("arguments", IsRequired = false)]
        public string Arguments
        {
            get { return (string)base["arguments"]; }
            set { base["arguments"] = value; }
        }
    }
}
