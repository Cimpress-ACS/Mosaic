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
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure;
using VP.FF.PT.Common.Infrastructure.FileAccess;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Events;
using VP.FF.PT.Common.PlatformEssentials.JobManagement;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.JobManagement
{
    [TestFixture]
    public class JobManagerTests
    {
        private IJobManager _testee;
        private Mock<IEventAggregator> _eventAggregator;
        private Mock<ILogger> _logger;
        private Mock<IPathExists> _fileSystem;
        private IJobContainer _jobContainer;

        private const string PdfFile = "file.pdf";
        private const string TifFile = "file.tif";
        private const long Barcode = 741;
        private const long JobId = 50;

        [SetUp]
        public void SetUp()
        {
            var entityContextFactory = new EntityContextFactory(new Mock<ILogger>().Object);
            _eventAggregator = new Mock<IEventAggregator>();
            _logger = new Mock<ILogger>();
            _fileSystem = new Mock<IPathExists>();
            _jobContainer = new JobContainer(_logger.Object, entityContextFactory, new Mock<IPlatformModuleEntities>().Object);

            _testee = new JobManager();

            var setupTestee = new PrivateObject(_testee);
            setupTestee.SetFieldOrProperty("Logger", _logger.Object);
            setupTestee.SetFieldOrProperty("EventAggregator", _eventAggregator.Object);
            setupTestee.SetFieldOrProperty("Jobs", _jobContainer);
        }

        [Test]
        public void FailJobItem_WithNull_DoesNotThrow()
        {
            Action a = () => _testee.FailJobItem(null, "none reason");

            a.ShouldNotThrow();
        }

        [Test]
        public void FulfillJobItem_WithNull_DoesNotThrow()
        {
            Action a = () => _testee.FulfillJobItem(null);

            a.ShouldNotThrow();
        }

        [Test]
        public void FailJobItem_WithUnknownItem_LogsWarning()
        {
            _logger.Setup(
                l => l.WarnFormat(
                    It.Is<string>(s => s.Contains("Unknown barcode")),
                    It.Is<long>(b => b == Barcode)));

            _testee.FailJobItem(new PlatformItem { ItemId = Barcode }, "none reason");

            _logger.VerifyAll();
        }

        [Test]
        public void FulfillJobItem_WithUnknownItem_LogsWarning()
        {
            _logger.Setup(
                l => l.WarnFormat(
                    It.Is<string>(s => s.Contains("Unknown barcode")),
                    It.Is<long>(b => b == Barcode)));

            _testee.FulfillJobItem(new PlatformItem { ItemId = Barcode });

            _logger.VerifyAll();
        }

        [Test]
        public void FulfillJobItem_WhenJobComplete_PublishesEvent()
        {
            _fileSystem.Setup(fs => fs.FileExists(PdfFile)).Returns(true);
            _fileSystem.Setup(fs => fs.FileExists(TifFile)).Returns(true);
            _eventAggregator
                .Setup(e => e.Publish(
                                It.Is<JobCompletedEvent>(
                                    evt => evt.JobId == JobId.ToString())));

            var job = CreateJob(JobId, Barcode, PdfFile, TifFile);
            _testee.AddNewJob(job).Wait();
            _testee.FulfillJobItem(new PlatformItem { ItemId = Barcode });
            job.JobItems.First().State.Should().Be(JobItemState.Fullfilled);

            _eventAggregator.VerifyAll();
        }

        [Test]
        public void AddJobItem_PublishesEventToTryStartJob()
        {
            _eventAggregator
                .Setup(e => e.Publish(
                                It.Is<TryStartJobEvent>(
                                    evt => evt.JobId == JobId.ToString())));

            var job = CreateJob(JobId, Barcode, PdfFile, TifFile);
            _testee.AddNewJob(job).Wait();

            _eventAggregator.VerifyAll();
        }

        private static Job CreateJob(long jobId, long barcode, string pdf, string tif)
        {
            var job = new Job
            {
                JobId = jobId.ToString(),
                JobItems = new Collection<JobItem>
                {
                    new JobItem
                    {
                        State = JobItemState.New,
                        ItemHostId = barcode.ToString(),
                    }
                }
            };

            job.JobItems.First().AssosiatedPlatformItems.Add(new PlatformItem { ItemId = barcode });

            return job;
        }
    }
}
