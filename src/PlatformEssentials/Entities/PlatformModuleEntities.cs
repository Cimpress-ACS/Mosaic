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
using System.Data.Entity;
using System.Linq;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    public class PlatformModuleEntities : IPartImportsSatisfiedNotification, IPlatformModuleEntities
    {
        [Import]
        internal IEntityContextFactory EntityContextFactory  { get; set; }

        private static Dictionary<string, PlatformModuleEntity> _platformModules = new Dictionary<string, PlatformModuleEntity>();

        public void OnImportsSatisfied()
        {
            lock (_platformModules)
            {
                var context = EntityContextFactory.CreateContext();
                
                if (context is CommonEntityContext)
                {
                    context.GetQuery<Route>().Include(x => x.RouteItems).Load();
                    _platformModules = context.GetQuery<PlatformModuleEntity>().Include(x => x.PlatformItems).ToDictionary(a => a.Name, b => b);
                }

                context.SaveChanges();
            }
        }

        public PlatformModuleEntity GetPlatformModuleEntity(PlatformModuleEntity obj)
        {
            lock (_platformModules)
            {
                if (_platformModules.ContainsKey(obj.Name))
                {
                    return _platformModules[obj.Name];
                }

                var context = EntityContextFactory.CreateContext();
                if (!(context is CommonEntityContext))
                    return null;

                context.Add(obj);
                context.SaveChanges();
                _platformModules.Add(obj.Name, obj);
            }
            return obj;
        }

        public IEnumerable<PlatformModuleEntity> GetAll()
        {
            return _platformModules.Values.ToList();
        }
    }
}
