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
using System.ServiceModel;
using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement.Wcf;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement.Wcf
{
    [UseReporter(typeof(NUnitReporter))]
    [TestFixture]
    public class AlarmManagementServiceTests : AlarmManagementServiceTests_Base
    {
        [Test]
        public void Given2AlarmsInLoa1_WhenGetCurrentAlarmsWithLoa1_ThenReturnBothAlarms()
        {
            IAlarmManagementService service = CreateService();
            _loadingStation1.SetupCurrentAlarms(LoadingStationAlarm(1), LoadingStationAlarm(2));
            IEnumerable<AlarmDTO> alarms = service.GetCurrentAlarms(LOA_1());
            Approvals.VerifyAll(alarms, x => x.ToJson());
        }

        [Test]
        public void GivenAlarmInLab1_WhenGetCurrentAlarmsWithLab1_ThenReturnAlarm()
        {
            IAlarmManagementService service = CreateService();
            _labelModule1.SetupCurrentAlarms(LabelModuleAlarm(1));
            IEnumerable<AlarmDTO> alarms = service.GetCurrentAlarms(LAB_1());
            Approvals.VerifyAll(alarms, x => x.ToJson());
        }

        [Test]
        public void GivenAlarmsInLoa1AndLab1_WhenGetCurrentAlarmsOfBothModules_ThenReturnAlarmsOfBothModules()
        {
            IAlarmManagementService service = CreateService();
            _loadingStation1.SetupCurrentAlarms(LoadingStationAlarm(1));
            _labelModule1.SetupCurrentAlarms(LabelModuleAlarm(2));
            Dictionary<string, List<AlarmDTO>> alarms = service.GetCurrentAlarmsOfModules(new Collection<string> { LAB_1(), LOA_1() });
            Approvals.VerifyAll(alarms, x => x.ToJson());
        }

        [Test]
        public void WhenGetCurrentAlarmsWithUnknownModule_ShouldThrowFaultException()
        {
            IAlarmManagementService services = CreateService();
            services.Invoking(x => x.GetCurrentAlarms("Unknown")).ShouldThrow<FaultException<AlarmServiceFault>>();
        }

        [Test]
        public void GivenAlarmInLoa1History_WhenGetHistoricAlarmsWithLoa1_ThenReturnAlarm()
        {
            _loadingStation1.SetupHistoricAlarms(LoadingStationAlarm(1));
            IAlarmManagementService service = CreateService();
            IEnumerable<AlarmDTO> alarms = service.GetHistoricAlarms(LOA_1());
            Approvals.VerifyAll(alarms, x => x.ToJson());
        }

        [Test]
        public void Given2AlarmsInLab1History_WhenGetHistoricAlarmsWithLab1_ShouldReturnBothAlarms()
        {
            _labelModule1.SetupHistoricAlarms(LabelModuleAlarm(3), LabelModuleAlarm(4));
            IAlarmManagementService service = CreateService();
            IEnumerable<AlarmDTO> alarms = service.GetHistoricAlarms(LAB_1());
            Approvals.VerifyAll(alarms, x => x.ToJson());
        }

        [Test]
        public void GivenAlarmsInMultipleModuleHistories_WhenGetHistoricAlarmsOfBothModules_ThenReturnAllAlarms()
        {
            _labelModule1.SetupHistoricAlarms(LabelModuleAlarm(15));
            _loadingStation1.SetupHistoricAlarms(LoadingStationAlarm(3), LoadingStationAlarm(23));
            IAlarmManagementService service = CreateService();
            Dictionary<string, List<AlarmDTO>> alarms = service.GetHistoricAlarmsOfModules(new Collection<string> { LAB_1(), LOA_1() });
            Approvals.VerifyAll(alarms, x => x.ToJson());
        }

        [Test]
        public void WhenGetHistoricAlarmsWithUnknownModule_ShouldThrowFaultException()
        {
            IAlarmManagementService service = CreateService();
            service.Invoking(x => x.GetHistoricAlarms("Blub")).ShouldThrow<FaultException<AlarmServiceFault>>();
        }

        [Test]
        public void WhenAcknowledgeAlarmsWithLoadingStation_ShouldResetAlarmOfLoadingStation()
        {
            IAlarmManagementService service = CreateService();
            service.AcknowledgeAlarms(LOA_1());
            _loadingStation1.VerifyResetAlarm();
        }

        [Test]
        public void WhenAcknowledgeAlarmsWithLabelModule_ShouldResetAlarmOfLabelModule()
        {
            IAlarmManagementService service = CreateService();
            service.AcknowledgeAlarms(LAB_1());
            _labelModule1.VerifyResetAlarm();
        }

        [Test]
        public void WhenAcknowledgeAlarmsWithNull_ShouldResetAllAlarms()
        {
            IAlarmManagementService service = CreateService();
            service.AcknowledgeAlarms(null);
            _loadingStation1.VerifyResetAlarm();
            _labelModule1.VerifyResetAlarm();
        }

        [Test]
        public void WhenAcknowledgeWithUnknownModule_ShouldThrowFaultException()
        {
            IAlarmManagementService service = CreateService();
            service.Invoking(x => x.AcknowledgeAlarms("AnyModule")).ShouldThrow<FaultException<AlarmServiceFault>>();
        }

        [Test]
        public void GivenSubscriber_WhenAlarmsInLoadingStationChanged_ShouldNotifySubscribers()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(LOA_1());
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(LOA_1());
        }

        [Test]
        public void GivenSubscriber_WhenAlarmsInLabelModuleChanged_ShouldNotifySubscribers()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(LAB_1());
            _labelModule1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(LAB_1());
        }

        [Test]
        public void GivenUnsubscriber_WhenAlarmsInLabelModuleChange_ShouldNotNotifyUnsubscriber()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(LAB_1());
            service.UnsubscribeFromAlarmChangesFromModule(LAB_1());
            _labelModule1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().BeNullOrEmpty();
        }
    }
}
