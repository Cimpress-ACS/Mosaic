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
using NUnit.Framework;
using FluentAssertions;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class BeckhoffTagExtensionsTests
    {
        [Test]
        public void FullName_ShouldReturnScopeAndName()
        {
            var tag = new Tag { Name = "SensorB", Scope = "ConveyorA" };

            var result = tag.FullName();

            result.Should().Be("ConveyorA.SensorB", "when scope is given the full access path to tag is expected");
        }

        [Test]
        public void ArrayValue_ShouldReturnRightValue()
        {
            var plcArray = new object[] { 1, 2 };
            var tag = new Tag {Value = plcArray};

            var value = tag.ArrayValue<int>(0);

            value.Should().Be(1);
        }

        [Test]
        public void ArrayValues_ShouldReturnArray()
        {
            var plcArray = new object[] { 1, 2 };
            var tag = new Tag { Value = plcArray };

            var array = tag.ArrayValues<int>();

            array.Should().HaveCount(2);
            array[0].Should().Be(1);
            array[1].Should().Be(2);
        }

        /// <summary>
        /// Becuase tag.ArrayValue<myDynamicType>(4) is not possible we use reflection to solve this.
        /// See BeckhoffTagController as an example.
        /// </summary>
        [Test]
        public void Reflection_ArrayValue()
        {
            var tag = new Tag
                {
                    DataType = "ARRAY [0..9] OF DINT",
                    Value = new object[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
                };
            Type type = typeof (int);

            var test = typeof (Tag)
                .GetMethod("ArrayValue")
                .MakeGenericMethod(type)
                .Invoke(tag, new object[] {4});

            test.Should().Be(4);
        }

        [Test]
        public void TestPointerlessFullName()
        {
            var tag = new Tag("fbAGS_1.ptrCircularPointerTest^.ptrCircularPointerTest", "MiddlePRG");

            tag.GetPointerlessFullName().Should().Be("MiddlePRG.fbAGS_1.ptrCircularPointerTest");
        }

        [Test]
        public void TestPointerlessComplexFullName()
        {
            var tag = new Tag("a.bc^.de.f.gh^.ij^.kl.m", string.Empty);

            tag.GetPointerlessFullName().Should().Be("a.de.f.kl.m");
        }

    }
}
