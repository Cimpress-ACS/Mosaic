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


namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public interface IPlatformModuleRouteForcing
    {
        /// <summary>
        /// Forces the path form a source module to a target module. All items will directed to through this path, as long as there is enough capacity at the target module.
        /// This would override the automatic item routing.
        /// </summary>
        /// <param name="sourceModule">From source module.</param>
        /// <param name="targetModule">The target module.</param>
        /// <param name="sourcePortIndex">Source output port index, default is 0.</param>
        /// <param name="targetPortIndex">Target input port index, default is 0.</param>
        void ForcePath(IPlatformModule sourceModule, IPlatformModule targetModule, int sourcePortIndex, int targetPortIndex);

        /// <summary>
        /// Releases the force path. This enables the automatic item routing again.
        /// </summary>
        /// <param name="sourceModule">From source module.</param>
        /// <param name="targetModule">The target module.</param>
        /// <param name="sourcePortIndex">Source output port index, default is 0.</param>
        /// <param name="targetPortIndex">Target input port index, default is 0.</param>
        void ReleaseForcePath(IPlatformModule sourceModule, IPlatformModule targetModule, int sourcePortIndex, int targetPortIndex);

        /// <summary>
        /// Determines whether there is a possible path from a source-module to a target-module.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <param name="targetModule">The target module.</param>
        /// <returns>Value indicating wheter the routing is possible or not.</returns>
        bool IsRoutePossible(IPlatformModule sourceModule, IPlatformModule targetModule);
    }
}
