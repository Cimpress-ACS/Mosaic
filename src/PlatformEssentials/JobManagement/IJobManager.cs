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


using System.Threading.Tasks;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.JobManagement
{
    public interface IJobManager
    {
        /// <summary>
        /// Adds a new job. The JobManager will try to start the job by broadcasting a TryStartJobEvent.
        /// </summary>
        /// <remarks>
        /// The returned task is completed when the next job in queue has been started.
        /// Waiting for the task to complete before calling AddNewJob again makes it possible
        /// to add and start jobs synchronously one at a time; however, ignoring the returned
        /// task and adding multiple jobs asynchronously is also fine.
        ///
        /// If this is called multiple times asynchronously, the returned task may get cancelled
        /// due to implementation details.
        /// </remarks>
        /// <param name="job">The job.</param>
        /// <returns>A task that completes when the next job is started</returns>
        Task AddNewJob(Job job);

        /// <summary>
        /// Mark a job item as failed.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="reason">The reason.</param>
        void FailJobItem(PlatformItem item, string reason);

        /// <summary>
        /// Mark a job item as fulfilled.
        /// </summary>
        /// <remarks>
        /// When all job items of a job are fulfilled, the job is completed and a message is sent to host.
        /// </remarks>
        /// <param name="item">The item.</param>
        void FulfillJobItem(PlatformItem item);
    }
}
