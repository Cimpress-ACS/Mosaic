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
using System.IO;
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Utils.Backup.BackupProviders
{
    public class FileBackupProvider : IBackupProvider, IDisposable
    {
        private readonly IFileStore _fileStore;
        private readonly ILogger _logger;
        private Dictionary<string, string> _filePathsToMonitor;
        private readonly Dictionary<string, FileSystemWatcher> _fileSystemWatchers;

        private readonly Object _syncRoot = new Object();

        public FileBackupProvider(IFileStore fileStore, ILogger logger)
        {
            if (fileStore == null) throw new ArgumentNullException("fileStore");
            if (logger == null) throw new ArgumentNullException("logger");

            _fileStore = fileStore;
            _logger = logger;
            _filePathsToMonitor = new Dictionary<string, string>();
            _fileSystemWatchers = new Dictionary<string, FileSystemWatcher>();

            _logger.Init(typeof(FileBackupProvider));
            IsMonitoring = false;
        }

        #region Private methods

        private void FileHasChanged(object sender, FileSystemEventArgs e)
        {
            if (IsModifiedFileRelevant(e.FullPath))
            {
                _logger.DebugFormat("Change {0} detected in file {1}", e.ChangeType, e.FullPath);

                if (e.ChangeType != WatcherChangeTypes.Deleted)
                {
                    string key = _filePathsToMonitor[e.FullPath];

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        key = GetIdentifierFromPath(e.FullPath);
                        // take folder with no root and transform it in something like vol_dir1_dir2_dir3
                    }

                    _logger.DebugFormat("** The file {0} has been changed and will be saved. Change Type: [{1}]. Id: [{2}]",
                        e.FullPath, e.ChangeType, key);

                    _fileStore.SaveFile(key, e.FullPath);
                }
            }
            else
            {
                _logger.DebugFormat("Element '{0}' has changed but it is not included in list of files to be monitored. No action will be taken.",
                    e.FullPath);
            }
        }

        private string GetIdentifierFromPath(string fullPath)
        {
            return fullPath.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');
        }

        private bool IsModifiedFileRelevant(string fullPath)
        {
            return _filePathsToMonitor.Keys.Contains(fullPath);
        }

        private void SubscribeToEvents()
        {
            lock (_syncRoot)
            {
                // Before subscribing: unsubscribe the handler to avoid duplication
                UnsubscribeToEvents();

                foreach (var fileSystemWatcher in _fileSystemWatchers.Values)
                {
                    fileSystemWatcher.Changed += FileHasChanged;
                }
            }
        }

        private void UnsubscribeToEvents()
        {
            lock (_syncRoot)
            {
                foreach (FileSystemWatcher fileSystemWatcher in _fileSystemWatchers.Values)
                {
                    fileSystemWatcher.Changed -= FileHasChanged;
                }
            }
        }

        private void CleanUpFileSystemWatchers()
        {
            lock (_fileSystemWatchers)
            {
                foreach (var parentFolder in _fileSystemWatchers.Keys)
                {
                    if (_filePathsToMonitor.Keys.Select(Path.GetDirectoryName).All(x => x != parentFolder))
                    {
                        _fileSystemWatchers[parentFolder].EnableRaisingEvents = false;
                        _fileSystemWatchers.Remove(parentFolder);
                    }
                }
            }

            SubscribeToEvents();
        }

        #endregion

        #region Public methods

        public void Initialize(Dictionary<string, string> filePathsToMonitor)
        {
            if (filePathsToMonitor == null)
            {
                throw new ArgumentNullException("filePathsToMonitor");
            }

            lock (_syncRoot)
            {
                _filePathsToMonitor = filePathsToMonitor;

                // for every unique directory with files to monitor, create a filewatcher
                foreach (var dirPath in _filePathsToMonitor.Keys.Select(Path.GetDirectoryName).Distinct())
                {
                    try
                    {
                        FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(dirPath);
                        lock (_syncRoot)
                        {
                            _logger.DebugFormat("Creating watcher for folder '{0}'", dirPath);
                            _fileSystemWatchers.Add(dirPath, fileSystemWatcher);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        _logger.ErrorFormat("Cannot create a watcher for {0}. Error: {1}.",
                            dirPath, ex.Message);
                    }
                }

                SubscribeToEvents();
            }
        }

        public void Initialize(IEnumerable<IConfigItem> itemsToMonitor)
        {
            Dictionary<string, string> filesToBeMonitored =
                itemsToMonitor.ToDictionary(i => i.Source, i => i.Identifier, StringComparer.InvariantCulture);

            Initialize(filesToBeMonitored);

            foreach (IConfigItem item in itemsToMonitor)
            {
                InitConfigItem(item);
            }
        }

        private void InitConfigItem(IConfigItem item)
        {
            if (item.SaveAtStart)
            {
                try
                {
                    _logger.DebugFormat("File {0} will be saved at start as configured", item.Source);
                    _fileStore.SaveFile(item.Identifier, item.Source);
                }
                catch (Exception ex)
                {
                    _logger.ErrorFormat("File {0} cannot be saved. Error: {1}", item.Source, ex.Message);
                }
            }

            if (item.RestoreAtStart)
            {
                _logger.DebugFormat("Checking file {0}: it will be restored if not present.", item.Source);
                try
                {
                    if (!File.Exists(item.Source))
                    {
                        _logger.Debug("The file doesn't exist and will be restored...");
                        string restoredPath = _fileStore.LoadFile(item.Identifier);

                        if (restoredPath != item.Source)
                        {
                            File.Copy(restoredPath, item.Source);
                            File.Delete(restoredPath);
                            _logger.DebugFormat("Restoration of file {0} complete.", item.Source);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorFormat("File {0} cannot be restored. Error: {1}", item.Source, ex.Message);
                }
            }
        }

        public void Add(string filePath, string identifier)
        {
            if (_filePathsToMonitor.ContainsKey(filePath)) return; // file is already monitored

            lock (_syncRoot)
            {
                _filePathsToMonitor.Add(filePath, identifier);
            }

            string parentFolder = Path.GetDirectoryName(filePath);

            if (parentFolder != null)
            {
                lock (_syncRoot)
                {
                    if (_fileSystemWatchers.ContainsKey(parentFolder)) return; // directory is already monitored

                    var fileSystemWatcher = new FileSystemWatcher(parentFolder) { EnableRaisingEvents = IsMonitoring };
                    _fileSystemWatchers.Add(parentFolder, fileSystemWatcher);
                }

                SubscribeToEvents();
            }
        }

        public void Add(IConfigItem item)
        {
            Add(item.Source, item.Identifier);
            InitConfigItem(item);
        }

        public void Remove(string filePath)
        {
            if (!_filePathsToMonitor.ContainsKey(filePath)) return; // nothing to remove

            lock (_syncRoot)
            {
                _filePathsToMonitor.Remove(filePath);

                CleanUpFileSystemWatchers();
            }
        }

        public void StartMonitoring()
        {
            lock (_syncRoot)
            {
                foreach (var fileSystemWatcher in _fileSystemWatchers.Values)
                {
                    fileSystemWatcher.EnableRaisingEvents = true;
                }

                IsMonitoring = true;
            }
        }

        public void StopMonitoring()
        {
            lock (_syncRoot)
            {
                foreach (var fileSystemWatcher in _fileSystemWatchers.Values)
                {
                    fileSystemWatcher.EnableRaisingEvents = false;
                }

                IsMonitoring = false;
            }
        }

        public bool IsMonitoring { get; private set; }

        public void Dispose()
        {
            // Ensure that everything is stopped
            StopMonitoring();
            UnsubscribeToEvents();

            foreach (var fileWatcher in _fileSystemWatchers.Values)
            {
                fileWatcher.Dispose();
            }
            _filePathsToMonitor.Clear();
            _fileSystemWatchers.Clear();
        }

        #endregion
    }
}
