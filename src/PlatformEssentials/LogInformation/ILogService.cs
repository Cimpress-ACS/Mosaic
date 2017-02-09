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


using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace VP.FF.PT.Common.PlatformEssentials.LogInformation
{
    /// <summary>
    /// The implementer of <see cref="ILogService"/> provides service methods for gathering log messages from Mosaic.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ILogServiceCallback))]
    public interface ILogService
    {
        /// <summary>
        /// Gets all log messages emitted by the specified <paramref name="emitters"/>.
        /// </summary>
        /// <param name="emitters">The emitters.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="LogMessageDto"/> instances.</returns>
        [OperationContract]
        List<LogMessageDto> GetLogMessagesFromEmitters(List<string> emitters);

        /// <summary>
        /// Subscribes the calling <see cref="ILogServiceCallback"/> for notifications when
        /// one of the specified <paramref name="emitters"/> emits a log message.
        /// </summary>
        /// <param name="emitters">The emitter to observe.</param>
        void SubscribeForEmitters(List<string> emitters);

        /// <summary>
        /// Unsubscribes the calling <see cref="ILogServiceCallback"/> from all notifications.
        /// </summary>
        void UnsubscribeFromAllEmitters();
    }

    [DataContract]
    public class LogServiceFault
    {
        public LogServiceFault()
        {
        }

        public LogServiceFault(string reason)
        {
            Reason = reason;
        }

        [DataMember]
        public string Reason { get; set; }
    }
}
