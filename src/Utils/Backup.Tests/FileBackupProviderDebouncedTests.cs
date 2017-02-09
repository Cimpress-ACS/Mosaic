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


using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.Utils.Backup.BackupProviders;

namespace VP.FF.PT.Common.Utils.Backup.UnitTests
{
    public class FileBackupProviderDebouncedTests
    {
        private FileBackupProviderDebounced _fileBackupProviderDebounced;
        private Mock<IFileStore> _fileStoreMock;
        private Mock<ILogger> _loggerMock;

        private string _fileToMonitor1;
        private string _fileToMonitor2;

        [SetUp]
        public void SetUp()
        {
            string tempPath = Path.GetTempPath();

            _fileToMonitor1 = Path.Combine(tempPath, "fileToMonitor1");
            _fileToMonitor2 = Path.Combine(tempPath, "fileToMonitor2");

            _fileStoreMock = new Mock<IFileStore>();
            _loggerMock = new Mock<ILogger>();

            Dictionary<string, string> filePathsToMonitor = GetFilePathsToMonitor();

            _fileBackupProviderDebounced = new FileBackupProviderDebounced(_fileStoreMock.Object, _loggerMock.Object);
            _fileBackupProviderDebounced.Initialize(filePathsToMonitor);
        }

        [TearDown]
        public void TearDown()
        {
            _fileBackupProviderDebounced.StopMonitoring();
            _fileBackupProviderDebounced.Dispose();

            foreach (var filePath in GetFilePathsToMonitor().Keys)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Test, Category("Integration")]
        public void CheckFilesAreMonitoredAsExpected()
        {
            _fileBackupProviderDebounced.StartMonitoring();

            // Modify a file, verify that it persisted
            File.WriteAllText(_fileToMonitor1, "Write some stuff to the file");

            string fileId = _fileToMonitor1.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(fileId, _fileToMonitor1));

            _fileBackupProviderDebounced.StopMonitoring();
        }

        [Test, Category("Integration")]
        public void CheckFilesAreMonitoredOnCreation()
        {
            string tempPath = Path.GetTempPath();

            string filePath = Path.Combine(tempPath, "tempFileToMonitor1.txt");

            RemoveFile(filePath);

            Assert.IsFalse(File.Exists(filePath), "File already exists.");

            _fileBackupProviderDebounced.Add(filePath, null);

            _fileBackupProviderDebounced.StartMonitoring();

            File.WriteAllText(filePath, "some content in the newly created file");

            Assert.IsTrue(File.Exists(filePath), "File has not been created.");

            string fileId = filePath.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(fileId, filePath), Times.Once);

            _fileBackupProviderDebounced.StopMonitoring();
        }

        private void RemoveFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [Test, Category("Integration")]
        public void CheckFilesAreMonitoredWhenModified()
        {
            string tempPath = Path.GetTempPath();

            string filePath = Path.Combine(tempPath, "tempFileToMonitor2.txt");

            File.WriteAllText(filePath, "Existing file content");

            Assert.IsTrue(File.Exists(filePath), "File exists.");

            _fileBackupProviderDebounced.Add(filePath, null);

            _fileBackupProviderDebounced.StartMonitoring();

            File.AppendAllText(filePath, "- add content to the file -");

            string fileId = filePath.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(fileId, filePath), Times.Once);

            _fileBackupProviderDebounced.StopMonitoring();
        }

        [Test, Category("Integration")]
        public void CheckFilesAreMonitoredWhenModifiedLongWaitingTime()
        {
            string tempPath = Path.GetTempPath();

            string filePath = Path.Combine(tempPath, "tempFileToMonitorLong.txt");
            string filePathCopy = Path.Combine(tempPath, "tempFileToMonitorCopy.txt");

            string identifier = "some_sort_of_ID";

            File.WriteAllText(filePath, "Existing file content");

            Assert.IsTrue(File.Exists(filePath), "File exists.");

            _fileBackupProviderDebounced.Add(filePath, identifier);

            _fileBackupProviderDebounced.StartMonitoring();

            File.AppendAllText(filePath, "- add content to the file -");

            Thread.Sleep(TimeSpan.FromMilliseconds(300));

            File.Copy(filePath, filePathCopy, true);

            Thread.Sleep(TimeSpan.FromSeconds(3));

            _fileStoreMock.Verify(x => x.SaveFile(identifier, filePath), Times.Once);

            _fileBackupProviderDebounced.StopMonitoring();

            RemoveFile(filePath);
            RemoveFile(filePathCopy);
        }

        [Test, Category("Integration")]
        public void CheckFilesIdentifierProvidedIsUsed()
        {
            const string fileName = "tempFileToMonitor3.txt";
            const string identifier = "idForMyFile";

            string tempPath = Path.GetTempPath();

            string filePath = Path.Combine(tempPath, fileName);

            File.WriteAllText(filePath, "Existing file content");

            Assert.IsTrue(File.Exists(filePath), "File exists.");

            _fileBackupProviderDebounced.Add(filePath, identifier);

            _fileBackupProviderDebounced.StartMonitoring();

            File.AppendAllText(filePath, "- add content to the file -");

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(identifier, filePath), Times.Once);

            _fileBackupProviderDebounced.StopMonitoring();
        }

        [Test, Category("Integration")]
        public void CheckFilesAreMonitoredWhenAddedToTheList()
        {
            _fileBackupProviderDebounced.StartMonitoring();

            string tempPath = Path.GetTempPath();

            string filePath = Path.Combine(tempPath, "fileThere.txt");
            string fileId = filePath.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');

            File.WriteAllText(filePath, "Existing file content");

            Assert.IsTrue(File.Exists(filePath), "File exists.");

            // Nothing saved
            _fileStoreMock.Verify(x => x.SaveFile(fileId, filePath), Times.Never);

            _fileBackupProviderDebounced.Add(filePath, null);

            File.AppendAllText(filePath, "Additional text");

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(fileId, filePath), Times.Once);

            _fileBackupProviderDebounced.StopMonitoring();

            RemoveFile(filePath);
        }


        [Test, Category("Integration")]
        public void CheckFilesAreNotMonitoredWhenRemoved()
        {
            _fileBackupProviderDebounced.StopMonitoring();

            string tempPath = Path.GetTempPath();
            string filePath = Path.Combine(tempPath, "aNewFile.txt");

            RemoveFile(filePath);

            string fileId = filePath.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');

            _fileBackupProviderDebounced.Add(filePath, null); // using default identifier

            _fileBackupProviderDebounced.StartMonitoring();

            File.WriteAllText(filePath, "some content");

            Assert.IsTrue(File.Exists(filePath), "File exists.");

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(fileId, filePath), Times.Once);
            _fileStoreMock.ResetCalls();

            _fileBackupProviderDebounced.Remove(filePath);

            File.AppendAllText(filePath, "Additional text");

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _fileStoreMock.Verify(x => x.SaveFile(fileId, filePath), Times.Never);

            _fileBackupProviderDebounced.StopMonitoring();

            RemoveFile(filePath);
        }

        [Test, Category("Integration")]
        public void ChangedNotMonitoredFileInMonitoredFolder_DoNotGetSaved()
        {
            _fileBackupProviderDebounced.StartMonitoring();

            string someFilePath = Path.Combine(Path.GetTempPath(), "someFile.Txt");
            File.WriteAllText(someFilePath, "Write some stuff to the file");

            string fileId = someFilePath.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_');

            _fileStoreMock.Verify(x => x.SaveFile(fileId, someFilePath), Times.Never);

            _fileBackupProviderDebounced.StopMonitoring();
            RemoveFile(someFilePath);
        }

        [Test]
        public void ExceptionRaisedWhenVcsAccessorIsNull()
        {
            Assert.That(() => new FileBackupProvider(null, _loggerMock.Object), Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ExceptionRaisedWhenLoggerIsNull()
        {
            Assert.That(() => new FileBackupProvider(_fileStoreMock.Object, null), Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ExceptionRaisedWhenNullFileListIsProvided()
        {
            var anotherFileBackupProvider = new FileBackupProvider(_fileStoreMock.Object, _loggerMock.Object);
            Assert.That(() => anotherFileBackupProvider.Initialize((Dictionary<string, string>)null), Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ExceptionRaisedWhenNullFileListIsProvidedOtherCtr()
        {
            var anotherFileBackupProvider = new FileBackupProvider(_fileStoreMock.Object, _loggerMock.Object);
            Assert.That(() => anotherFileBackupProvider.Initialize((IEnumerable<IConfigItem>)null), Throws.Exception.TypeOf<ArgumentNullException>());
        }

        private Dictionary<string, string> GetFilePathsToMonitor()
        {
            return new Dictionary<string, string> { { _fileToMonitor1, null }, { _fileToMonitor2, null } };
        }
    }
}
