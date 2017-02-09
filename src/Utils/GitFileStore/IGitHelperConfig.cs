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


namespace VP.FF.PT.Common.Utils.GitFileStore
{
    public interface IGitHelperConfig
    {
        /// <summary>
        /// Url relative to the Git system
        /// </summary>
        string GitRepoUrl { get; }

        /// <summary>
        /// Local folder used to store/retrieve remotely stored files
        /// </summary>
        string GitLocalFolder { get; }

        /// <summary>
        /// UserName to be used to access Git
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Password to be used to access Git
        /// </summary>
        string PasswordEncoded { get; }

        /// <summary>
        /// UserName used for the commit
        /// </summary>
        string SignatureName { get; }

        /// <summary>
        /// Email used for the commit
        /// </summary>
        string SignatureEmail { get; }

        /// <summary>
        /// Flag to indicate if the local folder should be purged at the beginning or only refreshed
        /// </summary>
        bool InitiallyDeleteLocalFolder { get; }
    }
}
