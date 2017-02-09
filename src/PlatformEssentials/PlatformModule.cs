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
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.AlarmManagement;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;
using VP.FF.PT.Common.PlatformEssentials.Statistic;

namespace VP.FF.PT.Common.PlatformEssentials
{
    [Serializable]
    [PartNotDiscoverable]
    public class PlatformModule : IPlatformModule, IEquatable<IPlatformModule>
    {
        public event EventHandler<ItemCountChangedEventArgs> CurrentItemCountChangedEvent;
        public event ModuleStateChanged ModuleStateChangedEvent;
        public event ModulePortFullChanged ModulePortFullChangedEvent;
        public event IsInitializedChanged IsInitializedChangedEvent;

        internal protected Action<PlatformModuleDTO> ModuleStateChangedCallback = delegate { };
        internal protected Action<MetricsDTO> MetricsChangedCallback = delegate { };
        internal protected Action<string, string> DatabaseAccessFailedCallback = delegate { };

        // MessageType, Message, Duration
        internal protected Action<MessageType, string, TimeSpan> SendUserNotificationCallback = delegate { };

        [Import]
        internal protected IEntityContextFactory EntityContextFactory { get; internal set; }

        /// <summary>
        /// Platform Module Entity manager. Manages the entities read from the database as well as the 
        /// ones created at runtime.
        /// </summary>
        [Import]
        internal protected IPlatformModuleEntities PlatformModuleEntities { get; internal set; }

        [Import]
        internal protected ILogger Logger { get; internal set; }

        [Import]
        internal protected ISafeEventRaiser EventRaiser { get; internal set; }

        [Import]
        internal protected IEventAggregator EventAggregator { get; internal set; }

        [Import]
        internal protected CompositeAlarmManager CompositeAlarmManager { get; internal set; }

        [Import]
        internal protected IModuleMetricMeasurement Metrics { get; internal set; }

        /// <summary>
        /// Constructor will be called after real constructor and before Initialize.
        /// </summary>
        public PlatformModule()
        {
            OldState = PlatformModuleState.Undefined;

            Equipments = new List<IModuleEquipment>();
            PortRoutings = new HashSet<int>();
            ItemRoutings = new Dictionary<PlatformItem, int>();
            Entities = new PlatformModuleEntity();
        }

        public virtual void Construct()
        {
            Logger.Init(Name);

            CurrentItemCountChangedEvent += (s, e) => { };
            ModuleStateChangedEvent += (s, e) => { };
            ModulePortFullChangedEvent += (s, i, f) => { };
            IsInitializedChangedEvent += (s) => { };
            CompositeAlarmManager.ModuleName = Name;

            State = PlatformModuleState.NotInitialized;
        }

        public IAlarmManager AlarmManager { get { return CompositeAlarmManager; } }

        public IList<IModuleEquipment> Equipments { get; private set; }

        private PlatformModuleState _state;

        public virtual PlatformModuleState State
        {
            get { return _state; }
            set
            {
                if (value != _state)
                {
                    OldState = _state;
                    _state = value;
                    RaiseModuleStateChangedEvent();
                }
            }
        }

        public PlatformModuleState OldState { get; set; }

        public virtual string SubState { get; set; }
        public virtual bool EnableStateLogging { get; set; }
        public virtual int ModuleNbr { get; set; }
        public virtual bool EnableSubStateLogging { get; set; }
        public virtual string PlcAddress { get; set; }

        public int StreamType { get; set; }
        public string Name { get; set; }
        public int ModuleTypeId { get; set; }
        public bool IsEnabled { get; set; }

        public void AddEquipment(IModuleEquipment equipment)
        {
            equipment.AssociatedModule = this;
            Equipments.Add(equipment);
        }

        public int AdsPort { get; set; }
        public string PathRootController { get; set; }
        public int OverallItemCount { get; set; }
        public int MaxCapacity { get; set; }
        public PlatformModuleEntity Entities { get; private set; }

        private bool _isInitialized;

        public virtual bool IsInitialized
        {
            get { return _isInitialized; }
            set
            {
                if (value != _isInitialized)
                {
                    _isInitialized = value;
                    IsInitializedChangedEvent(this);
                }
            }
        }

        public int LimitItemCount { get; set; }

        public int PlannedThroughput { get; set; }

        /// <summary>
        /// Gets the current routings for "all items to port-x".
        /// </summary>
        public HashSet<int> PortRoutings { get; private set; }

        /// <summary>
        /// Gets the current item routings.
        /// </summary>
        public IDictionary<PlatformItem, int> ItemRoutings { get; private set; }

        /// <summary>
        /// Module Initialization
        /// </summary>
        /// <remarks>
        /// BeforeItemDetectedEvent and ItemDetectedEvent are used in order to detect changes that are performed 
        /// on the PlatformItem in order for Entity Framework to store them. It is quite problematic to set the relationship into
        /// added state and that, although a little bit hacky seemed like the best way to record the changes.
        /// One other way, could be a shift torwards foreign key references, but that would require more significant changes in the diagram
        /// that I didn't want to perform.
        /// </remarks>
        public virtual async Task Initialize()
        {
            await Initialize(CancellationToken.None);
        }

        public virtual Task Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            CompositeAlarmManager.AddPlugin(new InMemoryManageCurrentAlarmsPlugin());
            LoadEntitiesFromDatabase();
            return Task.FromResult(true);
        }

        public virtual void ActivateModule()
        {
            Metrics.MetricsUpdatedEvent += (s, metric) => RaiseMetricsChangedEvent();
            Metrics.Initialize(this);
        }

        private void LoadEntitiesFromDatabase()
        {
            var loadedEntities = PlatformModuleEntities.GetPlatformModuleEntity(new PlatformModuleEntity { Name = Name });

            if (loadedEntities != null)
                Entities = loadedEntities;
        }

        public virtual void Disable()
        {
            State = PlatformModuleState.Disabled;
        }

        public virtual int CurrentItemCount
        {
            get
            {
                return Entities.PlatformItems.Count;
            }
        }

        public virtual bool IsFull(int inputPortIndex)
        {
            return false;
        }

        public bool ContainsItem(long itemId)
        {
            return GetItem(itemId) != null;
        }

        public PlatformItem GetItem(long itemId)
        {
            return Entities.PlatformItems.FirstOrDefault(x => x.ItemId == itemId);
        }

        public virtual void MoveItem(long itemId, IPlatformModule targetModule)
        {
            MoveItem(GetItem(itemId), targetModule);
        }

        protected virtual void MoveItem(PlatformItem item, IPlatformModule targetModule)
        {
            if (item == null)
            {
                return;
            }

            var c = EntityContextFactory.CreateContext();
            c.Attach(Entities);

            ReleaseLinks(item);
            c.SaveEntityStateAsyncAndDispose();

            var context = EntityContextFactory.CreateContext();
            context.Attach(((PlatformModule)targetModule).Entities);
            context.Attach(item);
            item.AssociatedPlatformModuleEntity = ((PlatformModule)targetModule).Entities;
            context.SaveEntityStateAsyncAndDispose();
            Entities.PlatformItems.Remove(item);
            targetModule.AddItem(item);

            ItemRoutings.Remove(item);

            CurrentItemCountChangedEvent(this, new ItemCountChangedEventArgs(CurrentItemCount));
        }

        /// <summary>
        /// Adds item to the module and schedules a persistance action to store the data in the db.
        /// The contract is that each item with Id = 0 is a new item to be created unless there is an item with the 
        /// same ItemId already read from the database. In that case the database object is used. Otherwise 
        /// a new platfrom item is created in the database.
        /// </summary>
        /// <param name="item"></param>
        public virtual void AddItem(PlatformItem item)
        {
            if (item == null)
            {
                return;
            }

            var context = EntityContextFactory.CreateContext();

            context.Attach(Entities);
            if (item.Id == 0)
            {
                var previouslyAddedItem = PlatformModuleEntities.GetAll().SelectMany(a => a.PlatformItems).FirstOrDefault(b => b.ItemId == item.ItemId);
                if (previouslyAddedItem != null)
                {
                    context.Detach(previouslyAddedItem);
                    Entities.PlatformItems.Remove(previouslyAddedItem);
                    item.Id = item.ItemId;
                    context.Update(item);
                }
                else
                {
                    item.Id = item.ItemId;
                    context.Add(item);
                }
            }
            else
            {
                context.Attach(item);
            }

            item.AssociatedPlatformModuleEntity = Entities;
            context.SaveEntityStateAsyncAndDispose();
            Entities.PlatformItems.Add(item);

            OverallItemCount++;
            CurrentItemCountChangedEvent(this, new ItemCountChangedEventArgs(CurrentItemCount));
        }

        public virtual void RemoveItem(long itemId)
        {
            RemoveItem(GetItem(itemId));
        }

        protected virtual void RemoveItem(PlatformItem item)
        {
            if (item == null)
            {
                return;
            }

            Entities.PlatformItems.Remove(item);

            var context = EntityContextFactory.CreateContext();
            context.Delete(item);
            context.SaveEntityStateAsyncAndDispose();

            ItemRoutings.Remove(item);

            CurrentItemCountChangedEvent(this, new ItemCountChangedEventArgs(CurrentItemCount));
        }

        public virtual void AddItemRouting(PlatformItem item, int outputPortIndex)
        {
            if (!ItemRoutings.ContainsKey(item))
                ItemRoutings.Add(item, outputPortIndex);
        }

        public virtual void RemoveItemRouting(PlatformItem item)
        {
            if (item == null)
            {
                Logger.Warn("RemoveItemRouting: item is null");
            }
            else if (ItemRoutings.ContainsKey(item))
            {
                ItemRoutings.Remove(item);
            }
        }

        public virtual void AddPortRouting(int portIndex)
        {
            if (PortRoutings.Add(portIndex))
            {
                Logger.Debug("Added port-routing for output port " + portIndex);
            }
        }

        public virtual void RemovePortRouting(int portIndex)
        {
            if (PortRoutings.Remove(portIndex))
            {
                Logger.Debug("Removed port-routing from output port " + portIndex);
            }
        }

        public virtual PlatformItem CreateNewPlatformItem()
        {
            return new PlatformItem
            {
                LastDetectionTime = DateTime.Now,
            };
        }

        public virtual void Start()
        {
        }

        public virtual void Standby()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void StopReleaseItems(bool enable, int port = 0)
        {
        }

        public virtual void ResetAlarms()
        {
            AlarmManager.AcknowledgeAlarms();
        }

        public bool KeepAlive()
        {
            return true;
        }

        public bool Equals(IPlatformModule other)
        {
            return other != null && string.Equals(Name, other.Name);
        }

        private PlatformModuleState _oldModuleState = PlatformModuleState.Undefined;

        internal protected void RaiseModuleStateChangedEvent()
        {
            // callbacks will be raised in any case even if the state hasn't changed. to update WCF clients.
            if (EventRaiser != null)
            {
                EventRaiser.Raise(ref ModuleStateChangedCallback, this.ToDTO());
            }

            if (State != _oldModuleState)
            {
                var handle = ModuleStateChangedEvent;
                if (handle != null)
                {
                    handle(this, State);
                }
                _oldModuleState = State;
            }
        }

        /// <summary>
        /// Raises the metrics changed event.
        /// </summary>
        /// <param name="oee">A value between 0 and 1 indicating the speedometer (for whatever metric).</param>
        protected internal void RaiseMetricsChangedEvent()
        {
            // callbacks will be raised in any case even if the state hasn't changed. to update WCF clients.
            if (EventRaiser != null)
            {
                var metrics = new MetricsDTO(
                    OverallItemCount, 
                    Metrics.UpTime, 
                    Metrics.DownTime, 
                    Metrics.OverallEquipmentEfficiency);

                EventRaiser.Raise(ref MetricsChangedCallback, metrics);
            }
        }

        protected void RaisePortFullChangedEvent(int inputPortIndex, bool isFull)
        {
            ModulePortFullChangedEvent(this, inputPortIndex, isFull);
        }

        /// <summary>
        /// Raises an event to notify LineControl that an item was discovered (independently whether it's a new or old item).
        /// </summary>
        protected void RaisePlatformItemDetected(long itemId)
        {
            var item = GetItem(itemId);
            if (item != null)
            {
                item.LastDetectionTime = DateTime.Now;
                item.DetectedInModuleCount++;
                item.DetectedCount++;

                using (var context = EntityContextFactory.CreateContext())
                {
                    context.UpdateField(item, a => a.DetectedCount);
                    context.UpdateField(item, a => a.DetectedInModuleCount);
                    context.UpdateField(item, a => a.LastDetectionTime);
                }
            }

            EventAggregator.Publish(new PlatformItemEvent(itemId, this, PlatformItemEventType.ItemDetected));
        }

        /// <summary>
        /// Raises an event to notify LineControl the item left the module.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="outputPort">The output port index describes exactly where the item left the module.</param>
        protected void RaisePlatformItemReleased(long itemId, int outputPort)
        {
            var item = GetItem(itemId);
            if (item != null)
            {
                item.DetectedInModuleCount = 0;
            }

            EventAggregator.Publish(new PlatformItemEvent(itemId, this, PlatformItemEventType.ItemLeft, outputPort));
        }

        private static void ReleaseLinks(PlatformItem item)
        {
            if (item.ItemBehind != null)
            {
                item.ItemBehind.ItemInFront = null;
                item.ItemBehind = null;
            }

            if (item.ItemInFront != null)
            {
                item.ItemInFront.ItemBehind = null;
                item.ItemInFront = null;
            }
        }
    }
}
