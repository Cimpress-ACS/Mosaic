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
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.Infrastructure.Wcf;

namespace VP.FF.PT.Common.GuiEssentials.Wcf
{
    /// <summary>
    /// Helper class around WCF communication functionality.
    /// </summary>
    /// <remarks>
    /// In case you're needing generic functionality for WCF from a module screen, consider using the module screen's extensions.
    /// </remarks>
    public static class WcfCommunicationHelper
    {
        /// <summary>
        /// The default keep alive interval.
        /// </summary>
        public static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(50);

        /// <summary>
        /// Start a keep alive schedule with the default interval as provided in <see cref="KeepAliveInterval"/>.
        /// </summary>
        /// <typeparam name="TFault">The WCF fault for the communication.</typeparam>
        /// <param name="keepAliveFunc">The function that should be executed as part of the keep alive ping, usually a WCF call.</param>
        /// <param name="logger">The logger to report errors to.</param>
        /// <returns>A handle that can be disposed to stop this keep-alive schedule.</returns>
        public static IDisposable KeepAlive<TFault>(Func<Task> keepAliveFunc, ILogger logger)
        {
            return KeepAlive<TFault>(keepAliveFunc, KeepAliveInterval, logger);
        }

        /// <summary>
        /// Start a keep alive schedule with a provided interval.
        /// </summary>
        /// <typeparam name="TFault">The WCF fault for the communication.</typeparam>
        /// <param name="keepAliveFunc">The function that should be executed as part of the keep alive ping, usually a WCF call.</param>
        /// <param name="interval">The interval to be used for scheduling the keep-alive pings.</param>
        /// <param name="logger">The logger to report errors to.</param>
        /// <returns>A handle that can be disposed to stop this keep-alive schedule.</returns>
        public static IDisposable KeepAlive<TFault>(Func<Task> keepAliveFunc, TimeSpan interval, ILogger logger)
        {
            return TaskPoolScheduler.Default.ScheduleRecurringActionWithWait(interval, () => KeepAliveExec<TFault>(keepAliveFunc, logger));
        }

        /// <summary>
        /// The function that's calling the keep-alive ping, wrapped with additional exception handling.
        /// </summary>
        /// <typeparam name="TFault">The WCF fault that can be thrown.</typeparam>
        /// <param name="keepAliveFunc">The keep-alive function to call.</param>
        /// <param name="logger">The logger to report errors.</param>
        /// <returns>A task, when completed, returns true if the keep-alive function executed correctly, or false if it resulted in errors.</returns>
        public static async Task<bool> KeepAliveExec<TFault>(Func<Task> keepAliveFunc, ILogger logger)
        {
            try
            {
                await keepAliveFunc();
            }
            catch (FaultException<TFault> ex)
            {
                logger.Error("Fault at keep alive.", ex);
                return false;
            }
            catch (CommunicationException ex)
            {
                logger.Error("Communication exception at keep alive.", ex);
                return false;
            }
            catch (TimeoutException ex)
            {
                logger.Error("Timeout at keep alive.", ex);
                return false;
            }

            return true;
        }

        private static readonly ConcurrentDictionary<object, IDisposable> _faultedEvents = new ConcurrentDictionary<object, IDisposable>();
        private static readonly ConcurrentDictionary<object, IDisposable> _keepAlives = new ConcurrentDictionary<object, IDisposable>();

        /// <summary>
        /// Starts a WCF connection.
        /// </summary>
        /// <typeparam name="TClient">The WCF client type to be used.</typeparam>
        /// <typeparam name="TFault">The WCF fault type that can be thrown by the client.</typeparam>
        /// <typeparam name="TChannel">The WCF channel.</typeparam>
        /// <param name="context">The context that should be used for the WCF client.</param>
        /// <param name="client">The WCF client that's currently exicsting and should be closed before a new one is created. Provide null if this doesn't exist.</param>
        /// <param name="createFunc">A function to create a WCF client.</param>
        /// <param name="postStartFunc">A method that is called after the client has been created to do custom initialization, such as starting a KeepAlive schedule.</param>
        /// <returns>A task that, when completed, returns the newly created WCF client.</returns>
        public static async Task<TClient> StartWcfClient<TClient, TFault, TChannel>(object context, TClient client,
            Func<InstanceContext, TClient> createFunc, Func<TClient, Task> postStartFunc)
            where TChannel : class
            where TClient : ClientBase<TChannel>
        {
            // create client
            var eventContext = new InstanceContext(context);
            var wcfClient = createFunc(eventContext);
            wcfClient.ChannelFactory.Endpoint.Behaviors.Add(new EndpointCallTimerBehavior());

            // transform faulted event to observable stream
            var faultedEvents = Observable.FromEventPattern(ev => ((ICommunicationObject)wcfClient).Faulted += ev,
                ev => ((ICommunicationObject)wcfClient).Faulted -= ev);

            // handle observable events of type communication faulted
            var events = faultedEvents.ObserveOn(TaskPoolScheduler.Default).Subscribe(async x =>
            {
                StopWcfClient(client);
                await StartWcfClient<TClient, TFault, TChannel>(context, default(TClient), createFunc, postStartFunc);
            });
            _faultedEvents.AddOrUpdate(wcfClient, o => events, (o, e) => events);

            await postStartFunc(wcfClient);

            return wcfClient;
        }

        /// <summary>
        /// Stops a WCF client.
        /// </summary>
        /// <param name="client">The client to stop.</param>
        public static void StopWcfClient(ICommunicationObject client)
        {
            if (client != null)
            {
                try
                {
                    client.Close();
                }
                catch (CommunicationException)
                {
                    // ignore, it's expected to be in a faulted state in case the connection
                    // isn't there anymore; theoretically we could only listen on a CommunicationObjectFaultedException,
                    // but there might be other communication exceptions that could occur during closing the client
                }
                IDisposable events;
                if (_faultedEvents.TryRemove(client, out events))
                {
                    events.Dispose();
                }
                IDisposable keepAlive;
                if (_keepAlives.TryRemove(client, out keepAlive))
                {
                    keepAlive.Dispose();
                }
            }
        }

        /// <summary>
        /// Initiates a WCF call in a safe way, including exception handling, and a callback function to reset the exception in case of errors.
        /// </summary>
        /// <typeparam name="TFault">The WCF fault that the client might throw.</typeparam>
        /// <param name="wcfCall">The wcf call that should be executed.</param>
        /// <param name="resetConnection">A function that's called in case of errors and allows to re-initiate the WCF connection.</param>
        /// <param name="logger">The logger for error reporting.</param>
        /// <returns>A task that, when completed, called the WCF function and completed resetting the connection if required.</returns>
        public static async Task WcfCall<TFault>(Func<Task> wcfCall, Func<Task> resetConnection, ILogger logger)
        {
            try
            {
                await wcfCall();
                return;
            }
            catch (FaultException<TFault> e)
            {
                logger.Error("Fault exception during WCF call.", e);
            }
            catch (Exception e)
            {
                logger.Error("Unexpected exception during WCF call.", e);
            }
            await resetConnection();
        }
    }
}
