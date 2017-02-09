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


using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.AlarmManagement.Wcf
{
    /// <summary>
    /// An implementer of <see cref="IAlarmManagementService"/> provides service method for interact with alarms
    /// of platform modules.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IAlarmManagementServiceCallback))]
    public interface IAlarmManagementService
    {
        [OperationContract]
        bool KeepAlive();

        [OperationContract]
        List<AlarmDTO> GetCurrentAlarms(string moduleName);

        [OperationContract]
        Dictionary<string, List<AlarmDTO>> GetCurrentAlarmsOfModules(ICollection<string> moduleNames);

        [OperationContract]
        List<AlarmDTO> GetHistoricAlarms(string moduleName);

        [OperationContract]
        Dictionary<string, List<AlarmDTO>> GetHistoricAlarmsOfModules(ICollection<string> moduleNames);

        [OperationContract]
        void AcknowledgeAlarms(string moduleName);

        /// <summary>
        /// Subscribes the <see cref="IAlarmManagementServiceCallback"/> in the current 
        /// context to get invoked whenever the alarms of the module with the specified <paramref name="moduleName"/> change.
        /// </summary>
        /// <param name="moduleName">The name of the module which alarm changes should get observed.</param>
        [OperationContract]
        void SubscribeForAlarmChangesOnModule(string moduleName);

        [OperationContract]
        void SubscribeForAlarmChangesOnModules(ICollection<string> moduleNames);

        /// <summary>
        /// Unsubscribes the <see cref="IAlarmManagementServiceCallback"/> in the current context
        /// from getting invoked when the alarms of the moduel with the specified <paramref name="moduleName"/> change.
        /// </summary>
        /// <param name="moduleName">The name of the module which alarm changes should not anyomre get observed.</param>
        [OperationContract]
        void UnsubscribeFromAlarmChangesFromModule(string moduleName);

        [OperationContract]
        void UnsubscribeFromAlarmChangesFromModules(ICollection<string> moduleNames);
    }

    [DataContract]
    public class AlarmServiceFault
    {
        public AlarmServiceFault()
        {
        }

        public AlarmServiceFault(string reason)
        {
            Reason = reason;
        }

        [DataMember]
        public string Reason { get; set; }
    }
}
