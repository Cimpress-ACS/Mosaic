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
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.JobManagement
{
    [Export(typeof(IJobContainer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class JobContainer : IJobContainer
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, Job> _jobs;
        private readonly object _syncLock = new object();
        private static int _internalSequence;
        private IEntityContextFactory _contextFactory;

        [ImportingConstructor]
        public JobContainer(
            ILogger logger,
            IEntityContextFactory contextFactory,
            IPlatformModuleEntities platformModuleEntities)
        {
            _logger = logger;
            _logger.Init(GetType());

            _internalSequence = 0;

            _jobs = new Dictionary<string, Job>();

            _contextFactory = contextFactory;

            LoadPersistedJobs(platformModuleEntities);
        }

        private void LoadPersistedJobs(IPlatformModuleEntities platformModuleEntities)
        {
            using (var dbcontext = _contextFactory.CreateContext())
            {
                var jobs = dbcontext.GetQuery<Job>().Include(j => j.JobItems).ToList();

                foreach (var job in jobs)
                {
                    _jobs.Add(job.JobId, job);
                    _logger.DebugFormat("loaded job: {0}", job.JobId);
                }
            }
        }

        public IEnumerable<Job> GetJobs(Func<Job, bool> predicate)
        {
            lock (_syncLock)
            {
                return _jobs.Values.Where(predicate).OrderBy(j => j.SequenceId).ToList();
            }
        }

        public void RemoveJobs(Func<Job, bool> predicate, Func<Job, string> message)
        {
            lock (_syncLock)
            {
                List<Job> jobsToDiscard = _jobs.Values.Where(predicate).ToList();

                using (var dbcontext = _contextFactory.CreateContext())
                {
                    foreach (Job job in jobsToDiscard)
                    {
                        if (_jobs.Remove(job.JobId))
                        {
                            _logger.Info(message(job));
                        }
                        else
                        {
                            _logger.WarnFormat("Could not remove job with id '{0}'", job.JobId);
                        }

                        foreach (var item in job.JobItems)
                        {
                            dbcontext.Delete(item);
                        }

                        dbcontext.Delete(job);
                    }

                    dbcontext.SaveChanges();
                }
            }
        }

        public void ClearAllJobs()
        {
            lock (_syncLock)
            {
                using (var dbcontext = _contextFactory.CreateContext())
                {
                    ((CommonEntityContext)dbcontext).JobItems.ForEach(j => dbcontext.Delete(j));
                    ((CommonEntityContext)dbcontext).Jobs.ForEach(j => dbcontext.Delete(j));
                    dbcontext.SaveChanges();
                }

                _jobs.Clear();
            }

        }

        public void AddOrUpdate(Job job)
        {
            lock (_syncLock)
            {
                using (var dbcontext = _contextFactory.CreateContext())
                {
                    if (_jobs.ContainsKey(job.JobId))
                    {
                        LogJobDetails("Updating", job);
                        _jobs[job.JobId] = job;

                        dbcontext.Update(job);
                        dbcontext.SaveChanges();
                    }
                    else
                    {
                        job.SequenceId = GetSequenceNumber();
                        LogJobDetails("Adding", job);
                        _jobs.Add(job.JobId, job);

                        dbcontext.Add(job);
                        foreach (var jobItem in job.JobItems)
                        {
                            dbcontext.Add(jobItem);
                        }

                        dbcontext.SaveChanges();
                    }
                }
            }
        }

        private void LogJobDetails(string operation, Job job)
        {
            _logger.DebugFormat("{0} job (sequenceId: {1}, jobId: {2})",
                operation,
                job.SequenceId,
                job.JobId);
        }

        private long GetSequenceNumber()
        {
            // reset the internal sequence number if there are no jobs in the dictionary
            if (_jobs.Count == 0) _internalSequence = 0;
            return _internalSequence++;
        }
    }
}
