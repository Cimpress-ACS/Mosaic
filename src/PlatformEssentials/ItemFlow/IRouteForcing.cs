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
    public interface IRouteForcing<in TNode>
    {
        /// <summary>
        /// Option to recalculate the existing route, for example if something in the route has changed such as a module being full.
        /// </summary>
        void RecalculateRoute();

        /// <summary>
        /// Forces the path form a source module to a target module. All items will directed to through this path, as long as there is enough capacity at the target module.
        /// </summary>
        /// <param name="sourceModule">From source module.</param>
        /// <param name="targetModule">The target module.</param>
        /// <param name="sourcePortIndex">Source output port index, default is 0.</param>
        /// <param name="targetPortIndex">Target input port index, default is 0.</param>
        void ForcePath(TNode sourceModule, TNode targetModule, int sourcePortIndex = 0, int targetPortIndex = 0);

        /// <summary>
        /// Releases the force path.
        /// </summary>
        /// <param name="sourceModule">From source module.</param>
        /// <param name="targetModule">The target module.</param>
        /// <param name="sourcePortIndex">Source output port index, default is 0.</param>
        /// <param name="targetPortIndex">Target input port index, default is 0.</param>
        void ReleaseForcePath(TNode sourceModule, TNode targetModule, int sourcePortIndex = 0, int targetPortIndex = 0);

        /// <summary>
        /// Sets a flag whether a module ignores the downstream module or not.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="ignore">if set to <c>true</c> module ignores the state of the downstream modules.</param>
        void SetIgnoreDownstreamModule(string moduleName, bool ignore);

        /// <summary>
        /// Gets a flag indicating a module ignores the downstream modules.
        /// </summary>
        /// <param name="moduleName">Name of the module.</param>
        bool GetIgnoreDownstreamModule(string moduleName);
    }
}
