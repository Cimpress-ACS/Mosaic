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
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure;

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// Extension methods and utility functions related to a platform module.
    /// </summary>
    public static class PlatformModuleExtensions
    {
        /// <summary>
        /// Starts the InitializeAll method with the assumed standard values for all modules. This could also be written as a standard static method without
        /// making it an extension method of platform module, but it'd be hard to find a good name to describe the default retry timeout.
        /// </summary>
        /// <param name="module">The platform module where this is the extension method from.</param>
        /// <param name="token">A cancellation token wich is passed into <see cref="initAllFunc"/>, and which will also cancel the scheduling once it's canceled.</param>
        /// <param name="initAllFunc">
        /// A function that will be called with the provided cancellation token, first immediately (on the next free TaskPoolThread),
        /// and then every 5 seconds until the timer is disposed, or the cancellation token canceled.
        /// </param>
        /// <returns>A disposable that can be used to stop the re-occurring event.</returns>
        public static IDisposable StartInitializeAll(this IPlatformModule module, CancellationToken token, Func<CancellationToken, Task> initAllFunc)
        {
            var timer = TaskPoolScheduler.Default.ScheduleRecurringActionWithWait(TimeSpan.FromTicks(1), TimeSpan.FromSeconds(5),
                async () => await initAllFunc(token));
            token.Register(timer.Dispose);
            return timer;
        }
    }
}
