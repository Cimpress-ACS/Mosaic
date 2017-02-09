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
    /// The default client communicates with a service over a communication object.
    /// </summary>
    /// <typeparam name="TService">The type of the service to communicate with.</typeparam>
    public class DefaultClient<TService> : AbstractReinitializingClient<TService>
    {
        private readonly object _lockObject = new object();

        private readonly ICommunicationObjectFactory<TService> _communicationObjectFactory;
        private TService _communicationObject;

        /// <summary>
        /// Initializes a new <see cref="DefaultClient{TService}"/> instance.
        /// </summary>
        /// <param name="communicationObjectFactory">The factory used to create a communication object.</param>
        /// <param name="logger">The logger.</param>
        public DefaultClient(ICommunicationObjectFactory<TService> communicationObjectFactory, ILogger logger)
            : base(logger)
        {
            _communicationObjectFactory = communicationObjectFactory;
        }

        protected override TService GetCommunicationObject()
        {
            lock (_lockObject)
            {
                if (Equals(_communicationObject, default(TService)))
                    _communicationObject = _communicationObjectFactory.CreateCommunicationObject();
                return _communicationObject;
            }
        }

        protected override void ResetCommunicationObject()
        {
            _communicationObject = default(TService);
        }
    }
}
