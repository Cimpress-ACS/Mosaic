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


using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class TagMetaDataParserIntegrationTests
    {
        private Log4NetLogger _logger;
        private ITagImporter _tagImporter;

        [SetUp]
        public void SetUp()
        {
            _logger = new Log4NetLogger();

            _tagImporter = new BeckhoffOnlineTagImporter(_logger);
            _tagImporter.Initialize(Global.AdsAddress, Global.AdsPort);

        }

        [TearDown]
        public void TearDown()
        {
            _tagImporter.Dispose();
        }

        [Test]
        public void GivenPtrMetadata_ShouldReadPointer()
        {
            var tag = _tagImporter.ImportTag("MiddlePRG_1.fbAGS_1.ptrPointerTest");

            tag.MetaData.ReadPointer.Should().BeTrue();
        }

        [Test]
        public void GivenNoPtrMetadata_ShouldNotReadPointer()
        {
            var tag = _tagImporter.ImportTag("MiddlePRG_1.fbAGS_1.ptrPointer2Test");

            tag.MetaData.ReadPointer.Should().BeFalse();
        }

        /*
         STRUCT
	        bolPrintOk: BOOL;
	        lreActTrayPos_mm : LREAL;
	        eState: E_Test;	
         END_STRUCT
         * */
        [Test]
        public void WhenReadPointer_ShouldContainData()
        {
            var tag = _tagImporter.ImportTagRecursive("MiddlePRG_1.fbAGS_1.ptrPointerTest");

            var child = tag.Childs.First();
            child.Should().NotBeNull("should read pointer to T_PointerTest");
            child.Childs.Count.Should().Be(3);
            child.Childs.Any(t => t.NestedName == "bolPrintOk").Should().BeTrue();
            child.Childs.Any(t => t.NestedName == "lreActTrayPos_mm").Should().BeTrue();
            child.Childs.Any(t => t.NestedName == "eState").Should().BeTrue();
        }

        [Test]
        public void GivenNoMetaData_WhenReadPointer_ShouldBeEmpty()
        {
            var tag = _tagImporter.ImportTagRecursive("MiddlePRG_1.fbAGS_1.ptrPointer2Test");

            tag.Childs.Should().BeEmpty();
        }

        // fbAGS_1.ptrCircularPointerTest <-----> fbAGS_2.ptrCircularPointerTest
        [Test, Timeout(2000)]
        public void GivenCircularPointer_WhenReadPointer_ShouldStopImportWhenDetectAlreadyRead()
        {
            var tag = _tagImporter.ImportTagRecursive("MiddlePRG_1.fbAGS_1.ptrCircularPointerTest");

            tag.Childs.Should().ContainSingle("must read first pointer to fbAGS_2");
            Tag backPointerTag = tag.Childs.First().Childs.FirstOrDefault(c => c.NestedName == "ptrCircularPointerTest");
            backPointerTag.Childs.Should().BeEmpty("must not pointer back to already read fbAGS_1");
        }

        [Test]
        public void GivenEnum_WhenReadTag_ThenParseCorrectly()
        {
            // {enum 0=Empty;1=Stop;2=Load;3=Pick;4=Print}
            var tag = _tagImporter.ImportTagRecursive("MiddlePRG_1.fbAGS_1.MetadataTest.eEnumTest");

            tag.MetaData.EnumerationMembers.Should().HaveCount(5);
            tag.MetaData.Minimum.Should().Be((short) 0, "0 is the smalled enum value");
            tag.MetaData.Maximum.Should().Be((short) 4, "4 is the biggest enum value");
            tag.MetaData.EnumerationMembers[0].Value.Should().Be(0);
            tag.MetaData.EnumerationMembers[0].Comment.Should().Be("Empty");
            tag.MetaData.EnumerationMembers[1].Value.Should().Be(1);
            tag.MetaData.EnumerationMembers[1].Comment.Should().Be("Stop");
            tag.MetaData.EnumerationMembers[2].Value.Should().Be(2);
            tag.MetaData.EnumerationMembers[2].Comment.Should().Be("Load");
            tag.MetaData.EnumerationMembers[3].Value.Should().Be(3);
            tag.MetaData.EnumerationMembers[3].Comment.Should().Be("Pick");
            tag.MetaData.EnumerationMembers[4].Value.Should().Be(4);
            tag.MetaData.EnumerationMembers[4].Comment.Should().Be("Print");
        }

        [Test]
        public void GivenEnumWithNegativesValues_WhenReadTag_ThenParseCorrectly()
        {
            /*
                {enum
			        -1=Negative;
			        0=Zero;
			        1=One;}
            */
            var tag = _tagImporter.ImportTagRecursive("MiddlePRG_1.fbAGS_1.MetadataTest.eEnumWithNegativesTest");

            tag.MetaData.EnumerationMembers.Should().HaveCount(3);
            tag.MetaData.Minimum.Should().Be((short)-1, "-1 is the smalled enum value");
            tag.MetaData.Maximum.Should().Be((short)1, "1 is the biggest enum value");
            tag.MetaData.EnumerationMembers[0].Value.Should().Be(-1);
            tag.MetaData.EnumerationMembers[0].Comment.Should().Be("Negative");
            tag.MetaData.EnumerationMembers[1].Value.Should().Be(0);
            tag.MetaData.EnumerationMembers[1].Comment.Should().Be("Zero");
            tag.MetaData.EnumerationMembers[2].Value.Should().Be(1);
            tag.MetaData.EnumerationMembers[2].Comment.Should().Be("One");
        }
    }
}
