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
using System.IO;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Utils.GitFileStore.UnitTests
{
    [TestFixture]
    public class GitHelperTests
    {
        private GitHelper _gitHelper;
        private Mock<ILogger> _logger;
        private Mock<IGitHelperConfig> _config;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger>();
            _config = new Mock<IGitHelperConfig>();
        }

        [Test, Ignore("Interactive test: not to be run as part of automatic build")]
        public void SetUpRepository_SaveFile_VerifyFileHasBeenSaved()
        {
            _config.Setup(x => x.GitRepoUrl)
                .Returns(@"https://github.com/sizilium/Mosaic.git");
            _config.Setup(x => x.GitLocalFolder).Returns("Cimpress_TestBackup");

            _config.Setup(x => x.UserName).Returns("saber");
            _config.Setup(x => x.PasswordEncoded).Returns("VW1sS05tTTFjMnRBS21GUQ==");

            _config.Setup(x => x.SignatureName).Returns("Saber");
            _config.Setup(x => x.SignatureEmail).Returns("saber@cimpress.com");

            _config.Setup(x => x.InitiallyDeleteLocalFolder).Returns(true);

            string path = Path.Combine(Path.GetTempPath(), "test_params.txt");

            File.WriteAllText(path, "stuff to save in the file " + DateTime.Now);

            _gitHelper = new GitHelper(_config.Object, _logger.Object);
            _gitHelper.Refresh();

            _gitHelper.SaveFile(@"testDir1\testDir2\test_module\test_params.txt", path);
        }
    }
}
