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


using System.ComponentModel.Composition;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.GuiEssentials.Wcf
{
    /// <summary>
    /// The <see cref="DuplexClientFactory{TService,TServiceCallback}"/> is capable of creating a 
    /// new <see cref="DuplexClient{TService,TServiceCallback}"/> instance.
    /// </summary>
    /// <typeparam name="TService">The type of the service the created client can communicate with.</typeparam>
    /// <typeparam name="TServiceCallback">The type of the callback the service can communicate with.</typeparam>
    [Export(typeof(IDuplexClientFactory<,>))]
    public class DuplexClientFactory<TService, TServiceCallback> : IDuplexClientFactory<TService, TServiceCallback>
    {
        private readonly IDuplexCommunicationObjectFactory<TService, TServiceCallback> _communicationObjectFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new <see cref="DuplexClientFactory{TService,TServiceCallback}"/> instance.
        /// </summary>
        /// <param name="communicationObjectFactory">The factory used to create the duplex communication object.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public DuplexClientFactory(
            IDuplexCommunicationObjectFactory<TService, TServiceCallback> communicationObjectFactory, 
            ILogger logger)
        {
            _communicationObjectFactory = communicationObjectFactory;
            _logger = logger;
        }

        /// <summary>
        /// Creates a <see cref="IClient{TService}"/> implementer using duplex communication.
        /// </summary>
        /// <param name="callbackInstance">The instance of the communication endpoint for the callbacks.</param>
        /// <returns>A <see cref="IClient{TService}"/> instance.</returns>
        public IClient<TService> CreateClient(TServiceCallback callbackInstance)
        {
            return new DuplexClient<TService, TServiceCallback>(
                _communicationObjectFactory, 
                _logger,
                callbackInstance);
        }
    }
}
