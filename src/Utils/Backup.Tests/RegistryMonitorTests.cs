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
using System.Threading;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Utils.Backup.UnitTests
{
    [TestFixture]
    public class RegistryMonitorTests
    {
        private RegistryMonitor _registryMonitor;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Test]
        public void CreateRegistryKey_ChangeIt_VerifyChangeTriggered()
        {
            string subKey = "whocares";
            string keyName = "someStuff";
            string keyValue = "key value";

            AutoResetEvent keyChangedEvent = new AutoResetEvent(false);
            bool signalled = false;

            try
            {
                string regKey = CreateNewRegistryKey(subKey, keyName, keyValue);

                Assert.IsNotNull(regKey);
                _registryMonitor = new RegistryMonitor(regKey, _loggerMock.Object);

                _registryMonitor.RegChanged += (sender, args) =>
                {
                    keyChangedEvent.Set();
                    Console.WriteLine("Key changed " + DateTime.Now);
                };

                _registryMonitor.Start();

                Thread.Sleep(2000); // Give some time to the monitoring thread to start

                ModifyRegistryKey(subKey, keyName, "new" + keyValue);

                signalled = keyChangedEvent.WaitOne(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected exception: " + ex.Message);
            }
            finally
            {
                RemoveRegistryKey(subKey);
            }
            Assert.IsTrue(signalled, "The reg key didn't change");
        }

        [Test]
        public void MonitorRegKeyInLocalMachine()
        {
            _registryMonitor = new RegistryMonitor(@"HKEY_LOCAL_MACHINE\SOFTWARE\DJM", _loggerMock.Object);

            _registryMonitor.Start();
            _registryMonitor.Stop();

            Assert.IsNotNull(_registryMonitor);
        }

        private void RemoveRegistryKey(string subKey)
        {
            Registry.CurrentUser.DeleteSubKeyTree(subKey);
        }

        private void ModifyRegistryKey(string subKey, string keyName, string keyValue)
        {
            var openSubKey = Registry.CurrentUser.OpenSubKey(subKey, true);
            if (openSubKey != null)
            {
                openSubKey.SetValue(keyName, keyValue, RegistryValueKind.String);
            }
        }

        private string CreateNewRegistryKey(string subKey, string keyName, string keyValue)
        {
            var key = Registry.CurrentUser.CreateSubKey(subKey);
            if (key != null)
            {
                key.SetValue(keyName, keyValue);
                string completeKeyName = key.Name;
                key.Close();
                return completeKeyName;
            }
            return null;
        }
    }
}
