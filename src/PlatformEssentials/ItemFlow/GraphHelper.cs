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
using System.Linq;

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow
{
    public class GraphHelper
    {
        private readonly ModuleGraph _moduleGraph;

        public GraphHelper(ModuleGraph moduleGraph)
        {
            _moduleGraph = moduleGraph;
        }

        public IEnumerable<IPlatformModule> FindSourceModules()
        {
            var sourceModules =
                from m in _moduleGraph.Vertices
                where !_moduleGraph.InEdges(m).Any()
                select m;

            return sourceModules;
        }

        public IEnumerable<IPlatformModule> FindSinkModules()
        {
            var sinkModules =
                from m in _moduleGraph.Vertices
                where !_moduleGraph.OutEdges(m).Any()
                select m;

            return sinkModules;
        }

        public IEnumerable<IPlatformModule> FindUpStreamModules(IPlatformModule module)
        {
            var dependingEdges = _moduleGraph.InEdges(module);
            return dependingEdges.Select(edge => edge.Source);
        }

        public IEnumerable<IPlatformModule> FindDownStreamModules(IPlatformModule module)
        {
            var dependingEdges = _moduleGraph.OutEdges(module);
            return dependingEdges.Select(edge => edge.Target);
        }
    }
}
