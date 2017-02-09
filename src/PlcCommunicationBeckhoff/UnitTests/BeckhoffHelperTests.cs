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
using FluentAssertions;
using NUnit.Framework;
using TwinCAT.Ads;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class BeckhoffHelperTests
    {
        [Test]
        public void WhenBooleanDataType_ShouldReadBool()
        {
            var buffer = new byte[] { 1 };
            var stream = new AdsStream(buffer);
            var binaryReader = new AdsBinaryReader(stream);

            var result = BeckhoffHelper.ReadDataType("BOOL", 8, binaryReader);

            result.GetType().Should().Be(typeof (bool));
            ((bool) result).Should().BeTrue();
        }

        [Test]
        public void WhenStringDataType_ShouldReadString()
        {
            var buffer = new[] { (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o' };
            var stream = new AdsStream(buffer);
            var binaryReader = new AdsBinaryReader(stream);

            var result = BeckhoffHelper.ReadDataType("STRING", 81, binaryReader);

            result.GetType().Should().Be(typeof (string));
            ((string) result).Should().Be("hello");
        }

        [Test]
        public void WhenDefaultEnumDataType_ShouldRead()
        {
            var buffer = new byte[] { 1, 2 };
            var stream = new AdsStream(buffer);
            var binaryReader = new AdsBinaryReader(stream);

            var result = BeckhoffHelper.ReadDataType("E_MyTestEnum", 16, binaryReader);

            result.GetType().Should().Be(typeof (Int16), "int16 is TwinCat 3 default for enumerations, negative values are allowed");
        }

        [Test]
        public void GivenNestedTag_WhenDefaultEnumDataType_ShouldRead()
        {
            var buffer = new byte[] { 1, 2 };
            var stream = new AdsStream(buffer);
            var binaryReader = new AdsBinaryReader(stream);

            var result = BeckhoffHelper.ReadDataType("MAIN.FB_1.E_MyTestEnum", 16, binaryReader);

            result.GetType().Should().Be(typeof(Int16), "int16 is TwinCat 3 default for enumerations, negative values are allowed");
        }

        [Test]
        public void WhenEnumDataTypeWithWrongNamingConvention_ShouldNotCrash()
        {
            var buffer = new byte[] { 1, 2, 3 };
            var stream = new AdsStream(buffer);
            var binaryReader = new AdsBinaryReader(stream);

            var act = new Action( () => BeckhoffHelper.ReadDataType("MyTestEnumDoesNotFollowNamingConvention", 16, binaryReader));

            act.ShouldNotThrow();
        }

    }
}
