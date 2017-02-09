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

namespace VP.FF.PT.Common.PlatformEssentials.ItemFlow.Dependencies
{
    [Export(typeof(IPlatformDependencyManager))]
    public class PlatformDependencyManager : IPlatformDependencyManager 
    {
        private readonly List<IDependency> _dependencies = new List<IDependency>();

        public void Add(IDependency dependency)
        {
            _dependencies.Add(dependency);
        }

        public void Remove(IDependency dependency)
        {
            _dependencies.Remove(dependency);
        }

        public IList<IDependency> Dependencies
        {
            get { return _dependencies.ToArray(); }
        }
    }
}
