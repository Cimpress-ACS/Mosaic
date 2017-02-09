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


using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace VP.FF.PT.Common.Utils.GitFileStore
{
    internal class FileHelper
    {
        internal static byte[] GetMd5Hash(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }

        internal static bool AreFilesTheSame(string fileName1, string fileName2)
        {
            byte[] hash1 = GetMd5Hash(fileName1);
            byte[] hash2 = GetMd5Hash(fileName2);
            return (hash1.SequenceEqual(hash2));
        }
    }
}
