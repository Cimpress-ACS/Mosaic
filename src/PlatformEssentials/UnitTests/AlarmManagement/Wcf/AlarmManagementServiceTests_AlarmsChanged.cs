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
using System.ServiceModel;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement.Wcf;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.AlarmManagement.Wcf
{
    [TestFixture]
    public class AlarmManagementServiceTests_AlarmsChanged : AlarmManagementServiceTests_Base
    {
        [Test]
        public void OnModuleAfterSubscribtion_ShouldNotifySubscriber()
        {
            IAlarmManagementService serivce = CreateService();
            serivce.SubscribeForAlarmChangesOnModule(LOA_1());
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(LOA_1());
        }

        [Test]
        public void OnDifferentModuleAfterSubscribtion_ShouldNotifySubscriber()
        {
            IAlarmManagementService serivce = CreateService();
            serivce.SubscribeForAlarmChangesOnModule(LAB_1());
            _labelModule1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(LAB_1());
        }

        [Test]
        public void OnModule_ShouldNotNotifySubscriberOfDifferentModule()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(LAB_1());
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().BeNull();
        }

        [Test]
        public void OnModuleAfterUnsubscribtion_ShouldNotNotifyFormerSubscriber()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(LAB_1());
            service.UnsubscribeFromAlarmChangesFromModule(LAB_1());
            _labelModule1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().BeNull();
        }

        [Test]
        public void Unsubscribe_WithNeverSubscribedCallback_ShouldNotThrow()
        {
            IAlarmManagementService service = CreateService();
            service.Invoking(s => s.UnsubscribeFromAlarmChangesFromModule(LAB_1())).ShouldNotThrow<Exception>();
        }

        [Test]
        public void OnModulesAfterSubscribtionWithEmptyString_ShouldNotifySubscriberTwice()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(string.Empty);
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(string.Empty);
            _serviceCallback.Reset();
            _labelModule1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(string.Empty);
        }

        [Test]
        public void OnAnyModuleAfterUnsubscribtionOfEmptyStringSubscriber_ShouldNotNotifyFormerSubscriber()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(string.Empty);
            service.UnsubscribeFromAlarmChangesFromModule(string.Empty);
            _labelModule1.RaiseAlarmsChangedEvent();
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().BeNull();
        }

        [Test]
        public void OnModulesAfterSubscribtionWithNull_ShouldNotifySubscriberTwice()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(null);
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(null);
            _serviceCallback.Reset();
            _labelModule1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().Be(null);
        }

        [Test]
        public void OnAnyModuleAfterUnsubscribtionOfNullSubscriber_ShouldNotNotifyFormerSubscriber()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(null);
            service.UnsubscribeFromAlarmChangesFromModule(null);
            _labelModule1.RaiseAlarmsChangedEvent();
            _loadingStation1.RaiseAlarmsChangedEvent();
            _serviceCallback.ModuleWithLastAlarmsChange.Should().BeNull();
        }

        [Test]
        public void Subscribe_WithNotExistingModule_ShouldThrowFaultException()
        {
            IAlarmManagementService service = CreateService();
            service.Invoking(s => s.SubscribeForAlarmChangesOnModule("BLUB"))
                   .ShouldThrow<FaultException<AlarmServiceFault>>();
        }

        [Test]
        public void Unsubscribe_WithNotExistingModule_ShouldThrowFaultException()
        {
            IAlarmManagementService service = CreateService();
            service.SubscribeForAlarmChangesOnModule(LOA_1());
            service.Invoking(s => s.UnsubscribeFromAlarmChangesFromModule("BLUB"))
                   .ShouldThrow<FaultException<AlarmServiceFault>>();
        }
    }
}
