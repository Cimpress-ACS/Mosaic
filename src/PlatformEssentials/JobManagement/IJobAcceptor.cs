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


using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.JobManagement
{
    public interface IJobAcceptor
    {
        /// <summary>
        /// Determines whether this instance can accept the specified job.
        /// </summary>
        /// <param name="job">The job.</param>
        AcceptResult CanAcceptJob(Job job);

        /// <summary>
        /// Determines whether this instance can accept any job.
        /// </summary>
        /// <returns>Result indicating whether the platform is capable of processing valid jobs.</returns>
        AcceptResult CanAcceptAnyJob();
    }
}
