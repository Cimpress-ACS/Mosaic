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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    /// <summary>
    /// Abstracted DB context to allow testability.
    /// </summary>
    public interface IEntityContext : IDisposable
    {
        /// <summary>
        /// Gets the query.
        /// </summary>
        IQueryable<T> GetQuery<T>() where T : class;

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        void Add<T>(T entity) where T : class;

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        void Delete<T>(T entity) where T : class;

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        void Update<T>(T entity) where T : class;

        /// <summary>
        /// Sets a field to an updated state, to force save
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <typeparam name="TProperty">Type of property that needs to be updates</typeparam>
        /// <param name="entity">Entity object</param>
        /// <param name="expression">Expression pointing to the property</param>
        void UpdateField<T, TProperty>(T entity, Expression<Func<T, TProperty>> expression) where T : class;

        /// <summary>
        /// Saves the changes.
        /// </summary>
        int SaveChanges();

        /// <summary>
        /// Saves the changes asynchronous.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Attaches the object to the Entity Framework Graph
        /// </summary>
        /// <typeparam name="T">The entity to attach</typeparam>
        /// <param name="entity"></param>
        void Attach<T>(T entity) where T : class;

        /// <summary>
        /// Detaches the object from the graph
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        void Detach<T>(T entity) where T : class;

        /// <summary>
        /// Marks the entity as unchaged
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        void Unchanged<T>(T entity) where T : class;

        /// <summary>
        /// Performs asynchronous save to the database and disposes the context.
        /// The idea for using this method is to basically configure the context with items that we want to store, 
        /// perform the changes on the Entity objects and then call this method in order to save the content to database.
        /// This method handles saves in an asynchronous manner, making sure that the order of saves is maintained.
        /// If the order of saves is not maintaned some saves could fail because EntityFramework includes the previous object values
        /// in the UPDATE statements to prevent accidental updates.
        /// </summary>
        void SaveEntityStateAsyncAndDispose();
    }
}
