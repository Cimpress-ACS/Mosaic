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
using System.ComponentModel.Composition;
using System.Linq;

namespace VP.FF.PT.Common.Simulation
{
    [Export(typeof(ITaktPartsRepository))]
	[PartCreationPolicy(CreationPolicy.Shared)]
    public class TaktPartsRepository : ITaktPartsRepository
    {
        [Import]
        private IModuleSimulatorRepository _moduleRepository;
        
        [ImportMany(AllowRecomposition = true)] 
        private IEnumerable<ITakt> _importedTaktParts;

        private readonly List<ITakt> _taktPartsAddedAtRuntime;
        
        public TaktPartsRepository()
        {
            _taktPartsAddedAtRuntime = new List<ITakt>();
        }

        public void AddTaktPart(ITakt taktPart)
        {
            _taktPartsAddedAtRuntime.Add(taktPart);
        }

        public IEnumerable<ITakt> GetTaktParts()
        {
            return _taktPartsAddedAtRuntime.Concat(_importedTaktParts).Concat(_moduleRepository.Modules).Distinct();
        }
    }
}
