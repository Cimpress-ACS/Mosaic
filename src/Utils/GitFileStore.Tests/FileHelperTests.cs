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


using NUnit.Framework;
using System;
using System.IO;

namespace VP.FF.PT.Common.Utils.GitFileStore.UnitTests
{
    [TestFixture]
    public class FileHelperTests
    {
        [Test]
        public void CheckMd5HashIsCalculated()
        {
            // create a new file and check md5
            string filePath = Path.GetTempFileName();

            File.WriteAllText(filePath, "write some stuff in the file");

            string md5Hash = ByteArrayToString(FileHelper.GetMd5Hash(filePath));

            Assert.IsNotNullOrEmpty(md5Hash);

            // cleanup
            File.Delete(filePath);
        }

        [Test]
        public void CopyFileAndVerifySameMd5Hash()
        {
            string sourceFile = Path.GetTempFileName();
            string targetFile = Path.GetTempFileName();

            File.WriteAllText(sourceFile, "write some stuff in the file");

            File.Copy(sourceFile, targetFile, true);

            Assert.IsTrue(FileHelper.AreFilesTheSame(sourceFile, targetFile));

            // cleanup
            File.Delete(sourceFile);
            File.Delete(targetFile);
        }

        public static string ByteArrayToString(byte[] byteArray)
        {
            string hex = BitConverter.ToString(byteArray);
            return hex.Replace("-", "");
        }
    }
}
