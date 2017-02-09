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
using VP.FF.PT.Common.Infrastructure.Assembling;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.LogInformation
{
    /// <summary>
    /// The <see cref="LogMessageDtoAssembler"/> is capable of assembling
    /// <see cref="LogMessage"/> instances to <see cref="LogMessageDto"/> instances.
    /// </summary>
    [Export(typeof(IAssemble<LogMessage, LogMessageDto>))]
    public class LogMessageDtoAssembler : IAssemble<LogMessage, LogMessageDto>
    {
        /// <summary>
        /// Assembles a new <see cref="LogMessageDto"/> instance out of the specified <paramref name="fromItem"/>.
        /// </summary>
        /// <param name="fromItem">The original object.</param>
        /// <param name="assembleParameters">Optional parameters the assemble method might consider when executed.</param>
        /// <returns>The resulting object.</returns>
        public LogMessageDto Assemble(LogMessage fromItem, dynamic assembleParameters = null)
        {
            if (fromItem == null)
                return null;
            return new LogMessageDto { Emitter = fromItem.Emitter, Exception = fromItem.Exception, Text = fromItem.Text };
        }
    }
}
