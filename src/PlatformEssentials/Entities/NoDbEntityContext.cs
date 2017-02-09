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


using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    public class NoDbEntityContext : IEntityContext
    {
        public void Dispose()
        {
        }

        public IQueryable<T> GetQuery<T>() where T : class
        {
            var dummy = new Collection<T>();
            return dummy.AsQueryable();
        }

        public IQueryable GetQuery(Type T)
        {
            return null;
        }

        public void Add<T>(T entity) where T : class
        {

        }

        public void Delete<T>(T entity) where T : class
        {

        }

        public void Update<T>(T entity) where T : class
        {

        }

        public void UpdateField<T, TProperty>(T entity, Expression<Func<T, TProperty>> expression) where T : class
        {

        }

        public void RequestSaveChanges()
        {
        }

        public int SaveChanges()
        {
            return 0;
        }

        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(0);
        }

        public Task<int> SaveChangesRetryableAsync()
        {
            return Task.FromResult(0);
        }

        public void Attach<T>(T entity) where T : class
        {
        }

        public void Detach<T>(T entity) where T : class
        {

        }

        public void Unchanged<T>(T entity) where T : class
        {

        }

        public void SaveEntityStateAsyncAndDispose()
        {

        }
    }
}
