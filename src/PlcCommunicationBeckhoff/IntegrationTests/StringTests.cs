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
using VP.FF.PT.Common.Infrastructure.Concurrency;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlcCommunication;

namespace VP.FF.PT.Common.PlcCommunicationBeckhoff.IntegrationTests
{
    [TestFixture]
    [Category("PlcIntegrationTest")]
    public class StringTests
    {
        private ITagController _tagController;
        private ITagListener _tagListener;
        private readonly Tag _defaultStringTestTag = new Tag("strDefaultString", "Test", "STRING(80)");

        [SetUp]
        public void SetUp()
        {
            var logger = new Log4NetLogger();
            _tagController = new BeckhoffTagController(Global.AdsAddress, Global.AdsPort, logger);
            _tagListener = new BeckhoffPollingTagListener(Global.AdsAddress, Global.AdsPort, new GlobalLock(), logger);

            _tagController.StartConnection();
            _tagListener.StartListening();
        }

        [TearDown]
        public void TearDown()
        {
            _tagController.Dispose();
            _tagListener.Dispose();
        }

        [Test]
        public void Test_WriteAndReadDefaultString()
        {
            _tagController.WriteTag(_defaultStringTestTag, "123456789abcdefghijklmnopqrstuvwxyz").Wait();

            _tagListener.ReadTagSynchronously(_defaultStringTestTag);
            _defaultStringTestTag.Value.ToString().Should().Be("123456789abcdefghijklmnopqrstuvwxyz");
        }

        [Test]
        public void Test_WriteAndReadShortString()
        {
            _tagController.WriteTag(_defaultStringTestTag, "123").Wait();

            _tagListener.ReadTagSynchronously(_defaultStringTestTag);
            _defaultStringTestTag.Value.ToString().Should().Be("123");
        }

        [Test]
        public void Test_WriteAndReadLongString()
        {
            _tagController.WriteTag(_defaultStringTestTag, "01234567890123456789012345678901234567890123456789012345678901234567890123456789").Wait();

            _tagListener.ReadTagSynchronously(_defaultStringTestTag);
            _defaultStringTestTag.Value.ToString().Should().Be("01234567890123456789012345678901234567890123456789012345678901234567890123456789");
        }

        [Test]
        public void Test_WriteAndReadCustomShortString()
        {
            var shortStringTestTag = new Tag("strShortString", "Test", "STRING(5)");
            _tagController.WriteTag(shortStringTestTag, "12345").Wait();

            _tagListener.ReadTagSynchronously(shortStringTestTag);
            shortStringTestTag.Value.ToString().Should().Be("12345");
        }

        [Test]
        public void Test_WriteAndReadCustomLongString()
        {
            var longStringTestTag = new Tag("strLongString", "Test", "STRING(160)");
            _tagController.WriteTag(longStringTestTag, "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789").Wait();

            _tagListener.ReadTagSynchronously(longStringTestTag);
            longStringTestTag.Value.ToString().Should().Be("0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789");
        }
    }
}
