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
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    class TagArrayTests
    {
        [Test]
        public void WhenDataTypeIsArrayOfINT_ItShouldCalculateCorrectBitSize()
        {
            var tag = new Tag { DataType = "ARRAY [0..9] OF BYTE" };

            tag.BitSize.Should().Be(10 * IEC61131_3_DataTypes.BitSizes[IEC61131_3_DataTypes.Byte],
                                    "ARRAY of 10 BYTEs with size of 8 is 10*8 = 80");
        }

        [Test]
        public void WhenDataTypeIsArrayOfLREAL_ItShouldCalculateCorrectBitSize()
        {
            var tag = new Tag { DataType = "ARRAY[2..5]OF LREAL" };

            tag.BitSize.Should().Be(4 * IEC61131_3_DataTypes.BitSizes[IEC61131_3_DataTypes.LReal],
                                    "ARRAY of 4 LREALSs with size of 32 is 4*32 = 128");
        }

        [Test]
        public void WhenDataTypeIsArrayWithWrongBounds_ItShouldThrowAnException()
        {
            var tag = new Tag();

            Action act = () => tag.DataType = "ARRAY[5..2] OF BYTE";

            act.ShouldThrow<TagException>()
                .Where(t => t.Tag == tag);
        }

        [Test]
        public void WhenDataTypeIsArrayWithUnknownType_BitSizeShouldBeNegative()
        {
            var tag = new Tag();

            tag.DataType = "ARRAY[0..9] OF UnknowsDataTypeTest";

            tag.BitSize.Should().Be(-1);
        }

        [Test]
        public void WhenDataTypeIsArrayWithUnknownNextedType_BitSizeShouldBeNegative()
        {
            var tag = new Tag();

            tag.DataType = tag.DataType = "ARRAY[0..9] OF Unknown.NestedDataTypeTest";

            tag.BitSize.Should().Be(-1);
        }
    }
}
