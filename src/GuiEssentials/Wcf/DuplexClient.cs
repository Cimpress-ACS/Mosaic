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


using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.GuiEssentials.Wcf
{
    /// <summary>
    /// The Duplex client communicates with a service over a duplexable communication object.
    /// </summary>
    /// <typeparam name="TService">The type of the service to communicate with.</typeparam>
    /// <typeparam name="TServiceCallback">The type of the callback interface for the duplex communication.</typeparam>
    public class DuplexClient<TService, TServiceCallback> : AbstractReinitializingClient<TService>
    {
        private readonly object _lockObject = new object();

        private readonly IDuplexCommunicationObjectFactory<TService, TServiceCallback> _communicationObjectFactory;
        private readonly TServiceCallback _serviceCallbackInstance;
        private TService _communicationObject;

        /// <summary>
        /// Initializes a new <see cref="DuplexClient{TService,TServiceCallback}"/> instance.
        /// </summary>
        /// <param name="communicationObjectFactory">The factory used to create a duplexable communication object.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="serviceCallbackInstance">An instance capable of handling the duplex communication.</param>
        public DuplexClient(
            IDuplexCommunicationObjectFactory<TService, TServiceCallback> communicationObjectFactory, 
            ILogger logger, 
            TServiceCallback serviceCallbackInstance = default(TServiceCallback))
            : base(logger)
        {
            _communicationObjectFactory = communicationObjectFactory;
            _serviceCallbackInstance = serviceCallbackInstance;
        }

        /// <summary>
        /// An implementer should return a communciation object of the type <typeparamref name="TService"/>.
        /// </summary>
        protected override TService GetCommunicationObject()
        {
            lock (_lockObject)
            {
                if (Equals(_communicationObject, default(TService)))
                    _communicationObject = _communicationObjectFactory.CreateCommunicationObject(_serviceCallbackInstance);
                return _communicationObject;
            }
        }

        /// <summary>
        /// An implementer should reset or reininitialize the communication object.
        /// </summary>
        protected override void ResetCommunicationObject()
        {
            _communicationObject = default(TService);
        }
    }
}
