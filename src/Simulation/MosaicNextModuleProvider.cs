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
using System.Linq;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow;
using VP.FF.PT.Common.PlatformEssentials.ItemFlow.DTOs;

namespace VP.FF.PT.Common.Simulation
{
    [Export(typeof(MosaicNextModuleProvider))]
    public class MosaicNextModuleProvider : INextModuleProvider
    {
        private readonly IModuleSimulatorRepository _moduleRepository;
        private readonly ModuleGraphDTO _graphDto;

        [ImportingConstructor]
        public MosaicNextModuleProvider(IModuleBusManager moduleBusManager, IModuleSimulatorRepository moduleRepository)
        {
            _moduleRepository = moduleRepository;

            _graphDto = moduleBusManager.GraphDto;
        }

        public IModuleSimulator GetNextModule(IModuleSimulator simulator)
        {
            var currentNode = _graphDto.Vertices.FirstOrDefault(v => v.Name == simulator.Name);

            if (currentNode == null)
            {
                return null;
            }

            var lastEdge = _graphDto.Edges.LastOrDefault(e => e.Source == currentNode.Name);

            if (lastEdge == null)
            {
                return null;
            }

            return _moduleRepository.GetModule(lastEdge.Target);
        }
    }
}
