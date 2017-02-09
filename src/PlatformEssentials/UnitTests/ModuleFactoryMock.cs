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

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests
{
    [Export("1", typeof(IPlatformModuleFactory))]
    [Export("2", typeof(IPlatformModuleFactory))]
    [Export("3", typeof(IPlatformModuleFactory))]
    [Export("4", typeof(IPlatformModuleFactory))]
    [Export("6", typeof(IPlatformModuleFactory))]
    [Export("7", typeof(IPlatformModuleFactory))]
    [Export("8", typeof(IPlatformModuleFactory))]
    public class ModuleFactoryMock : PlatformModuleFactoryBase
    {
        [Import]
        private ExportFactory<ModuleMock> _exportFactory;

        protected override IPlatformModule CreateModuleInstance()
        {
            return _exportFactory.CreateExport().Value;
        }
    }
}
