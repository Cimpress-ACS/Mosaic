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
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcEssentials;
using VP.FF.PT.Common.PlcEssentials.Impl;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.PlcBaseCommunication
{
    /// <summary>
    /// The <see cref="BeckhoffOnlineAlarmsImporter"/> is capable of importing alarms
    /// over a <see cref="ITagListener"/> instance and expose them to other parts.
    /// </summary>
    [Export(typeof(IAlarmsImporter))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BeckhoffOnlineAlarmsImporter : IAlarmsImporter
    {
        private readonly ILogger _logger;
        private readonly List<PlcAlarmStruct> _cachedAlarms;
        private ITagListener _tagListener;
        private Tag _alarmsArrayTag;
        private IReadOnlyCollection<PlcAlarmStruct> _alarmsSnapshot;

        /// <summary>
        /// Initializes a new <see cref="BeckhoffOnlineAlarmsImporter"/> instance.
        /// </summary>
        /// <param name="logger"></param>
        [ImportingConstructor]
        public BeckhoffOnlineAlarmsImporter(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
            _cachedAlarms = new List<PlcAlarmStruct>();
            _alarmsSnapshot = _cachedAlarms.ToReadOnly();
            AlarmsChanged += a => { };
        }

        /// <summary>
        /// Initializes a new <see cref="BeckhoffOnlineAlarmsImporter"/> instance.
        /// </summary>
        /// <remarks>
        /// This constructor mainly exists because of backwards compatibility. It creates an own
        /// <see cref="ITagListener"/> instance which may not be in your interest if you try to 
        /// keep control over those resource intensive instances.
        /// </remarks>
        public BeckhoffOnlineAlarmsImporter()
            :this(new Log4NetLogger())
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BeckhoffOnlineAlarmsImporter" /> instance.
        /// </summary>
        /// <param name="adsAddress">The ads address the tag listener should communicate with.</param>
        /// <param name="adsPort">The ads port the tag listener should communicate with.</param>
        /// <param name="globalLock">The global lock is a singleton and hold a global semaphor needed for TagListener synchronization.</param>
        /// <remarks>
        /// This constructor mainly exists because of backwards compatibility. It will not create an own
        /// <see cref="ITagListener" /> instance which means, that the initialize method must get called.
        /// </remarks>
        public BeckhoffOnlineAlarmsImporter(string adsAddress, int adsPort, IGlobalLock globalLock)
            : this()
        {
            _tagListener = new BeckhoffPollingTagListener(adsAddress, adsPort, globalLock);
        }

        /// <summary>
        /// Notifies the subscribers about changed alarms.
        /// </summary>
        public event Action<IEnumerable<Alarm>> AlarmsChanged;

        /// <summary>
        /// Initializes this alarms importer.
        /// </summary>
        /// <param name="tagListener">The tag listener to get used by this instance.</param>
        public void Initialize(ITagListener tagListener)
        {
            _logger.Debug(string.Format("Initializing alarms importer on '{0}'", tagListener));
            _tagListener = tagListener;
            _alarmsArrayTag = InitializeGlobalAlarmsTag();
            _alarmsArrayTag.ValueChanged += HandleAlarmsChanged;
            _tagListener.AddTag(_alarmsArrayTag);
        }

        private Tag InitializeGlobalAlarmsTag()
        {
            var maxAlarmsTag = Alarm.PlcMaximumAlarmsTag();
            _tagListener.ReadTagSynchronously(maxAlarmsTag);
            var arrayLength = (ushort) maxAlarmsTag.Value;

            string plcArrayDataType = Alarm.PlcArrayDataType(arrayLength);
            _tagListener.AddUdtHandler<PlcAlarmStruct>(plcArrayDataType);

            return Alarm.PlcArrayTag(arrayLength);
        }

        /// <summary>
        /// Imports all alarms from the plc.
        /// </summary>
        public void ImportAlarms()
        {
            _logger.Debug(string.Format("Import alarms on '{0}'", _tagListener));
            _tagListener.ReadTagSynchronously(_alarmsArrayTag);
            IEnumerable<PlcAlarmStruct> alarmStructs = _alarmsArrayTag.ArrayValues<PlcAlarmStruct>();
            UpdateCache(alarmStructs);
            _logger.Debug(string.Format("Finished alarm import on '{0}'", _tagListener));
        }

        private void UpdateCache(IEnumerable<PlcAlarmStruct> alarmStructs)
        {
            lock (_cachedAlarms)
            {
                _cachedAlarms.Clear();
                foreach (PlcAlarmStruct plcAlarm in alarmStructs)
                    if (plcAlarm != null)
                        _cachedAlarms.Add(plcAlarm);
                _alarmsSnapshot = _cachedAlarms.ToReadOnly();
            }
        }

        /// <summary>
        /// Gets all alarms which were previously imported.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        public IEnumerable<Alarm> GetAllImportedAlarms()
        {
            return _alarmsSnapshot.Select(Alarm.CopyPlcAlarmToAlarm).ToReadOnly();
        }

        /// <summary>
        /// Gets the alarms from the controller with the specified <paramref name="controllerId"/>.
        /// </summary>
        /// <param name="controllerId">The controller id of the desired alarms.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Alarm"/> instances.</returns>
        public IEnumerable<Alarm> GetAlarms(int controllerId)
        {
            return _alarmsSnapshot.Where(a => a.SourceId == controllerId).Select(Alarm.CopyPlcAlarmToAlarm).ToReadOnly();
        }
        
        private void HandleAlarmsChanged(Tag sender, TagValueChangedEventArgs e)
        {
            IEnumerable<PlcAlarmStruct> alarmStructs = sender.ArrayValues<PlcAlarmStruct>();
            UpdateCache(alarmStructs);
            AlarmsChanged(GetAllImportedAlarms());
        }
    }
}
