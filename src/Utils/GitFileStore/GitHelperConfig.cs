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
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Configuration;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.Utils.GitFileStore
{
    [Export(typeof(IGitHelperConfig))]
    public class GitHelperConfig : IGitHelperConfig
    {
        private readonly ILogger _logger;
        public string GitRepoUrl { get; private set; }
        public string GitLocalFolder { get; private set; }
        public string UserName { get; private set; }
        public string PasswordEncoded { get; private set; }
        public string SignatureName { get; private set; }
        public string SignatureEmail { get; private set; }
        public bool InitiallyDeleteLocalFolder { get; private set; }

        [ImportingConstructor]
        public GitHelperConfig(ILogger logger)
        {
            _logger = logger;
            _logger.Init(typeof(GitHelperConfig));

            string configSectionName = "GitConfiguration";

            try
            {
                // Retrieve the values from the app.config file
                var section = ConfigurationManager.GetSection(configSectionName) as NameValueCollection;

                if (section == null)
                {
                    _logger.ErrorFormat("Cannot read {0} section from app.config", configSectionName);
                }
                else
                {
                    GitRepoUrl = section["GitRepoUrl"];
                    GitLocalFolder = section["GitLocalFolder"];
                    UserName = section["UserName"];
                    PasswordEncoded = section["PasswordEncoded"];
                    SignatureName = section["SignatureName"];
                    SignatureEmail = section["SignatureEmail"];
                    InitiallyDeleteLocalFolder = Convert.ToBoolean(section["InitiallyDeleteLocalFolder"]);

                    _logger.DebugFormat("Successfully loaded GIT configuration params: local repo {0} (Repo URL: {1})",
                        GitLocalFolder, GitRepoUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error processing {0}: {1}", configSectionName, ex.Message);
            }
        }
    }
}
