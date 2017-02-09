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
using System.Threading.Tasks;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Events;

namespace VP.FF.PT.Common.PlatformEssentials.JobManagement
{
    public class JobManager : IJobManager, IPartImportsSatisfiedNotification
    {
        [Import]
        protected ILogger Logger;

        [Import]
        protected IEventAggregator EventAggregator;

        [Import]
        protected IJobContainer Jobs;

        protected readonly SingleActionScheduler StartJobScheduler;
        protected readonly IPlatformScheduler RemoveJobsScheduler;

        public JobManager()
        {
            StartJobScheduler = new SingleActionScheduler();
            RemoveJobsScheduler = new SingleActionScheduler();
        }

        public virtual Task AddNewJob(Job job)
        {
            Jobs.AddOrUpdate(job);

            EventAggregator.Publish(new NewJobAvailableEvent());

            return StartJobScheduler.Schedule(TryStartNextJob);
        }

        public void FailJobItem(PlatformItem item, string reason)
        {
            if (item == null)
            {
                Logger.Warn("FailJobItem: The given platform item is null");
                return;
            }

            var job = GetJobByPlatformItem(item);
            if (job == null)
            {
                Logger.WarnFormat("FailJobItem: Unknown barcode '{0}'", item.ItemId);
                return;
            }

            var jobItem = job.GetJobItemByPlatformItem(item);
            if (jobItem != null)
            {
                jobItem.State = JobItemState.Failed;
                jobItem.FailedReason = reason;

                Logger.WarnFormat("Job item failed - JobId='{0}', ItemId='{1}', reason='{2}'",
                    job.JobId, jobItem.ItemHostId, reason);
            }
        }

        public void FulfillJobItem(PlatformItem item)
        {
            if (item == null)
            {
                Logger.Warn("FulfillJobItem: The given platform item is null");
                return;
            }

            var job = GetJobByPlatformItem(item);
            if (job == null)
            {
                Logger.WarnFormat("FulfillJobItem: Unknown barcode '{0}'", item.ItemId);
                return;
            }

            Logger.DebugFormat("FulfillJobItem: JobId='{0}', barcode='{1}'", job.JobId, item.ItemId);

            var jobItem = job.GetJobItemByPlatformItem(item);
            if (jobItem == null)
            {
                Logger.Error("JobItem lookup failed");
                return;
            }

            Logger.DebugFormat("FulfillJobItem: Previous state was '{0}'", jobItem.State);
            jobItem.State = JobItemState.Fullfilled;

            var fulfilled = job.JobItems.Count(ji => ji.State == JobItemState.Fullfilled);
            var total = job.JobItems.Count();
            if (fulfilled < total)
            {
                Logger.DebugFormat("Job '{0}' contains {1}/{2} fulfilled job items", job.Id, fulfilled, total);
            }
            else
            {
                EventAggregator.Publish(new JobCompletedEvent(job.JobId));
            }
        }

        protected virtual void TryStartNextJob()
        {
            RemoveJobsScheduler.Schedule(RemoveCompletedAndInvalidJobs);

            var job = GetUnstartedJobs().FirstOrDefault();
            if (job == null)
            {
                return;
            }

            EventAggregator.Publish(new TryStartJobEvent(job.JobId));

            Logger.DebugFormat("Job counts: unstarted={0}, ongoing={1}, completed={2}",
                GetUnstartedJobs().Count(), GetJobsInProduction().Count(), GetCompletedJobs().Count());

            StartJobScheduler.Schedule(TryStartNextJob);
        }

        protected virtual void RemoveCompletedAndInvalidJobs()
        {
            Jobs.RemoveJobs(IsCompleted, job => string.Format("Removed job '{0}' because it is completed", job.JobId));
        }

        private IEnumerable<Job> GetUnstartedJobs()
        {
            return Jobs.GetJobs(job => job.JobItems.All(jobItem => jobItem.State == JobItemState.New));
        }

        private IEnumerable<Job> GetJobsInProduction()
        {
            return Jobs.GetJobs(job => job.JobItems.All(jobItem => jobItem.State == JobItemState.InProduction));
        }

        private IEnumerable<Job> GetCompletedJobs()
        {
            return Jobs.GetJobs(IsCompleted);
        }

        private static bool IsCompleted(Job job)
        {
            return job.JobItems.All(jobItem => jobItem.State == JobItemState.Fullfilled);
        }

        private Job GetJobByPlatformItem(PlatformItem item)
        {
            return Jobs.GetJobs(job => job.ContainsPlatformItem(item)).FirstOrDefault();
        }

        public void OnImportsSatisfied()
        {
            Logger.Init(GetType());
        }
    }
}
