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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement
{
    /// <summary>
    /// The <see cref="CompositeAlarmManager"/> uses <see cref="IManageCurrentAlarmsPlugin"/> instances
    /// to fullfill his job as <see cref="IAlarmManager"/>.
    /// </summary>
    [Export(typeof(CompositeAlarmManager))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CompositeAlarmManager : IAlarmManager
    {
        private readonly List<IManageCurrentAlarmsPlugin> _currentAlarmsPlugins;
        private IReadOnlyCollection<IManageCurrentAlarmsPlugin> _pluginsSnapshot;
        private readonly ILogger _logger;
        private readonly ConcurrentBag<Alarm> _historicAlarms;

        /// <summary>
        /// Initializes a new <see cref="CompositeAlarmManager"/> instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public CompositeAlarmManager(ILogger logger)
        {
            _currentAlarmsPlugins = new List<IManageCurrentAlarmsPlugin>();
            _pluginsSnapshot = _currentAlarmsPlugins.ToReadOnly();
            _logger = logger;
            _logger.Init("ALARM");
            _historicAlarms = new ConcurrentBag<Alarm>();
            AlarmsChanged += () => { };
        }

        /// <summary>
        /// Initializes a new <see cref="CompositeAlarmManager"/> instance.
        /// </summary>
        /// <param name="currentAlarmsPlugins">The plugin implementations this instance should use.</param>
        /// <param name="logger">The logger.</param>
        public CompositeAlarmManager(
            IEnumerable<IManageCurrentAlarmsPlugin> currentAlarmsPlugins,
            ILogger logger)
            : this(logger)
        {
            foreach (IManageCurrentAlarmsPlugin plugin in currentAlarmsPlugins)
            {
                AddPlugin(plugin);
            }
        }

        /// <summary>
        /// Notifies any subscriber when the <see cref="IAlarmManager.CurrentAlarms"/> or the <see cref="IAlarmManager.HistoricAlarms"/> changed.
        /// </summary>
        public event Action AlarmsChanged;

        /// <summary>
        /// Gets the current alarms.
        /// </summary>
        public IEnumerable<Alarm> CurrentAlarms
        {
            get { return _pluginsSnapshot.SelectMany(p => p.GetCurrentAlarms()); }
        }

        /// <summary>
        /// Gets the historic alarms.
        /// </summary>
        public IEnumerable<Alarm> HistoricAlarms
        {
            get { return _historicAlarms; }
        }

        /// <summary>
        /// Indicates whether there is any current error-level alarm.
        /// </summary>
        public bool HasErrors
        {
            get { return CurrentAlarms.Any(a => a.Type == AlarmType.Error); }
        }

        public string ModuleName { get; set; }

        /// <summary>
        /// Indicates whether there is any current warning-level alarm.
        /// </summary>
        public bool HasWarnings
        {
            get { return CurrentAlarms.Any(a => a.Type == AlarmType.Warning); }
        }

        /// <summary>
        /// Adds a <see cref="IManageCurrentAlarmsPlugin"/> to this composite alarm manager.
        /// </summary>
        /// <param name="plugin">The plugin to be used for current alarms management.</param>
        public void AddPlugin(IManageCurrentAlarmsPlugin plugin)
        {
            lock (_currentAlarmsPlugins)
            {
                _currentAlarmsPlugins.Add(plugin);
                _pluginsSnapshot = _currentAlarmsPlugins.ToReadOnly();
            }
            plugin.AlarmAdded += RaiseAlarmsChangedEvent;
            RaiseAlarmsChangedEvent(null);
        }

        private void RaiseAlarmsChangedEvent(Alarm alarm)
        {
            if (alarm != null)
            {
                _logger.InfoFormat("Id {0}, message '{1}', source '{2}', sourcetype '{3}', type '{4}' at {5:s}", alarm.AlarmId, alarm.Message, alarm.Source,
                    alarm.SourceType, alarm.Type, alarm.Timestamp);
            }

            AlarmsChanged();
        }

        /// <summary>
        /// Removes all plugins of the specified <typeparamref name="TPlugin"/> type.
        /// </summary>
        /// <typeparam name="TPlugin">The type of the plugins to get removed.</typeparam>
        public void RemoveAllPluginsOfType<TPlugin>() where TPlugin : IManageCurrentAlarmsPlugin
        {
            lock (_currentAlarmsPlugins)
            {
                foreach (var pluginToRemove in _pluginsSnapshot.OfType<TPlugin>())
                {
                    _currentAlarmsPlugins.Remove(pluginToRemove);
                    pluginToRemove.AlarmAdded -= RaiseAlarmsChangedEvent;
                }
                _pluginsSnapshot = _currentAlarmsPlugins;
            }
        }

        /// <summary>
        /// Current Alarms will be removed from current alarm list but be still available as historical alarms.
        /// </summary>
        public void AcknowledgeAlarms()
        {
            IEnumerable<Alarm> currentAlarms = CurrentAlarms.ToReadOnly();
            if (currentAlarms.IsNullOrEmpty())
            {
                return;
            }

            foreach (var plugin in _pluginsSnapshot)
            {
                plugin.TryRemoveAlarms(currentAlarms);
            }

            foreach (var alarm in currentAlarms)
            {
                _historicAlarms.Add(alarm);
            }

            AlarmsChanged();
        }

        /// <summary>
        /// Adds the <paramref name="newAlarm"/> to the current alarms.
        /// Will do nothing if the alarm was already added before.
        /// </summary>
        /// <param name="newAlarm">A new <see cref="Alarm"/> instance.</param>
        public void AddAlarm(Alarm newAlarm)
        {
            // get a better timestamp than a default time value if none has been set
            if (newAlarm.Timestamp == default(DateTime))
            {
                newAlarm.Timestamp = DateTime.Now;
            }

            // add the alarm to all plugins that allow dynamic alarm management
            foreach (var plugin in _pluginsSnapshot.OfType<IAlarmAddingPlugin>())
            {
                plugin.TryAddAlarm(newAlarm);
            }
        }

        public void RemoveAlarm(Alarm alarm)
        {
            alarm.Timestamp = DateTime.MinValue;

            foreach (var plugin in _pluginsSnapshot)
            {
                plugin.ForceRemoveAlarms(new Collection<Alarm> { alarm });
            }

            AlarmsChanged();
        }

        public int RemoveAlarm(string source, int alarmId)
        {
            int numOfRemovedAlarms = 0;

            foreach (var plugin in _pluginsSnapshot)
            {
                numOfRemovedAlarms += plugin.ForceRemoveAlarms(source, alarmId);
            }

            if (numOfRemovedAlarms>0)
                AlarmsChanged();

            return numOfRemovedAlarms;
        }
    }
}
