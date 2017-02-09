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


using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public interface IRouteCalculator<in TNode>
    {
        /// <summary>
        /// Whether a module is part of the graph.
        /// </summary>
        /// <param name="module">The module that should be verified whether its part of the graph.</param>
        /// <returns>True if the module is part of the graph, false otherwise.</returns>
        bool GraphContainsModule(TNode module);

        /// <summary>
        /// Gets the target module based on a release port.
        /// </summary>
        /// <param name="releasePort"></param>
        /// <param name="module">The current module where the item was released from.</param>
        /// <returns>The next module, or null if it has reached the end of the route.</returns>
        IPlatformModule GetTargetModule(int releasePort, TNode module);

        /// <summary>
        /// Whether a route from a source module to a target module is possible.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <param name="targetModule">The target module.</param>
        /// <returns>True if the route is possible.</returns>
        bool IsRoutePossible(TNode sourceModule, TNode targetModule);

        /// <summary>
        /// Calculates the single item routing based on Module states, CurrentItemCount, Limit, port-full, 
        /// and RouteItem flags like ForbiddenModuleType and ForceModuleInstance.
        /// </summary>
        /// <param name="item">The underlying item for path finding.</param>
        /// <param name="associatedModule">The current origin module of the item.</param>
        /// <returns>Whether the item routing was successful.</returns>
        bool CalculateSingleItemRouting(PlatformItem item, TNode associatedModule);

        /// <summary>
        /// Gets the graph that calculates the route.
        /// </summary>
        /// <returns></returns>
        ModuleGraphDTO GraphToDto();
    }
}
