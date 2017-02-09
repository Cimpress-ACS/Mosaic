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
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using VP.FF.PT.Common.Infrastructure;

namespace VP.FF.PT.Common.PlatformEssentials.ConfigurationService
{
    /// <summary>
    /// The configuration service provides a simple key value storage as a WCF service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    [Export(typeof(IConfigurationService))]
    public class ConfigurationService : IConfigurationService
    {
        private readonly ApplicationSettingsBase _settings;
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// Empty constructor is needed to download service interface at design time.
        /// </summary>
        public ConfigurationService()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="applicationSettingsFactory">The application settings factory. The specfic factory needs to be implemented in the target project as this is not part of PlatformEssentials.</param>
        [ImportingConstructor]
        public ConfigurationService(IEventAggregator eventAggregator, IApplicationSettingsBaseFactory applicationSettingsFactory)
        {
            _eventAggregator = eventAggregator;
            _settings = applicationSettingsFactory.CreateDefault();
            _settings.Reload();

            EnsureValueCollectionPopulated();

            IsInitialized = true;
        }

        public bool KeepAlive()
        {
            return true;
        }

        public string GetValue(string key)
        {
            return GetValue<string>(key);
        }

        public bool GetBoolValue(string key)
        {
            return GetValue<bool>(key);
        }

        public void SetValue(string key, string value)
        {
            SetValue<string>(key, value);
        }

        public void SetBoolValue(string key, bool value)
        {
            SetValue(key, value);
        }

        public IEnumerable<string> GetAllKeys()
        {
            var ret = new List<string>();

            foreach (SettingsProperty property in _settings.Properties)
            {
                ret.Add(property.Name);
            }

            return ret;
        }

        public bool IsInitialized { get; private set; }

        private T GetValue<T>(string key)
        {
            var value = _settings.PropertyValues[key];
            if (value != null)
            {
                return (T) Convert.ChangeType(value.PropertyValue, typeof(T));
            }

            throw new ConfigurationKeyNotFoundException(string.Format("Cannot get value from configuration for key {0}", key), key);
        }

        private void SetValue<T>(string key, T value)
        {
            var entry = _settings.PropertyValues[key];
            if (entry != null)
            {
                entry.PropertyValue = value;
                _settings.Save();
            }
            else
            {
                throw new ConfigurationKeyNotFoundException(string.Format("Cannot set value to configuration for key {0}", key), key);
            }

            _eventAggregator.Publish(new ConfigurationChangedEvent(key, value));
        }

        // this is a workaround to make sure the lazy loaded collections are populated with values.
        // see also: http://stackoverflow.com/questions/2565357/using-application-settings-and-reading-defaults-from-app-config
        private void EnsureValueCollectionPopulated()
        {
            var firstKey = GetAllKeys().FirstOrDefault();

            if (!string.IsNullOrEmpty(firstKey))
            {
                var dummy = _settings[firstKey];
            }
        }
    }
}
