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


using System.Threading.Tasks;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials
{
    /// <summary>
    /// Interface for actions or checks that need to be performed before we can accept a PlatformItem.
    ///
    /// Typically implemented by a PlatformModule, currently HPModule and JetterControlModule.
    /// </summary>
    public interface IPlatformItemAcceptor
    {
        /// <summary>
        /// Return true if the PlatformItem can be accepted, or false and a message.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<AcceptResult> AcceptAsync(PlatformItem item);
    }
}
