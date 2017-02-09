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


using LibGit2Sharp;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.Utils.Backup;
using VP.FF.PT.Common.Utils.Security;

namespace VP.FF.PT.Common.Utils.GitFileStore
{
    [Export(typeof(IFileStore))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GitHelper : IFileStore
    {
        private readonly IGitHelperConfig _config;
        private readonly ILogger _logger;

        private readonly string _remoteRepoUrl;
        private readonly string _configurationPath;
        private readonly string _localBranchName;
        private readonly string _trackedBranchName;

        private static Repository _gitRepo;
        private readonly Credentials _credentials;
        private readonly Signature _signature;

        [ImportingConstructor]
        public GitHelper(IGitHelperConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;

            _logger.Init(typeof(GitHelper));

            _remoteRepoUrl = _config.GitRepoUrl;
            _credentials = new UsernamePasswordCredentials { Username = _config.UserName, Password = _config.PasswordEncoded.DecodePassword() };
            _signature = new Signature(_config.SignatureName, _config.SignatureEmail, DateTimeOffset.Now);

            _configurationPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _config.GitLocalFolder);

            // If configured to be cleaned and the repository folder already exists: let's clean it
            if (_config.InitiallyDeleteLocalFolder && Directory.Exists(_configurationPath))
            {
                try
                {
                    Directory.Delete(_configurationPath, true);
                }
                catch (Exception ex)
                {
                    _logger.WarnFormat("Trying to clean folder {0} failed with the following error {1}",
                        _configurationPath, ex.Message);
                }
            }

            _localBranchName = "master";
            _trackedBranchName = "origin/master";

            InitializeRepo();
        }

        private Branch TrackedBranch
        {
            get
            {
                if (_gitRepo != null)
                {
                    return _gitRepo.Branches[_trackedBranchName];
                }
                return null;
            }
        }

        private Branch LocalBranch
        {
            get
            {
                if (_gitRepo != null)
                {
                    var branch = _gitRepo.Branches[_localBranchName];

                    if (branch == null)
                    {
                        // Create a local branch pointing at tracked branch last commit
                        return _gitRepo.CreateBranch(_localBranchName, TrackedBranch.Tip);
                    }

                    return _gitRepo.Branches[_localBranchName];
                }
                return null;
            }
        }

        private void InitializeRepo()
        {
            try
            {
                Repository.Init(_configurationPath);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Error initializing repository in {0}. Error: {1}", _configurationPath, ex.Message);
            }

            _gitRepo = new Repository(_configurationPath);

            // if the repo was not created then log the situation and return, no reason to try to fetch/update from
            // uninitialized repo
            if (_gitRepo == null)
            {
                _logger.Error("Fatal error: Unable to create the GIT repository");
                return;
            }

            if (!_gitRepo.Network.Remotes.Any())
            {
                // Only add and fetch origin if we don't already have it
                _gitRepo.Network.Remotes.Add("origin", _remoteRepoUrl);
            }

            try
            {
                _gitRepo.Fetch("origin", new FetchOptions { CredentialsProvider = (url, user, cred) => _credentials });

                // So, let's configure the local branch to track the remote one.
                _gitRepo.Branches.Update(LocalBranch, b => b.TrackedBranch = TrackedBranch.CanonicalName);

                Branch checkedOutBranch = _gitRepo.Checkout(LocalBranch, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                _gitRepo.Network.Pull(_signature, GetFastForwardPullOptions());

                _logger.DebugFormat("Branch checked out for repository '{0}' in '{1}' (branch: {2})",
                    _remoteRepoUrl, _configurationPath, checkedOutBranch.Name);
            }
            catch (LibGit2SharpException ex)
            {
                _logger.ErrorFormat("Error fetching/pulling from origin ({0}). Error: {1} (credentials used: [{2}][{3}]. Password is encoded.",
                    _remoteRepoUrl, ex.Message, _config.UserName, _config.PasswordEncoded);
            }
        }

        public void Refresh()
        {
            if (_gitRepo == null)
            {
                InitializeRepo();
            }

            try
            {
                _gitRepo.Fetch("origin", new FetchOptions { CredentialsProvider = (url, user, cred) => _credentials });
                _gitRepo.Network.Pull(_signature, GetFastForwardPullOptions());
            }
            catch (LibGit2SharpException ex)
            {
                _logger.ErrorFormat("Error refreshing repository '{0}'. Error: {1}", _remoteRepoUrl, ex.Message);
            }

            Branch checkedOutBranch = _gitRepo.Checkout(LocalBranch, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

            _logger.DebugFormat("Refreshed configuration for repository '{0}' in '{1}' (branch: {2})",
                _remoteRepoUrl, _configurationPath, checkedOutBranch.Name);
        }

        private PullOptions GetFastForwardPullOptions()
        {
            return new PullOptions()
            {
                MergeOptions = new MergeOptions()
                {
                    FastForwardStrategy = FastForwardStrategy.Default
                }
            };
        }

        /// <summary>
        /// Save a file (filepath) in relative path from Git root (identifier)
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="filepath"></param>
        public void SaveFile(string identifier, string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    _logger.WarnFormat("File {0} not present. Please check the folder where it should be located.", filepath);
                    return; // do nothing
                }

                string filepathInWorkDir = identifier;

                string targetFilePath = Path.Combine(_configurationPath, filepathInWorkDir);
                string containingDirectory = Path.GetDirectoryName(targetFilePath);

                if (containingDirectory != null && !Directory.Exists(containingDirectory))
                {
                    Directory.CreateDirectory(containingDirectory);
                }

                // Check if there is anything that needs to be saved first
                if (File.Exists(targetFilePath))
                {
                    if (FileHelper.AreFilesTheSame(filepath, targetFilePath))
                    {
                        _logger.InfoFormat("The file {0} that should be persisted has not changed. No action will take place.",
                            targetFilePath);
                        return;
                    }
                }

                // Copy the file in the repository
                File.Copy(filepath, targetFilePath, true);

                // Commit the change
                _gitRepo.Index.Add(filepathInWorkDir);

                string message = string.Format("Saving config file {0} for module {1}", Path.GetFileName(filepath),
                    identifier);

                _logger.DebugFormat("GIT: committing changes to local branch using default signature ({0})", _signature.Name);
                _gitRepo.Commit(message, _signature, _signature);

                PushOptions options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) => _credentials
                };

                // Push the change
                _logger.Debug("GIT: pushing local branch");
                _gitRepo.Network.Push(LocalBranch, options);

                PullOptions pullOptions = new PullOptions
                {
                    FetchOptions = new FetchOptions
                    {
                        CredentialsProvider = (url, usernameFromUrl, types) => _credentials
                    },
                    MergeOptions = new MergeOptions
                    {
                        FastForwardStrategy = FastForwardStrategy.Default
                    }
                };

                _logger.Debug("GIT: pulling remote branch");
                _gitRepo.Network.Pull(_signature, pullOptions);

                _logger.InfoFormat("Saved configurations of  module {0} to version control system as {1}.",
                    identifier, filepath);
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Failed saving config file {0} for module {1}. Error: {2}",
                    filepath, identifier, ex.Message);
            }
        }

        /// <summary>
        /// Retrieve the filepath for the file corresponding to specific identifier
        /// </summary>
        /// <param name="identifier">unique file identifier</param>
        /// <returns>file path or null if the file could not be found</returns>
        public string LoadFile(string identifier)
        {
            Refresh();
            string filepath = Path.Combine(_configurationPath, identifier);
            if (File.Exists(filepath))
            {
                _logger.DebugFormat("Returning required file location: {0}", filepath);
                return filepath;
            }
            _logger.WarnFormat("The required file {0} does not exist. Cannot retrieve it from {1}", identifier, filepath);
            return null;
        }
    }
}
