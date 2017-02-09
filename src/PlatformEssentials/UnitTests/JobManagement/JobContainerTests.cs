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
using System.Linq;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.JobManagement;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.JobManagement
{
    [TestFixture]
    public class JobContainerTests
    {
        private IJobContainer _jobContainer;
        private Mock<ILogger> _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger>();
            _jobContainer = new JobContainer(
                                    _logger.Object, new EntityContextFactory(_logger.Object), 
                                    new Mock<IPlatformModuleEntities>().Object);
        }

        [Test]
        public void InsertItems_Extract_VerifyOrder()
        {
            for (int i = 0; i < 20; i++)
            {
                Job job = new Job { Id = i, JobId = i.ToString() };
                _jobContainer.AddOrUpdate(job);
            }

            int jobId = 0;
            foreach (Job extractedJob in _jobContainer.GetJobs(j => true))
            {
                Assert.AreEqual((int)extractedJob.Id, jobId++, "Job elements out of sequence");
            }
        }

        [Test]
        public void InsertManyItems_Extract_VerifyOrder()
        {
            for (int i = 0; i < 35000; i++)
            {
                Job job = new Job { Id = i, JobId = i.ToString() };
                _jobContainer.AddOrUpdate(job);
            }

            int jobId = 0;
            Job previousJob = new Job { SequenceId = -1 };
            foreach (Job extractedJob in _jobContainer.GetJobs(j => true))
            {
                Assert.AreEqual((int)extractedJob.Id, jobId++, String.Format("Job Elements out of sequence current: {0} previous: {1}",
                    extractedJob.SequenceId, previousJob.SequenceId));
                previousJob = extractedJob;
            }
        }

        [Test]
        public void InsertItems_RemoveAll_EnsureSequenceNumberWasReset()
        {
            int numOfJobs = 10;
            int initialSequenceNumber = 0;

            for (int i = 0; i < numOfJobs; i++)
            {
                var job = new Job { Id = i, JobId = i.ToString() };
                _jobContainer.AddOrUpdate(job);
            }

            Assert.AreEqual(numOfJobs, _jobContainer.GetJobs(j => true).Count());

            // Remove all
            _jobContainer.RemoveJobs(j => true, j => "Cannot remove {0}");

            Assert.AreEqual(0, _jobContainer.GetJobs(j => true).Count());

            // Add a new job and verify the sequence number
            _jobContainer.AddOrUpdate(new Job { Id = 3, JobId = "whatever" });

            Job testJob = _jobContainer.GetJobs(j => true).SingleOrDefault();

            Assert.IsNotNull(testJob);
            Assert.AreEqual(initialSequenceNumber, testJob.SequenceId);
        }

        [Test]
        public void InsertNewJob_EnsureLoggingInfoIsProvided()
        {
            // expected values
            string operation = "Adding";
            string jobId = "888111";
            long sequenceId = 0;
            string expectedFormat = "{0} job (sequenceId: {1}, jobId: {2})";

            Job testJob = new Job
            {
                JobId = jobId,
                SequenceId = sequenceId,
                JobItems = new List<JobItem>
                {
                    new JobItem
                    {
                        ItemHostId = "111333"
                    }
                }
            };

            _logger.ResetCalls();

            _jobContainer.AddOrUpdate(testJob);

            Assert.AreEqual(1, _jobContainer.GetJobs(j => true).Count());
            _logger.Verify(l => l.DebugFormat(expectedFormat, operation, sequenceId, jobId), Times.Once);
        }
    }
}
