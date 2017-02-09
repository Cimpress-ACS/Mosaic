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
    [ConfigurationCollection(typeof(TriggerConfigBase), AddItemName = "eventTrigger,eventNotRaisedTrigger,propertyChangedTrigger,propertyNotChangedTrigger,periodicTimeTrigger")]

    public class DependencyTriggerCollectionConfig : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TriggerConfigBase();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            if (elementName == "eventTrigger")
            {
                return new EventTriggerConfig();
            }
            if (elementName == "eventNotRaisedTrigger")
            {
                return new EventNotRaisedTriggerConfig();
            }
            if (elementName == "propertyChangedTrigger")
            {
                return new PropertyChangedTriggerConfig();
            }
            if (elementName == "propertyNotChangedTrigger")
            {
                return new PropertyNotChangedTriggerConfig();
            }
            if (elementName == "periodicTimeTrigger")
            {
                return new PeriodicTimeTriggerConfig();
            }

            throw new ConfigurationErrorsException(string.Format("elementName {0} not implemented!", elementName));
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (TriggerConfigBase)element;
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
            return elementName == "eventTrigger" ||
                   elementName == "eventNotRaisedTrigger" ||
                   elementName == "propertyNotChangedTrigger" || 
                   elementName == "propertyChangedTrigger" ||
                   elementName == "periodicTimeTrigger";
        }
    }

    public class TriggerConfigBase : ConfigurationElement
    {
        [ConfigurationProperty("path")]
        public string Path
        {
            get { return (string)base["path"]; }
            set { base["path"] = value; }
        }
    }

    public class EventTriggerConfig : TriggerConfigBase
    {
        [ConfigurationProperty("eventName", IsRequired = true)]
        public string EventName
        {
            get { return (string) base["eventName"]; }
            set { base["eventName"] = value; }
        }
    }

    public class EventNotRaisedTriggerConfig : TriggerConfigBase
    {
        [ConfigurationProperty("eventName", IsRequired = true)]
        public string EventName
        {
            get { return (string)base["eventName"]; }
            set { base["eventName"] = value; }
        }

        [ConfigurationProperty("timeout", IsRequired = true)]
        public int Timeout
        {
            get { return (int)base["timeout"]; }
            set { base["timeout"] = value; }
        }
    }

    public class PropertyChangedTriggerConfig : TriggerConfigBase
    {
        [ConfigurationProperty("propertyName", IsRequired = true)]
        public string PropertyName
        {
            get { return (string)base["propertyName"]; }
            set { base["propertyName"] = value; }
        }
    }

    public class PropertyNotChangedTriggerConfig : TriggerConfigBase
    {
        [ConfigurationProperty("propertyName", IsRequired = true)]
        public string PropertyName
        {
            get { return (string)base["propertyName"]; }
            set { base["propertyName"] = value; }
        }

        [ConfigurationProperty("timeout", IsRequired = true)]
        public int Timeout
        {
            get { return (int)base["timeout"]; }
            set { base["timeout"] = value; }
        }
    }

    public class PeriodicTimeTriggerConfig : TriggerConfigBase
    {
        [ConfigurationProperty("timeout", IsRequired = true)]
        public int Timeout
        {
            get { return (int)base["timeout"]; }
            set { base["timeout"] = value; }
        }
    }
}
