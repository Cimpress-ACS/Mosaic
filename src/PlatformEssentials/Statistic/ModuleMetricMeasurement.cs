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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.Statistic
{
    [Export(typeof(IModuleMetricMeasurement))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModuleMetricMeasurement : IModuleMetricMeasurement
    {
        private readonly object _lock = new object();
        private IPlatformModule _module;
        private TimeSpan _refreshRate;

        private int _oldItemCount;

        private readonly AutoResetEvent _timerSignalEvent = new AutoResetEvent(false);
        private Timer _refreshTimer;
        private readonly IList<DateTime> _itemAddedHistory = new List<DateTime>();

        private readonly Stopwatch _upTimeWatch = new Stopwatch();
        private readonly Stopwatch _downTimeWatch = new Stopwatch();

        public event EventHandler<ModuleMetricUpdateEventArgs> MetricsUpdatedEvent;

        [ImportingConstructor]
        public ModuleMetricMeasurement()
        {
            MetricsUpdatedEvent += (s, a) => { };
            RefreshRate = TimeSpan.FromSeconds(10);
            FloatingTimeWindow = TimeSpan.FromHours(1);
        }

        public void Initialize(IPlatformModule module)
        {
            _module = module;
            _module.CurrentItemCountChangedEvent += OnCurrentItemCountChanged;
            _module.ModuleStateChangedEvent += OnStateChanged;
            _oldItemCount = _module.CurrentItemCount;

            CheckUpDownTime();

            _refreshTimer = new Timer(OnRefreshTimeout, _timerSignalEvent, RefreshRate, RefreshRate);
        }

        public void Reset()
        {
            lock (_lock)
            {
                _itemAddedHistory.Clear();
            }
        }

        public void Dispose()
        {
            _refreshTimer.Change(10, int.MaxValue);
            _timerSignalEvent.WaitOne();
            _refreshTimer.Dispose();
        }

        public TimeSpan RefreshRate
        {
            get { return _refreshRate; }
            set
            {
                _refreshRate = value;
                DelayRefreshTimer();
            }
        }

        public TimeSpan FloatingTimeWindow { get; set; }

        public double Availability
        {
            get
            {
                return 1;
            }
        }

        public double Performance
        {
            get
            {
                if (_module.PlannedThroughput == 0)
                    return 1;

                return Math.Min(_itemAddedHistory.Count / (double)_module.PlannedThroughput, 1);
            }
        }

        public double Quality
        {
            get
            {
                return 1;
            }
        }

        public double OverallEquipmentEfficiency
        {
            get
            {
                return Availability * Performance * Quality;
            }
        }

        public TimeSpan UpTime
        {
            get { return _upTimeWatch.Elapsed; }
        }

        public TimeSpan DownTime
        {
            get { return _downTimeWatch.Elapsed; }
        }

        private void OnCurrentItemCountChanged(object sender, ItemCountChangedEventArgs e)
        {
            lock (_lock)
            {
                DelayRefreshTimer();
                CleanHistory();

                int delta = e.Count - _oldItemCount;

                if (delta <= 0)
                    return;

                for (int i = 0; i < delta; i++)
                    _itemAddedHistory.Add(DateTime.Now);

                MetricsUpdatedEvent(this, new ModuleMetricUpdateEventArgs(Availability, Performance, Quality, OverallEquipmentEfficiency));
            }
        }

        private void OnStateChanged(IPlatformModule sender, PlatformModuleState newState)
        {
            lock (_lock)
            {
                DelayRefreshTimer();

                CheckUpDownTime();

                CleanHistory();

                MetricsUpdatedEvent(this, new ModuleMetricUpdateEventArgs(Availability, Performance, Quality, OverallEquipmentEfficiency));
            }
        }

        private void OnRefreshTimeout(object state)
        {
            lock (_lock)
            {
                AutoResetEvent autoEvent = (AutoResetEvent)state;
                autoEvent.Reset();

                CleanHistory();

                MetricsUpdatedEvent(this, new ModuleMetricUpdateEventArgs(Availability, Performance, Quality, OverallEquipmentEfficiency));

                autoEvent.Set();
            }
        }

        private void DelayRefreshTimer()
        {
            if (_refreshTimer != null)
                _refreshTimer.Change(RefreshRate, RefreshRate);
        }

        // remove old events to have a floating time window
        private void CleanHistory()
        {
            DateTime currentTimestamp = DateTime.Now;

            while(!_itemAddedHistory.IsNullOrEmpty() && currentTimestamp - _itemAddedHistory.First() > FloatingTimeWindow)
                _itemAddedHistory.RemoveAt(0);
        }

        private void CheckUpDownTime()
        {
            switch (_module.State)
            {
                case PlatformModuleState.Run:
                case PlatformModuleState.Off:
                case PlatformModuleState.Standby:
                    _downTimeWatch.Stop();
                    _upTimeWatch.Start();
                    break;
                default:
                    _upTimeWatch.Stop();
                    _downTimeWatch.Start();
                    break;
            }
        }
    }
}
