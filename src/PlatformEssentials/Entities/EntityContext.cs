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
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    /// <summary>
    /// Use this DB context because of optimized performance tweaks
    /// </summary>
    public class EntityContext : DbContext, IEntityContext
    {
        private readonly ILogger _logger;
        private const int MaxRetries = 5;
        private const int RetryTimeoutMilliseconds = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityContext"/> class.
        /// Use it instead of Entities because improved performance tweaks.
        /// </summary>
        public EntityContext(string connectionString, ILogger logger)
            : base(connectionString)
        {
            _logger = logger;

            // classes are serializable because all properties are loaded and in memory (decreases performance)
            Configuration.LazyLoadingEnabled = false;

            // needed for lazy loading (creates wrapper with metadata around context classes)
            Configuration.ProxyCreationEnabled = false;

            //it is crucial because of delayed execution
            Configuration.AutoDetectChangesEnabled = false;

            //if (Settings.Default.LogSQL)
            //{
            //    Database.Log = Console.Write;
            //}
        }

        /// <summary>
        /// Gets the query for any entity because it's generic.
        /// </summary>
        public IQueryable<T> GetQuery<T>() where T : class
        {
            return Set<T>().AsQueryable();
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        public void Update<T>(T entity) where T : class
        {
            AttachIfNotAttached(entity);
            Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        public void UpdateField<T, TProperty>(T entity, Expression<Func<T, TProperty>> expression) where T : class
        {
            AttachIfNotAttached(entity);
            Entry(entity).State = EntityState.Modified;
            Entry(entity).Property(expression).IsModified = true;
        }


        private void AttachIfNotAttached<T>(T entity) where T : class
        {
            var ent = Entry(entity);
            if (ent == null || ent.State == EntityState.Detached)
            {
                Set<T>().Attach(entity);
                Entry(entity).State = EntityState.Unchanged;
            }
        }

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        public void Add<T>(T entity) where T : class
        {
            var ent = Entry(entity);
            if (ent == null || ent.State != EntityState.Added)
            {
                Set<T>().Add(entity);
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        public void Delete<T>(T entity) where T : class
        {
            AttachIfNotAttached(entity);
            Set<T>().Remove(entity);
        }

        public void Attach<T>(T entity) where T : class
        {
            AttachIfNotAttached(entity);
        }

        public void Detach<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Detached;
        }

        public void Unchanged<T>(T entity) where T : class
        {
            AttachIfNotAttached(entity);
            Entry(entity).State = EntityState.Unchanged;
        }

        public void DetectChanges()
        {
            ChangeTracker.DetectChanges();
        }

        // force the compile that EntityFramework.SqlServer will be copied to target folder
        private void ForceEntityFrameworkSqlServerAssemblyCopy()
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        /// <summary>
        /// The implementation of this interface method enqueus the action to be run on the custom
        /// task scheduler. The task scheduler runs asynchronously, however it has only one slot, that allows to maintain
        /// the order of execution for each of the items.
        /// </summary>
        public void SaveEntityStateAsyncAndDispose()
        {
            lock (Locker)
            {
                DetectChanges();
                if (!TasksToRun.Any())
                {
                    var task = GetSaveTask();
                    TasksToRun.Enqueue(task);
                    task.Start();
                }
                else
                {
                    TasksToRun.Enqueue(GetSaveTask());
                }
            }
        }

        private static readonly object Locker = new object();

        private static readonly Queue<Task> TasksToRun = new Queue<Task>();

        /// <summary>
        /// This method rigs properly the task to have continuation that processes next items in the
        /// queue. 
        /// The save operations in this setup are not allowed to fail, if failure occurs it may lead to
        /// inconsistent database state. The purpose of performing one update at a time in the order they came is to make 
        /// sure that updates are consistent.
        /// </summary>
        /// <returns></returns>
        private Task GetSaveTask()
        {
            var task = new Task(() =>
            {
                using (this)
                {
                    try
                    {
                        SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        _logger.Error("Optimistic concurrency issue", e);
                    }
                    catch (DbUpdateException e)
                    {
                        _logger.Error("Write to DB failed", e);
                    }
                    catch (OptimisticConcurrencyException e)
                    {
                        _logger.Error("Optimistic concurrency issue", e);
                    }
                    catch (Exception e)
                    {
                        _logger.Error("FATAL", e);
                    }
                }
            });
            task.ContinueWith(a =>
            {
                lock (Locker)
                {
                    TasksToRun.Dequeue();
                    if (TasksToRun.Any())
                        TasksToRun.Peek().Start();
                }
            });
            return task;
        }
    }
}
