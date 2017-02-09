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

namespace VP.FF.PT.Common.PlatformEssentials.Entities
{
    public class Job
    {
        public Job()
        {
            JobItems = new HashSet<JobItem>();
        }
    
        // Used as primary key in database. Will be generated automatically by entity framework.
        public long Id { get; set; }

        // Logical primary key which should be used for business logic. Usually this is linked with the host.
        public string JobId { get; set; }

        public long SequenceId { get; set; }
    
        public virtual ICollection<JobItem> JobItems { get; set; }
    }
}
