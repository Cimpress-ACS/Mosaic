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
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class BeckhoffTagImporterTests_GlobalVariablesTestExportFile
    {
        private ITagImporter _testee;
        private ICollection<Tag> _importedTagList;

        [SetUp]
        public void Setup()
        {
            _testee = new BeckhoffXmlTagImporter();
            _importedTagList = _testee.ImportTags("GlobalVariables_TestExportFile.xml", port:0);
        }

        [TearDown]
        public void TearDown()
        {
            _importedTagList = null;
        }

        [Test]
        public void GivenTestFileImported_TagListShouldContain3Tags()
        {
            _importedTagList.Should().HaveCount(3, 
                "Test Export File contains 3 Tags. 2 In_ and 1 Out_ Tag");
        }

        [Test]
        public void GivenTestFileImported_InEmergencyStopTagShouldBeAvailable()
        {
            _importedTagList.Should().Contain(
                tag => tag.Name == "In_EmergencyStop");
        }

        [Test]
        public void GivenTestFileImported_InMotorFailureTagShouldBeAvailable()
        {
            _importedTagList.Should().Contain(
                tag => tag.Name == "In_MotorFailure");
        }

        [Test]
        public void GivenTestFileImported_OutMotorTlsShouldBeAvailable()
        {
            _importedTagList.Should().Contain(
                tag => tag.Name == "Out_MotorTls");
        }

        /// <summary>
        /// The specifier property of tag is in case of Beckhoff implementatin equals hardware address
        /// </summary>
        [Test]
        public void GivenTestFileImported_InEmergencyStopTagShouldHaveCorrectScopedDataTypeAndSpecifier()
        {
            var tag = (from t in _importedTagList
                       where t.Name == "In_EmergencyStop"
                       select t).First();

            tag.Scope.Should().Be("GlobalVariables", 
                "this is specified by the Test Export File");

            tag.Specifier.Should().Be("%I*",
                "this is specified by the Test Export File");

            tag.DataType.Should().Be("BOOL",
                "this is specified by the Test Export File");
        }

        [Test]
        public void WhenWrongPath_ShouldThrowTagReaderException()
        {
            Action action = () => _importedTagList = _testee.ImportTags("invalid_file_does_not_exist_test.xml", port:0);

            action.ShouldThrow<TagReaderException>().WithInnerException<FileNotFoundException>();
        }
    }

    [TestFixture]
    public class BeckhoffTagImporterTests_ForcingLayerTestExportFile
    {
        private ITagImporter _testee;
        private ICollection<Tag> _importedTagList;

        [SetUp]
        public void Setup()
        {
            _testee = new BeckhoffXmlTagImporter();
            _importedTagList = _testee.ImportTags("ForcingLayer_TestExportFile.xml", port:0);
        }

        [TearDown]
        public void TearDown()
        {
            _importedTagList = null;
        }

        [Test]
        public void GivenTestFileImported_TagListShouldContain10Tags()
        {
            _importedTagList.Should().HaveCount(10,
                                                "Test Export File contains 10 Tags. 1 EnableForcing, 3*3=9 Tags for forcing In_EmergencyStop, In_MotorFailure and Out_MotorTls");
        }

        [Test]
        public void GivenTestFileImported_EnableForcingTagShouldBeAvailable()
        {
            _importedTagList.Should().Contain(
                tag => tag.Name == "EnableForcing");
        }

        /// <summary>
        /// The specifier property of tag is in case of Beckhoff implementatin equals hardware address
        /// </summary>
        [Test]
        public void GivenTestFileImported_EnableForcingTagShouldHaveCorrectScopedDataTypeAndSpecifier()
        {
            var tag = (from t in _importedTagList
                       where t.Name == "EnableForcing"
                       select t).First();

            tag.Scope.Should().Be("ForcingLayer",
                "this is specified by the Test Export File");

            tag.Specifier.Should().Be(null,
                "there is no hardware mapping defined for this variable (no %I* or %Q*)");

            tag.DataType.Should().Be("BOOL",
                "this is specified by the Test Export File");
        }
    }
}
