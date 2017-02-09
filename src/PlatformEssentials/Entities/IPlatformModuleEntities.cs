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

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    /// <summary>
    /// Interface of an object that should serve as a static store of entity objects that describe
    /// platform modules and platform items. Especially handy in scenarios where that startup loads data from the database: 
    /// if each module loaded this data separately there would be duplicate entities in the system, whereas every entity object should
    /// exist only once across all modules.
    /// </summary>
    [InheritedExport]
    public interface IPlatformModuleEntities
    {
        PlatformModuleEntity GetPlatformModuleEntity(PlatformModuleEntity obj);
        IEnumerable<PlatformModuleEntity> GetAll();
    }
}
