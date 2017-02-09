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


using SystemWrapper.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    [TestFixture]
    [Ignore("Not implemented")]
    public class LogixCsvTagImporterTests
    {
        private ITagImporter _testee;

        [SetUp]
        public void SetUp()
        {
            var streamReaderMock = new Mock<IStreamReaderWrap>();

            SimulatedLogixCsvFile.Reset();

            streamReaderMock
                .Setup(streamReader => streamReader.ReadLine())
                .Returns(SimulatedLogixCsvFile.GetNextLine);
                
            CsvLogixTagImporter.StreamReader = streamReaderMock.Object; 

            _testee = new CsvLogixTagImporter();
        }

        [Test]
        public void WhenReadCsvFile_ResultMustContainTag()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Should().Contain(c => 
                c.Type == TagType.Tag && 
                c.Scope.Equals(string.Empty) &&
                c.Name.Equals("DistributedEthernetIO:1:C"),
                    "it is defined in simulated csv test data");
        }

        [Test]
        public void WhenReadCsvFile_ResultMustContainAlias()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Should().Contain(c =>
                c.Type == TagType.Alias &&
                c.Scope.Equals(string.Empty) &&
                c.Name.Equals("test_alias_tag"),
                    "it is defined in simulated csv test data");
        }

        [Test]
        public void WhenReadCsvFile_StringsInQuotesMustNotBeSplitted()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Should().Contain(c =>
                c.Attributes.Equals("(RADIX := Decimal, ExternalAccess := Read/Write)"),
                    "parser must not split strings in quotes, even if its comma separated");
        }

        [Test]
        public void WhenReadCsvFile_ResultMustNotContainNull()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Should().NotContainNulls();
        }

        [Test]
        // Also WhenReadCsvFile_MustIgnoreRowsWithTypeDescription
        public void WhenReadCsvFile_ResultMustContain_2_Tags()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Count.Should().Be(2, "it is defined in simulated csv test data");
        }

        [Test]
        public void WhenReadCsvFile_ResultMustNotContainHeaderInformations()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Should().NotContain(c => c.Type.Equals("TYPE"), "metadata must not be parsed");
            tags.Should().NotContain(c => c.Type.Equals("SCOPE"), "metadata must not be parsed");
            tags.Should().NotContain(c => c.Type.Equals("NAME"), "metadata must not be parsed");
            tags.Should().NotContain(c => c.Type.Equals("DESCRIPTION"), "metadata must not be parsed");
            tags.Should().NotContain(c => c.Type.Equals("DATATYPE"), "metadata must not be parsed");
            tags.Should().NotContain(c => c.Type.Equals("SPECIFIER"), "metadata must not be parsed");
            tags.Should().NotContain(c => c.Type.Equals("ATTRIBUTES"), "metadata must not be parsed");
        }

        [Test]
        public void WhenReadCsvFile_MustIgnoreComments()
        {
            var tags = _testee.ImportTags(It.IsAny<string>());

            tags.Should().NotContain(c => c.Type == TagType.Comment);
        }
    }
}
