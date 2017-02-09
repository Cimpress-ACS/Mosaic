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
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.GuiEssentials.Wcf
{
    /// <summary>
    /// An implementer of <see cref="AbstractReinitializingClient{TService}"/> communicates with a service over a communication object.
    /// </summary>
    /// <typeparam name="TService">The type of the service to communicate with.</typeparam>
    public abstract class AbstractReinitializingClient<TService> : IClient<TService>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new <see cref="AbstractReinitializingClient{TService}"/> instance.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected AbstractReinitializingClient(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());
        }

        /// <summary>
        /// Invoke the specified <paramref name="actionToExecute"/> on the service.
        /// </summary>
        /// <param name="actionToExecute">The action to execute.</param>
        public async Task InvokeOnService(Func<TService, Task> actionToExecute)
        {
            try
            {
                TService serviceClient = GetCommunicationObject();
                await actionToExecute(serviceClient);
            }
            catch (Exception exception)
            {
                ResetCommunicationObject();
                _logger.Error("The connection to Mosaic faulted. It will get reinitialized on the next try.", exception);
                throw;
            }
        }

        /// <summary>
        /// Invoke the specified <paramref name="actionToExecute"/> on the service.
        /// </summary>
        /// <param name="actionToExecute">The action to execute.</param>
        /// <returns>The result from the service call.</returns>
        public async Task<TResult> InvokeOnService<TResult>(Func<TService, Task<TResult>> actionToExecute)
        {
            try
            {
                TService serviceClient = GetCommunicationObject();
                return await actionToExecute(serviceClient);
            }
            catch (Exception exception)
            {
                ResetCommunicationObject();
                _logger.Error("The connection to Mosaic faulted. It will get reinitialized on the next try.", exception);
                throw;
            }
        }

        /// <summary>
        /// An implementer should return a communciation object of the type <typeparamref name="TService"/>.
        /// </summary>
        protected abstract TService GetCommunicationObject();

        /// <summary>
        /// An implementer should reset or reininitialize the communication object.
        /// </summary>
        protected abstract void ResetCommunicationObject();
    }
}
