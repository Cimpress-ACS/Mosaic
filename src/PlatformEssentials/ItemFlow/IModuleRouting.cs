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
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public interface IModuleRouting
    {
        /// <summary>
        /// Instructs the module to route a single item to an output port as soon as possible.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="outputPortIndex">Index of the output port.</param>
        void AddItemRouting(PlatformItem item, int outputPortIndex);

        /// <summary>
        /// Cancels the route task of a single item.
        /// </summary>
        /// <remarks>
        /// The module bus manager might decide that another route is more efficient or there is maybe a traffic jam etc.
        /// </remarks>
        /// <param name="item">The item.</param>
        void RemoveItemRouting(PlatformItem item);

        /// <summary>
        /// Instructs the module to guide all items through port 1.
        /// </summary>
        /// <param name="portIndex">Index of the port.</param>
        void AddPortRouting(int portIndex);

        /// <summary>
        /// Releases a port routing.
        /// </summary>
        /// <param name="portIndex">Index of the port.</param>
        void RemovePortRouting(int portIndex);

        /// <summary>
        /// Contains all active port routings for this module. Every module must respect this "task collection".
        /// Port routing has higher priority than single item routing, it will just move all items to a port and disregards the ItemId and any item state.
        /// <remarks>
        /// It's a HashSet because a module can have multiple active port routings at the same time. In this case it should route everything to the first port until it is full
        /// then the second port and so on... Note: The port-routing gets remove automatically if the target is full, the module must not care.
        /// </remarks>
        /// </summary>
        HashSet<int> PortRoutings { get; }

        /// <summary>
        /// Contains all active individual item routings. It maps an ItemId to an output port.
        /// Every module must respect this "task collection" and listen for incoming items to route those to the specified ports.
        /// This behavior might be overidden by the port-routings.
        /// </summary>
        IDictionary<PlatformItem, int> ItemRoutings { get; }

        /// <summary>
        /// Stops the module from releasing PlatformItems, but keeps the module in a running state.
        /// </summary>
        /// <param name="enable">Value true to not hold back PlatformItems but still in production mode, false to release items to the next module.</param>
        /// <param name="port">Specifies the port index to stop release items (optional because most modules has only one output port).</param>
        void StopReleaseItems(bool enable, int port = 0);
    }
}
