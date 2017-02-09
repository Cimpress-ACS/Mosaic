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
using System.ComponentModel.Composition;
using System.Reactive.Concurrency;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Simulation
{
    [Export(typeof(ITaktManager))]
	[PartCreationPolicy(CreationPolicy.Shared)]
    public class TaktManager : ITaktManager
    {
        private readonly TimeSpan _defaultTaktDelay = TimeSpan.FromMilliseconds(100);
        private readonly ILogger _logger;
        private readonly ITaktPartsRepository _taktPartsRepository;
        private IDisposable _timer;

        [ImportingConstructor]
        public TaktManager(ILogger logger, ITaktPartsRepository taktPartsRepository)
        {
            _logger = logger;
            _taktPartsRepository = taktPartsRepository;

            DelayPerTakt = _defaultTaktDelay;

            _logger.Init(GetType());
        }

        public TimeSpan DelayPerTakt { get; set; }

        public void Start()
        {
            Stop();
            _timer = TaskPoolScheduler.Default.SchedulePeriodic(DelayPerTakt, Takt);
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void SingleStep()
        {
            foreach (var taktPart in _taktPartsRepository.GetTaktParts())
            {
                taktPart.Takt();
            }

            TaktCount++;
        }

        public ulong TaktCount { get; private set; }

        private void Takt()
        {
            try
            {
                SingleStep();
            }
            catch (AggregateException ex)
            {
                _logger.Error("Error during simulation.", ex);
            }
        }
    }
}
