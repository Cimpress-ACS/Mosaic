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


using FluentAssertions;
using System.Collections.Generic;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class TagImporterTests
    {
        private Log4NetLogger _logger;

        private ITagController _tagController;
        private ITagListener _tagListener;
        private ITagImporter _tagImporter;

        [SetUp]
        public void SetUp()
        {
            _logger = new Log4NetLogger();
            _tagController = new BeckhoffTagController(Global.AdsAddress, Global.AdsPort, _logger);
            _tagListener = new BeckhoffPollingTagListener(Global.AdsAddress, Global.AdsPort, new GlobalLock(), _logger);

            _tagController.StartConnection();
            _tagListener.StartListening();

            _tagImporter = new BeckhoffOnlineTagImporter(_logger);
            _tagImporter.Initialize(Global.AdsAddress, Global.AdsPort);
        }

        [TearDown]
        public void TearDown()
        {
            _tagController.Dispose();
            _tagListener.Dispose();
            _tagImporter.Dispose();
        }

        [Test]
        public void SimpleTagImporterTest()
        {
            IReadOnlyCollection<Tag> importedTags = _tagImporter.ImportTags("MiddlePRG_1.fbAGS_1");

            var listener = new BeckhoffPollingTagListener(Global.AdsAddress, Global.AdsPort, new GlobalLock(), _logger);
            foreach (var importedTag in importedTags)
            {
                listener.AddTagsRecursively(importedTag);
            }
            listener.RefreshAll();

            var tag = new Tag("fbAGS_1.SIf.In.bolAddTestAlarm", "MiddlePRG_1", Global.AdsPort);
            listener.GetTags().Should().ContainSingle(t => t == tag);
        }

        /* Beckhoff comment:
            /// a useful comment for the user (strSingleCommentTest)
        */
        [Test]
        public void ImportSingleCommentTagTest()
        {
            var tag = _tagImporter.ImportTag("MiddlePRG_1.fbAGS_1.MetadataTest.strSingleCommentTest");

            tag.MetaData.Comment.Should().Be("a useful comment for the user (strSingleCommentTest)");
        }

        /* Beckhoff comment:
            (*
                a useful comment for the user (strMultiCommentTest)
                with more details
            *)
         */
        [Test]
        public void ImportMultiCommentTagTest()
        {
            var tag = _tagImporter.ImportTag("MiddlePRG_1.fbAGS_1.MetadataTest.strMultiCommentTest");

            tag.MetaData.Comment.Should().Be(
                "a useful comment for the user (strMultiCommentTest)\r\n\t\twith more details");
        }

        /* Beckhoff comment:
            /// a useful comment for the user (strCommentMetadataTest) {hidden}{simulation}
        */
        [Test]
        public void ImportCommentWithMetadataTagTest()
        {
            ITagImporter tagImporter = new BeckhoffOnlineTagImporter(_logger);
            tagImporter.Initialize(Global.AdsAddress, Global.AdsPort);
            var tag = tagImporter.ImportTag("MiddlePRG_1.fbAGS_1.MetadataTest.strCommentMetadataTest");

            tag.MetaData.Comment.Should().Be("a useful comment for the user (strCommentMetadataTest) {hidden}{simulation}");
        }

        /* Beckhoff comment:
	        (*
		        a useful comment for the user (strCommentMetadataTestMixed_m)
		
		        {unit:ms}
		        defines the unit for UI. The unit in PLC might be different, in this case a conversion is necessary
		
		        {range:0-999}
		        limits the range in UI. User cannot enter values exceeding the range.
		
		        {offset:5.8}
		        shifts the value by an offset in UI
		
		        {hidden}
		        hides the Parameter/Configuration in UI except for the "saber" user who can see everything
		
		        {readptr}
		        tells the TagImporter to follow a pointer/reference and read out the de-referenced type. can be recursive. must be smart enough to detect cyclic pointers and break
		
		        {simulation}
		        Tag is only useful when controller is in simulation mode
	        *)
        */
        [Test]
        public void strCommentMetadataTestMixed_m()
        {
            var tag = _tagImporter.ImportTag("MiddlePRG_1.fbAGS_1.MetadataTest.strCommentMetadataTestMixed_m");

            tag.MetaData.Comment.Should().Be(
                "a useful comment for the user (strCommentMetadataTestMixed_m)\r\n		\r\n		{unit:ms}\r\n		defines the unit for UI. The unit in PLC might be different, in this case a conversion is necessary\r\n		\r\n		{range:0-999}\r\n		limits the range in UI. User cannot enter values exceeding the range.\r\n		\r\n		{offset:5.8}\r\n		shifts the value by an offset in UI\r\n		\r\n		{hidden}\r\n		hides the Parameter/Configuration in UI except for the \"saber\" user who can see everything\r\n		\r\n		\r\n		tells the TagImporter to follow a pointer/reference and read out the de-referenced type. can be recursive. must be smart enough to detect cyclic pointers and break\r\n		\r\n		{simulation}\r\n		Tag is only useful when controller is in simulation mode");
        }
    }
}
