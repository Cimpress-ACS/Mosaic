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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Utils.Backup
{
    public class RegistryKeyBackupProvider : IBackupProvider, IDisposable
    {
        private readonly IFileStore _fileStore;
        private readonly ILogger _logger;
        private Dictionary<string, string> _registryKeysToMonitor;
        private readonly Dictionary<string, RegistryMonitor> _regKeyMonitors;
        private readonly Object _syncRoot = new Object();

        public RegistryKeyBackupProvider(IFileStore fileStore, ILogger logger)
        {
            if (fileStore == null) throw new ArgumentNullException("fileStore");
            if (logger == null) throw new ArgumentNullException("logger");

            _fileStore = fileStore;
            _logger = logger;

            _registryKeysToMonitor = new Dictionary<string, string>();
            _regKeyMonitors = new Dictionary<string, RegistryMonitor>();

            _logger.Init(typeof(RegistryKeyBackupProvider));
            IsMonitoring = false;
        }

        #region Private methods

        private void OnRegistryKeyChanged(object sender, EventArgs e)
        {
            // ensure that we can get the information about the registry key and save it
            RegistryMonitor regMon = sender as RegistryMonitor;
            if (regMon != null)
            {
                SaveRegistryKey(regMon.RegistryKey);
            }
        }

        private void SaveRegistryKey(string registryKey)
        {
            // Get the information relevant to the changed key and store it
            string filePath = GetTemporaryFilePath(registryKey);

            try
            {
                ExportKey(registryKey, filePath);

                if (!File.Exists(filePath))
                {
                    throw new ApplicationException("Cannot find the file to back up");
                }
                string key;
                if (!_registryKeysToMonitor.TryGetValue(registryKey, out key)
                    || string.IsNullOrWhiteSpace(key))
                {
                    key = GetIdentifierFromKey(registryKey);
                }
                _fileStore.SaveFile(key, filePath);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Failed to save exported registry key {0} in file {1}. Error: {2}",
                    registryKey, filePath, ex.Message);
            }
        }

        private string GetIdentifierFromKey(string registryKey)
        {
            return registryKey.Replace(" ", "_").Replace("/", "_").Replace("\\", "_");
        }

        private string GetTemporaryFilePath(string registryKey)
        {
            string tempFolder = Path.GetTempPath();
            string[] splitParts = registryKey.Replace(" ", "_")
                .Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);

            string fileName = String.Format("{0}.{1}", splitParts[splitParts.Length - 1], "reg"); // take the last string and add an extension

            return Path.Combine(tempFolder, fileName);
        }

        private void ExportKey(string regKey, string exportFilePath)
        {
            string path = "\"" + exportFilePath + "\"";
            string key = "\"" + regKey + "\"";

            var proc = new Process();
            try
            {
                proc.StartInfo.FileName = "regedit.exe";
                proc.StartInfo.UseShellExecute = false;
                proc = Process.Start("regedit.exe", "/e " + path + " " + key + "");

                if (proc != null) proc.WaitForExit();
            }
            finally
            {
                if (proc != null) proc.Dispose();
            }
        }

        private void SubscribeToEvents()
        {
            lock (_syncRoot)
            {
                // Before subscribing: unsubscribe the handler to avoid duplication
                UnsubscribeToEvents();

                foreach (RegistryMonitor regMon in _regKeyMonitors.Values)
                {
                    regMon.RegChanged += OnRegistryKeyChanged;
                }
            }
        }

        private void UnsubscribeToEvents()
        {
            lock (_syncRoot)
            {
                foreach (RegistryMonitor regMon in _regKeyMonitors.Values)
                {
                    regMon.RegChanged -= OnRegistryKeyChanged;
                }
            }
        }

        private void UnsubscribeToEvents(RegistryMonitor regMon)
        {
            regMon.RegChanged -= OnRegistryKeyChanged;
        }

        #endregion

        #region Public methods

        public void Initialize(Dictionary<string, string> registryKeysToMonitor)
        {
            if (registryKeysToMonitor == null)
            {
                throw new ArgumentNullException("registryKeysToMonitor");
            }

            lock (_syncRoot)
            {
                _registryKeysToMonitor = registryKeysToMonitor;

                foreach (string regKey in _registryKeysToMonitor.Keys)
                {
                    _logger.DebugFormat("Creating watcher for registry key '{0}'", regKey);
                    RegistryMonitor regMon = new RegistryMonitor(regKey, _logger);

                    _regKeyMonitors.Add(regKey, regMon);
                }
                SubscribeToEvents();
            }
        }

        public void Initialize(IEnumerable<IConfigItem> itemsToMonitor)
        {
            Dictionary<string, string> registryKeysToMonitor =
                itemsToMonitor.ToDictionary(i => i.Source, i => i.Identifier, StringComparer.InvariantCulture);

            Initialize(registryKeysToMonitor);

            foreach (IConfigItem item in itemsToMonitor)
            {
                InitConfigItem(item);
            }
        }

        private void InitConfigItem(IConfigItem item)
        {
            if (item.SaveAtStart)
            {
                SaveRegistryKey(item.Source);
            }

            if (item.RestoreAtStart)
            {
                //TODO: not implemented yet: check if the local registry keys
                throw new NotImplementedException("This functionality is not available yet");
            }
        }

        public void Add(string regKey, string identifier = null)
        {
            if (_registryKeysToMonitor.Keys.Contains(regKey)) return; // key is already monitored

            lock (_syncRoot)
            {
                _registryKeysToMonitor.Add(regKey, identifier);

                var regMon = new RegistryMonitor(regKey, _logger);
                if (IsMonitoring) regMon.Start();

                _regKeyMonitors.Add(regKey, regMon);

                SubscribeToEvents();
            }
        }

        public void Add(IConfigItem item)
        {
            Add(item.Source, item.Identifier);
            InitConfigItem(item);
        }

        public void Remove(string regKey)
        {
            if (!_registryKeysToMonitor.Keys.Contains(regKey)) return; // nothing to remove

            lock (_syncRoot)
            {
                RegistryMonitor regMon;

                if (_regKeyMonitors.TryGetValue(regKey, out regMon))
                {
                    regMon.Stop();
                    UnsubscribeToEvents(regMon);
                    _regKeyMonitors.Remove(regKey); // should I dispose first??
                    regMon.Dispose();
                }
                else
                {
                    _logger.WarnFormat("Trying to remove watcher for key {0} but unable to find the registry monitor", regKey);
                    _registryKeysToMonitor.Remove(regKey);
                }
            }
        }

        public void StartMonitoring()
        {
            lock (_syncRoot)
            {
                foreach (RegistryMonitor registryMonitor in _regKeyMonitors.Values)
                {
                    if (!registryMonitor.IsMonitoring) registryMonitor.Start();
                }

                IsMonitoring = true;
            }
        }

        public void StopMonitoring()
        {
            lock (_syncRoot)
            {
                foreach (RegistryMonitor registryMonitor in _regKeyMonitors.Values)
                {
                    if (registryMonitor.IsMonitoring) registryMonitor.Stop();
                }

                IsMonitoring = false;
            }
        }

        public bool IsMonitoring { get; private set; }

        public void Dispose()
        {
            StopMonitoring();
            UnsubscribeToEvents();

            foreach (var regMon in _regKeyMonitors.Values)
            {
                regMon.Dispose();
            }

            _regKeyMonitors.Clear();
            _registryKeysToMonitor.Clear();
        }

        #endregion
    }
}
