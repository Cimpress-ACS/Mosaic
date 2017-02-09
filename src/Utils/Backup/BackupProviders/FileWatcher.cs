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
using System.IO;
using System.Reactive.Linq;

// Find interesting reference here: http://jaylee.org/post/2012/08/26/An-update-to-matthieumezil-Rx-and-the-FileSystemWatcher.aspx

namespace VP.FF.PT.Common.Utils.Backup.BackupProviders
{
    public class FileWatcher : IDisposable
    {
        public string Path { get; private set; }
        public string Filter { get; private set; }
        public TimeSpan Throttle { get; private set; }

        readonly FileSystemWatcher _fsWatcher;
        public FileWatcher(string path, string filter, TimeSpan throttle)
        {
            Path = path;
            Filter = filter;
            Throttle = throttle;

            _fsWatcher = new FileSystemWatcher(Path, Filter);
        }

        public bool IsMonitoring
        {
            get
            {
                return _fsWatcher == null ? false : _fsWatcher.EnableRaisingEvents;
            }
        }

        public void StartMonitoring()
        {
            if (_fsWatcher != null)
            {
                _fsWatcher.EnableRaisingEvents = true;
            }
        }

        public void StopMonitoring()
        {
            if (_fsWatcher != null)
            {
                _fsWatcher.EnableRaisingEvents = false;
            }
        }

        public IDisposable Subscribe(Action<FileSystemEventArgs> onNext, Action<Exception> onError)
        {
            return ObserveFolderChanges().Subscribe(onNext, onError);
        }

        private IObservable<FileSystemEventArgs> ObserveFolderChanges()
        {
            return Observable.Create<FileSystemEventArgs>(
                observer =>
                {
                    var sources = new[]
                    {
                        Observable.FromEventPattern<FileSystemEventArgs>(_fsWatcher, "Changed").Select(ev => ev.EventArgs)
                    };

                    return sources.Merge()
                        .Throttle(Throttle)
                        .Subscribe(observer);
                }
                );
        }

        public void Dispose()
        {
            if (_fsWatcher != null)
            {
                _fsWatcher.Dispose();
            }
        }
    }
}
