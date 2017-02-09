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
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunicationLogix.IngearImplementation;

namespace VP.FF.PT.Common.PlcCommunicationLogix.UnitTests
{
    [TestFixture]
    public class ParseLogixDataTypeTests
    {
        [Test]
        public void WhenParsingBool_ThenLogixBoolIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("BOOL");
            parsedType.Should().Be(Logix.Tag.ATOMIC.BOOL);
        }

        [Test]
        public void WhenParsingInt_ThenLogixIntIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("SINT");
            parsedType.Should().Be(Logix.Tag.ATOMIC.SINT);
        }

        [Test]
        public void WhenParsingDInt_ThenLogixIntIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("DINT");
            parsedType.Should().Be(Logix.Tag.ATOMIC.DINT);
        }

        [Test]
        public void WhenParsingLInt_ThenLogixIntIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("LINT");
            parsedType.Should().Be(Logix.Tag.ATOMIC.LINT);
        }

        [Test]
        public void WhenParsingReal_ThenLogixIntIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("REAL");
            parsedType.Should().Be(Logix.Tag.ATOMIC.REAL);
        }

        [Test]
        public void WhenParsingString_ThenLogixIntIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("STRING");
            parsedType.Should().Be(Logix.Tag.ATOMIC.STRING);
        }

        [Test]
        public void WhenParsingBoolInAnyCasing_ThenLogixBoolIsReturned()
        {
            var parsedTypeLowerCase = IngearHelper.ParseNetLogixDataType("bool");
            var parsedTypeMixedCase = IngearHelper.ParseNetLogixDataType("Bool");
            
            parsedTypeLowerCase.Should().Be(Logix.Tag.ATOMIC.BOOL);
            parsedTypeMixedCase.Should().Be(Logix.Tag.ATOMIC.BOOL);
        }

        [Test]
        public void WhenParsingUnknownString_ThenLogixObjectIsReturned()
        {
            var parsedType = IngearHelper.ParseNetLogixDataType("123123_this is nonsense_blub_moooo");
            parsedType.Should().Be(Logix.Tag.ATOMIC.OBJECT);
        }
    }
}
