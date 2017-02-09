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
using System.ServiceModel;
using System.Threading.Tasks;
using VP.FF.PT.Common.GuiEssentials.Wcf;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.GuiEssentials
{
    /// <summary>
    /// Extensions for the module control screen, mostly related to WCF handling
    /// </summary>
    public static class ModuleControlScreenExtensions
    {
        /// <summary>
        /// Start a keep alive schedule with the default interval as provided in <see cref="WcfCommunicationHelper.KeepAliveInterval" />.
        /// </summary>
        /// <typeparam name="TFault">The WCF fault for the communication.</typeparam>
        /// <param name="screen">The screen to which this extension method is applied to.</param>
        /// <param name="keepAliveFunc">The function that should be executed as part of the keep alive ping, usually a WCF call.</param>
        /// <param name="logger">The logger to report errors to.</param>
        /// <returns>A handle that can be disposed to stop this keep-alive schedule.</returns>
        public static IDisposable KeepAlive<TFault>(this ModuleControlScreen screen, Func<Task> keepAliveFunc, ILogger logger)
        {
            return KeepAlive<TFault>(screen, keepAliveFunc, WcfCommunicationHelper.KeepAliveInterval, logger);
        }

        /// <summary>
        /// Start a keep alive schedule with a provided interval.
        /// </summary>
        /// <typeparam name="T">The WCF fault for the communication.</typeparam>
        /// <param name="screen">The screen to which this extension method is applied to.</param>
        /// <param name="keepAliveFunc">The function that should be executed as part of the keep alive ping, usually a WCF call.</param>
        /// <param name="interval">The interval to schedule the keep alive pings.</param>
        /// <param name="logger">The logger to report errors to.</param>
        /// <returns>A handle that can be disposed to stop this keep-alive schedule.</returns>
        public static IDisposable KeepAlive<T>(this ModuleControlScreen screen, Func<Task> keepAliveFunc, TimeSpan interval, ILogger logger)
        {
            return TaskPoolScheduler.Default.ScheduleRecurringActionWithWait(interval, () => KeepAliveExec<T>(screen, keepAliveFunc, logger));
        }

        private static async Task KeepAliveExec<TFault>(ModuleControlScreen screen, Func<Task> keepAliveFunc, ILogger logger)
        {
            if (!await WcfCommunicationHelper.KeepAliveExec<TFault>(keepAliveFunc, logger))
            {
                screen.DisableModule(new Exception("Keep alive ping failed."), logger);
            }
        }

        /// <summary>
        /// Starts a WCF connection.
        /// </summary>
        /// <typeparam name="TClient">The WCF client type to be used.</typeparam>
        /// <typeparam name="TFault">The WCF fault type that can be thrown by the client.</typeparam>
        /// <typeparam name="TChannel">The WCF channel.</typeparam>
        /// <param name="screen">The screen to which this extension method is valid for.</param>
        /// <param name="client">The WCF client that's currently exicsting and should be closed before a new one is created. Provide null if this doesn't exist.</param>
        /// <param name="createFunc">A function to create a WCF client.</param>
        /// <param name="logger">The logger to log errors.</param>
        /// <param name="postStartFunc">A method that is called after the client has been created to do custom initialization, such as starting a KeepAlive schedule.</param>
        /// <returns>A task that, when completed, returns the newly created WCF client.</returns>
        public static async Task<TClient> StartWcfClient<TClient, TFault, TChannel>(this ModuleControlScreen screen, TClient client,
            Func<InstanceContext, TClient> createFunc, ILogger logger, Func<TClient, Task> postStartFunc)
            where TChannel : class
            where TClient : ClientBase<TChannel>
        {
            return await WcfCommunicationHelper.StartWcfClient<TClient, TFault, TChannel>(screen, client, createFunc, postStartFunc);
        }
    }
}
