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
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    public class VirtualPlatformModule : IPlatformModule
    {
        public virtual event EventHandler<ItemCountChangedEventArgs> CurrentItemCountChangedEvent { add { } remove { } }
        public virtual event ModulePortFullChanged ModulePortFullChangedEvent { add { } remove { } }
        public event IsInitializedChanged IsInitializedChangedEvent { add { } remove { } }
        public PlatformModuleState OldState { get; set; }
        public string SubState { get; set; }
        public bool EnableStateLogging { get; set; }
        public bool EnableSubStateLogging { get; set; }
        public int ModuleNbr { get; set; }
        public virtual event ModuleStateChanged ModuleStateChangedEvent { add { } remove { } }
        public IList<IModuleEquipment> Equipments { get; private set; }
        public void AddEquipment(IModuleEquipment equipment)
        {
        }

        public virtual int AdsPort { get; set; }
        public string PlcAddress { get; set; }
        public virtual int CurrentItemCount { get; private set; }
        public int OverallItemCount { get; set; }
        public virtual int LimitItemCount { get; set; }
        public int PlannedThroughput { get; set; }
        public virtual int MaxCapacity { get; private set; }
        public virtual int ModuleTypeId { get; set; }
        public int StreamType { get; set; }
        public virtual string Name { get; set; }
        public virtual IAlarmManager AlarmManager { get; private set; }
        public virtual Task Initialize()
        {
            return Task.FromResult(0);
        }

        public Task Initialize(CancellationToken token)
        {
            return Task.FromResult(0);
        }

        public void Construct()
        {
        }

        public virtual void ActivateModule() { }
        public virtual void Disable() { }
        public virtual bool ContainsItem(long itemId) { return false; }
        public virtual void MoveItem(long itemId, IPlatformModule targetModule) { }
        public virtual void AddItem(PlatformItem item) { }
        public virtual void RemoveItem(long itemId) { }
        public virtual void Start() { }
        public void Standby()
        {
        }

        public virtual void Stop() { }
        public void StopReleaseItems(bool enable, int port = 0)
        {
        }

        public virtual void ResetAlarms() { }
        public virtual bool IsFull(int inputPortIndex) { return false; }
        public virtual PlatformItem GetItem(long itemId) { return null; }
        public virtual string PathRootController { get; set; }
        public bool IsInitialized { get; set; }
        public virtual PlatformModuleState State { get; set; }
        public virtual void AddItemRouting(PlatformItem item, int outputPortIndex) { }
        public virtual void RemoveItemRouting(PlatformItem item) { }
        public virtual void AddPortRouting(int portIndex) { }
        public virtual void RemovePortRouting(int portIndex) { }
        public HashSet<int> PortRoutings { get; private set; }
        public IDictionary<PlatformItem, int> ItemRoutings { get; private set; }

        public PlatformItem CreateNewPlatformItem()
        {
            return new PlatformItem();
        }
    }
}
