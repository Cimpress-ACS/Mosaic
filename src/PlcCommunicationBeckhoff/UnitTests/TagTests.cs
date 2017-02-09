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


using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using VP.FF.PT.Common.PlcCommunication;
using VP.FF.PT.Common.PlcCommunication.Infrastructure;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.UnitTests
{
    [TestFixture]
    public class TagTests
    {
        [Test]
        public void Equals_WhenTagsHaveSameScopeAndNameAndAdsPort_ItIsEqual()
        {
            var tagA = new Tag {Name = "SensorA", Scope = "ProgramA", AdsPort = 850};
            var tagB = new Tag {Name = "SensorA", Scope = "ProgramA", AdsPort = 850};

            tagA.Equals(tagB).Should().BeTrue("tags have same scope and names");
        }

        [Test]
        public void Equals_WhenTagsSameNameOnly_ItIsNotEqual()
        {
            var tagA = new Tag {Name = "SensorA", Scope = "ProgramB"};
            var tagB = new Tag {Name = "SensorA"};

            tagA.Equals(tagB).Should().BeFalse("tags have same names but different scopes");
        }

        [Test]
        public void Equals_WhenTagsHaveDifferentPorts_ItIsNotEqual()
        {
            var tagA = new Tag { Name = "SensorA", Scope = "ProgramB", AdsPort = 850};
            var tagB = new Tag { Name = "SensorA", Scope = "ProgramB", AdsPort = 851};

            tagA.Equals(tagB).Should().BeFalse("tags have different ADS ports");
        }

        [Test]
        public void OperatorEqual_WhenTagsHaveSameScopeAndNameAndAdsPort_ItShouldBeTrue()
        {
            var tagA = new Tag { Name = "SensorA", Scope = "ProgramA", AdsPort = 850};
            var tagB = new Tag { Name = "SensorA", Scope = "ProgramA", AdsPort = 850};

            bool equal = tagA == tagB;

            equal.Should().BeTrue("tags have same scope and names");
        }

        [Test]
        public void OperatorNotEqual_WhenTagsHaveSameScopeAndName_ItShouldBeFalse()
        {
            var tagA = new Tag { Name = "SensorA", Scope = "ProgramA" };
            var tagB = new Tag { Name = "SensorA", Scope = "ProgramA" };

            bool equal = tagA != tagB;

            equal.Should().BeFalse("tags have same scope and names");
        }

        [Test]
        public void GivenNoDataType_WhenSetDataTypeBool_ItShouldDetectBitSizeAutomatically()
        {
            var tag = new Tag {DataType = "BOOL"};

            tag.BitSize.Should().Be(8);
        }

        [Test]
        public void GivenNoDataType_WhenSetDataTypeToDINT_ItShouldDetectBitSizeAutomatically()
        {
            var tag = new Tag {DataType = "DINT"};

            tag.BitSize.Should().Be(32);
        }

        [Test]
        public void GivenNoDataType_WhenSetDataTypeToSTRING_ItShouldDetectBitSizeAutomatically()
        {
            var tag = new Tag {DataType = "STRING"};

            tag.BitSize.Should().Be(8 * (80 + 1), "tag.Value is \"hello\" which means by default 80 characters * 8 bit plus 1 for termination");
        }

        [Test]
        public void GivenStringDataType_WhenChangeBitSize_ItShouldRetainChange()
        {
            const int numberOfCharacters = 200;
            var tag = new Tag {DataType = "STRING"};

            tag.BitSize = (numberOfCharacters + 1) * 8;
            tag.Value = "test";

            tag.BitSize.Should().Be((numberOfCharacters + 1)*8, "bit size was set manually to 200 + 1 characters");
        }

        [Test]
        public void GivenNoDataTypeAndNoValue_WhenSetDataTypeToSTRING_ItShouldDetectBitSizeAutomatically()
        {
            var tag = new Tag("name", "scope", "STRING");

            tag.BitSize.Should().Be((80 + 1) * 8, "default string length is 80 + 1 for termination character");
        }

        [Test]
        public void GivenCustomDataType_WhenSetBitSize_ItShouldReturnMyBitSize()
        {
            var tag = new Tag {DataType = "MyCustomDataTypeTest"};

            tag.BitSize = 99;
            tag.Value = new List<int>(5);

            tag.BitSize.Should().Be(99);
        }

        [Test]
        public void GivenDataTypeAlreadySet_WhenSetValue_DoNotAutomaticallyDetectDatatype()
        {
            var tag = new Tag { DataType = "myCustomUDT" };

            tag.Value = true;

            tag.DataType.Should().Be("myCustomUDT", "manually set DataType should not be overriden automatically");
        }

        [Test]
        public void Event_WhenValueChanged_ShouldRaiseTagValueChangedEvent()
        {
            var testee = new Tag("tag", "scope");
            testee.MonitorEvents();

            testee.Value = true;

            testee
                .ShouldRaise("ValueChanged")
                .WithSender(testee)
                .WithArgs<TagValueChangedEventArgs>(a => (bool)a.Value == true);
        }

        [Test]
        public void Event_GivenValueAlreadySet_WhenSetValueAgain_ShouldNotRaiseEvent()
        {
            var testee = new Tag("tag", "scope");
            testee.Value = true;
            testee.MonitorEvents();

            testee.Value = true;

            testee
                .ShouldNotRaise("ValueChanged", "it must improve performance");
        }

        [Test]
        public void DetectDataTypeForValue_WhenCallWithBool_DataTypeMustBeBOOL()
        {
            var testee = new Tag();

            testee.DetectDataTypeForValue(true);

            testee.DataType.Should().Be(IEC61131_3_DataTypes.Boolean);
        }

        [Test]
        public void DetectDataTypeForValue_WhenCallWithString_DataTypeMustBeSTRING()
        {
            var testee = new Tag();

            testee.DetectDataTypeForValue("this is a string value");

            testee.DataType.Should().Be(IEC61131_3_DataTypes.String);
        }

        [Test]
        public void GivenFixedStringLength_ItMustDetectBitsizeCorrectly()
        {
            var testee = new Tag("name", "scope", "STRING (30)");

            testee.Value = "test";

            testee.BitSize.Should().Be((30 + 1) * 8, "there are 30 characters +1 for null termination multiplied with 8 (for byte)");
        }

        [Test]
        public void GivenNoStringLength_ItShouldUseBeckhoffDefault()
        {
            var testee = new Tag();

            testee.DetectDataTypeForValue("this is a string value");

            testee.BitSize.Should().Be((80 + 1) * 8, "Beckhoff strings has 80 characters +1 null termination as default");
        }

        [Test]
        public void GivenFixedStringLength_ItMustNotChange()
        {
            var testee = new Tag("name", "scope", "STRING (40)");

            testee.Value = "test";

            testee.DataType.Should().Be("STRING (40)");
        }

        [Test]
        public void GivenTagPath_Name_ReturnsTheWholePath()
        {
            var testee = new Tag("MasterController.ChildController.MyVar", "Scope");

            testee.Name.Should().Be("MasterController.ChildController.MyVar");
        }

        [Test]
        public void GivenTagPath_NestedName_ReturnsOnlyVariableName()
        {
            var testee = new Tag("MasterController.ChildController.MyVar", "Scope");

            testee.NestedName.Should().Be("MyVar");
        }

        [Test]
        public void WhenValueIsStillNull_MustNotThrowValueChangedEvent()
        {
            var testee = new Tag();
            testee.MonitorEvents();

            testee.Value = null;

            testee.ShouldNotRaise("ValueChanged", "old and new value is still null");
        }

        [Test]
        public void WhenValueIsNullAgain_MustThrowValueChangedEvent()
        {
            var testee = new Tag();
            testee.Value = true;
            testee.MonitorEvents();

            testee.Value = null;

            testee.ShouldRaise("ValueChanged");
        }

        [Test]
        public void NewTag_MustNotBeInitialized()
        {
            var testee = new Tag();

            testee.IsInitialized.Should().BeFalse();
        }
    }
}
