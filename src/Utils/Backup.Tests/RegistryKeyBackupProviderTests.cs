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


using Microsoft.Win32;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Utils.Backup.UnitTests
{
    [TestFixture]
    public class RegistryKeyBackupProviderTests
    {
        private RegistryKeyBackupProvider _registryKeyBackupProvider;
        private Mock<IFileStore> _fileStoreMock;
        private Mock<ILogger> _loggerMock;
        private Dictionary<string, string> _keysToMonitor;

        // I am assuming keys in HK_CURRENT_USER, and one level depth
        // no reason to test in a different scenario as the purpose of the RegistryKeyBackupProvider is to
        // monitor changes in a registry key subtree and not create/delete complex tree structures 
        private static readonly string _regKeyToMonitor1 = "firstKey";
        private static readonly string _regKeyToMonitor2 = "secondKey";
        private static readonly string _regKey2Identifier = "myRegKeyID";

        [SetUp]
        public void SetUp()
        {
            _fileStoreMock = new Mock<IFileStore>();
            _loggerMock = new Mock<ILogger>();
            _keysToMonitor = GetRegistryKeysToMonitor();

            CreateTestKeys(_keysToMonitor);

            _registryKeyBackupProvider = new RegistryKeyBackupProvider(_fileStoreMock.Object, _loggerMock.Object);
            _registryKeyBackupProvider.Initialize(_keysToMonitor);
            _registryKeyBackupProvider.StartMonitoring();
        }

        [TearDown]
        public void TearDown()
        {
            RemoveTestKeys(_keysToMonitor);
            _registryKeyBackupProvider.StopMonitoring();
            _registryKeyBackupProvider.Dispose();
        }

        private void CreateTestKeys(Dictionary<string, string> keysToMonitor)
        {
            foreach (var regKey in keysToMonitor.Keys)
            {
                var subKey = Registry.CurrentUser.CreateSubKey(StripRegistryHive(regKey));
                if (subKey != null)
                {
                    subKey.SetValue("some value", "some content");
                    subKey.Close();
                }
            }
        }

        private string StripRegistryHive(string regKey)
        {
            int indexOfSeparator = regKey.IndexOf("\\", StringComparison.Ordinal);

            if (indexOfSeparator > 0)
            {
                return regKey.Substring(indexOfSeparator + 1);
            }
            return regKey;
        }

        private void RemoveTestKeys(Dictionary<string, string> keysToMonitor)
        {
            foreach (var regKey in keysToMonitor.Keys)
            {
                Registry.CurrentUser.DeleteSubKeyTree(StripRegistryHive(regKey));
            }
        }

        [Test, Category("Integration")] 
        public void SetUpRegistryMonitor_ChangeRegKeys_VerifyVcsPersistence()
        {
            ChangeTestKey(_regKeyToMonitor1);

            Thread.Sleep(TimeSpan.FromSeconds(2)); // we should not need to give time to the monitor to be triggered, but I would
            // leave this commented out for the moment to remind that the monitoring happens
            // on a different thread

            string expectedIdentifier = "HKEY_CURRENT_USER_" + _regKeyToMonitor1;
            string expectedFilePath = string.Format(@"{0}{1}.reg", Path.GetTempPath(), _regKeyToMonitor1);

            // verify mocks
            _fileStoreMock.Verify(x => x.SaveFile(expectedIdentifier, expectedFilePath), Times.Once());
        }

        [Test, Category("Integration")] 
        public void SetUpRegistryMonitor_ChangeRegKeys_EnsureIdentifierIsUsed()
        {
            ChangeTestKey(_regKeyToMonitor2);

            Thread.Sleep(TimeSpan.FromSeconds(2));

            string expectedFilePath = string.Format(@"{0}{1}.reg", Path.GetTempPath(), _regKeyToMonitor2);

            // verify mock
            _fileStoreMock.Verify(x => x.SaveFile(_regKey2Identifier, expectedFilePath), Times.Once());
        }

        private void ChangeTestKey(string registryKey)
        {
            Console.WriteLine("Changing key {0}", registryKey);
            var openSubKey = Registry.CurrentUser.OpenSubKey(registryKey, true);
            if (openSubKey != null)
            {
                openSubKey.SetValue("testKey", "testValue " + DateTime.Now);
            }
        }

        private Dictionary<string, string> GetRegistryKeysToMonitor()
        {
            return new Dictionary<string, string>
            {
                {@"HKEY_CURRENT_USER\" + _regKeyToMonitor1, null},
                {@"HKEY_CURRENT_USER\" + _regKeyToMonitor2, _regKey2Identifier}
            };
        }
    }
}
