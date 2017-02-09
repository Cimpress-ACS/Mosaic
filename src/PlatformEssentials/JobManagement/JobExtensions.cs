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
using System.Linq;
using VP.FF.PT.Common.PlatformEssentials.Entities;

namespace VP.FF.PT.Common.PlatformEssentials.JobManagement
{
    public static class JobExtensions
    {
        public static IList<PlatformItem> GetPlatformItems(this Job job)
        {
            return job.JobItems.OfType<JobItem>().SelectMany(jobItem => jobItem.GetPlatformItems()).ToArray();
        }

        public static bool ContainsPlatformItem(this Job job, PlatformItem platformItem)
        {
            return job.JobItems.OfType<JobItem>().Any(jobItem => jobItem.ContainsPlatformItem(platformItem));
        }

        public static JobItem GetJobItemByPlatformItem(this Job job, PlatformItem platformItem)
        {
            return job.JobItems.OfType<JobItem>().SingleOrDefault(jobItem => jobItem.ContainsPlatformItem(platformItem));
        }
    }
}
