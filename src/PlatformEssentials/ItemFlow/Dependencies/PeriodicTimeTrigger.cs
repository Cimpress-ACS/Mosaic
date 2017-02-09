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
using System.Threading;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies
{
    public class PeriodicTimeTrigger : ITrigger, IDisposable
    {
        private readonly TimeSpan _period;
        public event EventHandler<EventArgs> TriggerOccurred;

        private readonly Timer _timer;

        public PeriodicTimeTrigger(TimeSpan period)
        {
            _period = period;
            _timer = new Timer(OnTimeout, null, (int)period.TotalMilliseconds, Timeout.Infinite);
        }

        private void OnTimeout(object state)
        {
            var handler = TriggerOccurred;
            if (handler != null)
            {
                TriggerOccurred(this, new EventArgs());
            }

            _timer.Change((int) _period.TotalMilliseconds, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
