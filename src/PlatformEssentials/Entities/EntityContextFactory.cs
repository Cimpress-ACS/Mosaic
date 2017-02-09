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


using System.Data.Entity;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    /// <summary>
    /// Creates a new common DB context. The class is designed to extend it via inheritance for a custom context.
    /// </summary>
    public class EntityContextFactory : IEntityContextFactory
    {
        protected readonly ILogger Logger;

        public EntityContextFactory(ILogger logger)
        {
            Logger = logger;
            Logger.Init(GetType());
            Database.SetInitializer(new CreateDatabaseIfNotExists<EntityContext>());
        }

        /// <summary>
        /// Creates the context.
        /// </summary>
        public virtual IEntityContext CreateContext()
        {
            Logger.Info("Creating NoDbEntityContext");
            return new NoDbEntityContext();
        }
    }
}
