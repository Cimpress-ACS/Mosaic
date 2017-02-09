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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.ServiceModel;
using System.Text;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.Infrastructure.Wcf;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement.Wcf
{
    [Export(typeof(IAlarmManagementService))]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AlarmManagementService : IAlarmManagementService
    {
        private readonly IPlatformModuleRepository _platformModuleRepository;
        private readonly ICallbackChannelProvider _callbackChannelProvider;
        private readonly ISafeEventRaiser _eventRaiser;
        private readonly ILogger _logger;
        private readonly ICollection<Subscriber> _subscribers;

        public AlarmManagementService()
        {
        }

        [ImportingConstructor]
        public AlarmManagementService(
            IPlatformModuleRepository platformModuleRepository,
            ICallbackChannelProvider callbackChannelProvider,
            ISafeEventRaiser eventRaiser,
            ILogger logger)
        {
            _platformModuleRepository = platformModuleRepository;
            _callbackChannelProvider = callbackChannelProvider;
            _eventRaiser = eventRaiser;
            _logger = logger;
            _subscribers = new Collection<Subscriber>();
            _logger.Init(GetType());
        }

        public bool KeepAlive()
        {
            return true;
        }

        public List<AlarmDTO> GetCurrentAlarms(string moduleName)
        {
            try
            {
                IPlatformModule platformModule = GetModule(moduleName);
                if (platformModule == null)
                    throw new InvalidOperationException(new StringBuilder().AppendLine()
                        .AppendFormat("Tried to get current alarms of module '{0}'.", moduleName).AppendLine()
                        .AppendFormat("There is no module with the specified name.").ToString());

                return SortAlarms(platformModule.AlarmManager.CurrentAlarms).ToDTOs();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                throw new FaultException<AlarmServiceFault>(new AlarmServiceFault(), e.Message);
            }
        }

        public Dictionary<string, List<AlarmDTO>> GetCurrentAlarmsOfModules(ICollection<string> moduleNames)
        {
            var dictionary = new Dictionary<string, List<AlarmDTO>>();
            foreach (var moduleName in moduleNames)
            {
                dictionary.Add(moduleName, GetCurrentAlarms(moduleName));
            }

            return dictionary;
        }

        private IPlatformModule GetModule(string moduleName)
        {
            return _platformModuleRepository.GetModule(moduleName);
        }

        public List<AlarmDTO> GetHistoricAlarms(string moduleName)
        {
            try
            {
                IPlatformModule platformModule = GetModule(moduleName);
                if (platformModule == null)
                    throw new InvalidOperationException(new StringBuilder().AppendLine()
                        .AppendFormat("Tried to get historic alarms of module '{0}'.", moduleName).AppendLine()
                        .AppendFormat("There is no module with the specified name.").ToString());

                return SortAlarms(platformModule.AlarmManager.HistoricAlarms).ToDTOs();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                throw new FaultException<AlarmServiceFault>(new AlarmServiceFault(), e.Message);
            }
        }

        public Dictionary<string, List<AlarmDTO>> GetHistoricAlarmsOfModules(ICollection<string> moduleNames)
        {
            var dictionary = new Dictionary<string, List<AlarmDTO>>();
            foreach (var moduleName in moduleNames)
            {
                dictionary.Add(moduleName, GetHistoricAlarms(moduleName));
            }

            return dictionary;
        }

        private static IEnumerable<Alarm> SortAlarms(IEnumerable<Alarm> alarms)
        {
            return alarms.OrderByDescending(alarm => alarm.Timestamp);
        }

        public void AcknowledgeAlarms(string moduleName)
        {
            try
            {
                _logger.InfoFormat("Acknowledge current alarms on {0}.", FormatModuleName(moduleName));
                if (string.IsNullOrEmpty(moduleName))
                {
                    IEnumerable<IPlatformModule> modules = _platformModuleRepository.Modules;
                    foreach (IPlatformModule module in modules)
                        module.AlarmManager.AcknowledgeAlarms();
                    return;
                }
                IPlatformModule platformModule = GetModule(moduleName);
                if (platformModule == null)
                    throw new InvalidOperationException(new StringBuilder().AppendLine()
                        .AppendFormat("Tried to acknowledge alarms of module '{0}'.", moduleName).AppendLine()
                        .AppendFormat("There is no module with the specified name.").ToString());
                platformModule.AlarmManager.AcknowledgeAlarms();
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                throw new FaultException<AlarmServiceFault>(new AlarmServiceFault(), e.Message);
            }
        }

        /// <summary>
        /// Subscribes the <see cref="IAlarmManagementServiceCallback"/> int the current 
        /// context to get invoked whenever the alarms of the module with the specified <paramref name="moduleName"/> change.
        /// </summary>
        /// <param name="moduleName">The name of the module which alarm changes should get observed.</param>
        public void SubscribeForAlarmChangesOnModule(string moduleName)
        {
            try
            {
                var callbackChannel = _callbackChannelProvider.GetCallbackChannel<IAlarmManagementServiceCallback>();
                if (!_subscribers.Any(s => Equals(s.Callback, callbackChannel)))
                {
                    var subscriber = new Subscriber(moduleName, _eventRaiser, callbackChannel);
                    ExecuteOnModule(moduleName,
                        module => module.AlarmManager.AlarmsChanged += subscriber.TriggerCallbackEvent);
                    _subscribers.Add(subscriber);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                throw new FaultException<AlarmServiceFault>(new AlarmServiceFault(), e.Message);
            }
        }

        public void SubscribeForAlarmChangesOnModules(ICollection<string> moduleNames)
        {
            try
            {
                var callbackChannel = _callbackChannelProvider.GetCallbackChannel<IAlarmManagementServiceCallback>();
                if (!_subscribers.Any(s => Equals(s.Callback, callbackChannel)))
                {
                    foreach (var moduleName in moduleNames)
                    {
                        var subscriber = new Subscriber(moduleName, _eventRaiser, callbackChannel);
                        ExecuteOnModule(moduleName,
                            module => module.AlarmManager.AlarmsChanged += subscriber.TriggerCallbackEvent);
                        _subscribers.Add(subscriber);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                throw new FaultException<AlarmServiceFault>(new AlarmServiceFault(), e.Message);
            }
        }

        private void ExecuteOnModule(string moduleName, Action<IPlatformModule> actionToExecute)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                foreach (string platformModuleName in _platformModuleRepository.Modules.Select(m => m.Name))
                    ExecuteOnModule(platformModuleName, actionToExecute);
                return;
            }
            IPlatformModule platformModule = _platformModuleRepository.GetModule(moduleName);
            actionToExecute(platformModule);
        }

        /// <summary>
        /// Unsubscribes the <see cref="IAlarmManagementServiceCallback"/> in the current context
        /// from getting invoked when the alarms of the moduel with the specified <paramref name="moduleName"/> change.
        /// </summary>
        /// <param name="moduleName">The name of the module which alarm changes should not anyomre get observed.</param>
        public void UnsubscribeFromAlarmChangesFromModule(string moduleName)
        {
            try
            {
                var callbackChannel = _callbackChannelProvider.GetCallbackChannel<IAlarmManagementServiceCallback>();
                Subscriber subscriber = _subscribers.FirstOrDefault(s => Equals(callbackChannel, s.Callback));
                if (subscriber != null)
                {
                    ExecuteOnModule(moduleName, module => module.AlarmManager.AlarmsChanged -= subscriber.TriggerCallbackEvent);
                    _subscribers.Remove(subscriber);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message, e);
                throw new FaultException<AlarmServiceFault>(new AlarmServiceFault(), e.Message);
            }
        }

        public void UnsubscribeFromAlarmChangesFromModules(ICollection<string> moduleNames)
        {
            foreach (var moduleName in moduleNames)
            {
                UnsubscribeFromAlarmChangesFromModule(moduleName);
            }
        }

        private static string FormatModuleName(string moduleName)
        {
            return string.IsNullOrEmpty(moduleName) ? "all modules" : string.Format("module '{0}'", moduleName);
        }

        private class Subscriber : WcfEventSubscriber<IAlarmManagementServiceCallback>
        {
            private readonly string _moduleName;

            private event Action<string> AlarmsChanged;

            public Subscriber(string moduleName, ISafeEventRaiser eventRaiser, IAlarmManagementServiceCallback remoteSubscriber)
                : base(eventRaiser, remoteSubscriber)
            {
                _moduleName = moduleName;
                AlarmsChanged += remoteSubscriber.AlarmsChanged;
            }

            public void TriggerCallbackEvent()
            {
                EventRaiser.Raise(ref AlarmsChanged, _moduleName);
            }
        }
    }
}
