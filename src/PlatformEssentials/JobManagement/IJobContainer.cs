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
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.JobManagement
{
    /// <summary>
    /// IJobContainer represents a wrapper to have access to a thread safe ordered job list.
    /// The implementation can also persist the jobs into an EntityContext.
    /// </summary>
    public interface IJobContainer
    {
        /// <summary>
        /// Retrieves a collection of jobs based on a specific condition (predicate)
        /// </summary>
        /// <param name="predicate">the condition that needs to be true to return the jobs</param>
        /// <returns>a collection of jobs</returns>
        IEnumerable<Job> GetJobs(Func<Job, bool> predicate);

        /// <summary>
        /// Removes jobs from the container based on a condition (predicate)
        /// </summary>
        /// <param name="predicate">condition to be satisfied to remove jobs</param>
        /// <param name="message">a formatted message to be provided in the logs when the job cannot be removed</param>
        void RemoveJobs(Func<Job, bool> predicate, Func<Job, string> message);
        
        /// <summary>
        /// Clears all jobs from the container
        /// </summary>
        void ClearAllJobs();

        /// <summary>
        /// Add or update a job
        /// </summary>
        /// <param name="job">the job to be added/updated</param>
        void AddOrUpdate(Job job);
    }
}
