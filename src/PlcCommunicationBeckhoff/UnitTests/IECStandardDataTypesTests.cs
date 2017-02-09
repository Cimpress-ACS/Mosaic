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
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class IECStandardDataTypesTests
    {
        [Test(Description = "This test ensures that necessary informations are available when extending the IEC61131_3_DataTypes class structure.")]
        public void BitSizes_NetDataTypes_ListsMustHaveSameLength()
        {
            IEC61131_3_DataTypes.BitSizes.Count.Should().Be(IEC61131_3_DataTypes.NetDataTypes.Count, "it could be that someone did a wrong refactoring when extending IEC61131_3_DataTypes");
        }

        [Test(Description = "This test ensures that necessary informations are available when extending the IEC61131_3_DataTypes class structure.")]
        public void DataTypeMemberCount_MustBeSameAsNetDataTypesListCount()
        {
           int dataTypesCount = (from field in typeof (IEC61131_3_DataTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
                                  where field.FieldType == typeof (string)
                                  select field).Count();

            dataTypesCount.Should().Be(IEC61131_3_DataTypes.NetDataTypes.Count, "it could be that someone did a wrong refactoring when extending IEC61131_3_DataTypes");
        }
    }
}
