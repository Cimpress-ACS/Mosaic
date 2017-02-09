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
using System.Linq;
using System.ServiceModel;
using VP.FF.PT.Common.Infrastructure.Assembling;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.LogInformation
{
    /// <summary>
    /// The <see cref="LogService"/> is the default implementation of the 
    /// <see cref="ILogService"/> WCF service.
    /// </summary>
    [Export(typeof(ILogService))]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LogService : ILogService
    {
        private readonly IProvideLogMessages _provideLogMessages;
        private readonly NullFilteringEnumerableAssembler<LogMessage, LogMessageDto> _logMessagesAssembler;
        private readonly ILogger _logger;

        /// <summary>
        /// Initialized a new <see cref="LogService"/> instance.
        /// </summary>
        public LogService()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="LogService"/> instance.
        /// </summary>
        /// <param name="provideLogMessages">The instance to get the log messages from.</param>
        /// <param name="logMessagesAssembler">The assembler to create the dtos from the log messages.</param>
        /// <param name="logger">The logger.</param>
        [ImportingConstructor]
        public LogService(
            IProvideLogMessages provideLogMessages,
            NullFilteringEnumerableAssembler<LogMessage, LogMessageDto> logMessagesAssembler,
            ILogger logger)
        {
            _provideLogMessages = provideLogMessages;
            _logMessagesAssembler = logMessagesAssembler;
            _logger = logger;
            _logger.Init(GetType());
        }

        /// <summary>
        /// Gets all log messages emitted by the specified <paramref name="emitters"/>.
        /// </summary>
        /// <param name="emitters">The emitters.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LogMessageDto"/> instances.</returns>
        public List<LogMessageDto> GetLogMessagesFromEmitters(List<string> emitters)
        {
            try
            {
                IEnumerable<LogMessage> messages = _provideLogMessages.GetMessages(emitters);
                List<LogMessageDto> dtos = _logMessagesAssembler.Assemble(messages).ToList();
                return dtos;
            }
            catch (Exception exception)
            {
                throw new FaultException<LogServiceFault>(new LogServiceFault(exception.Message));
            }
        }

        public void SubscribeForEmitters(List<string> emitters)
        {
        }

        public void UnsubscribeFromAllEmitters()
        {
        }
    }
}
