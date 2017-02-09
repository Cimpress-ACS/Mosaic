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
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using VP.FF.PT.Common.Infrastructure.Logging;

namespace VP.FF.PT.Common.PlatformEssentials.UserManagement
{
    [Export(typeof(IUserManager))]
    public class UserManager : IUserManager
    {
        private readonly ILogger _logger;
        private readonly ICollection<User> _users = new List<User>();

        /// <summary>
        /// Empty constructor is needed by WCF proxy code generator. Do not remove!
        /// </summary>
        public UserManager()
        {
        }

        [ImportingConstructor]
        public UserManager(ILogger logger)
        {
            _logger = logger;
            _logger.Init(GetType());

            string userFile = string.Empty;
            try
            {
                userFile = GetAppConfigFile();
                LoadUsers(userFile);
                _logger.Debug("users loaded");
            }
            catch (FileNotFoundException e)
            {
                _logger.Error("failed to load users from file " + userFile, e);
            }
        }

        private string GetAppConfigFile()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
        }

        public bool CheckCredentials(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
                return false;

            var user = _users.FirstOrDefault(u => u.Name == username);

            if (user == null)
            {
                _logger.Warn("User " + username + " tried to login, but failed because its not registered");
                return false;
            }

            var pwd = user.Password;

            SHA256 encoder = SHA256.Create();
            var encodedPwd = encoder.ComputeHash(Encoding.Unicode.GetBytes(password));

            if (Convert.ToBase64String(encodedPwd) == pwd)
            {
                user.LoginFailedCount = 0;
                return true;
            }

            _logger.Warn("User " + username + " tried to login, but used wrong password");
            user.LoginFailedCount++;
            return false;
        }

        public bool ChangePassword(string username, string newPassword)
        {
            SHA256 encoder = SHA256.Create();
            byte[] encodedPwd = encoder.ComputeHash(Encoding.Unicode.GetBytes(newPassword));
            try
            {
                _users.First(user => user.Name == username).Password = Convert.ToBase64String(encodedPwd);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        public void AddUser(User user)
        {
            var tempUser = new User(user);
            SHA256 encoder = SHA256.Create();
            byte[] encodedPwd = encoder.ComputeHash(Encoding.Unicode.GetBytes(tempUser.Password));
            tempUser.Password = Convert.ToBase64String(encodedPwd);
            _users.Add(tempUser);
        }

        public void RemoveUser(User user)
        {
            _users.Remove(user);
        }

        public User GetUser(string username)
        {
            var user = _users.FirstOrDefault(u => u.Name == username);

            if (user == null)
                _logger.Warn("Could not find user " + username);

            return user;
        }

        public IEnumerable<string> GetUserNames()
        {
            return _users.Select(user => user.Name).ToList();
        }

        public void LogIn(User user)
        {
            var validUser = _users.FirstOrDefault(u => u.Name == user.Name);

            if (validUser != null)
            {
                validUser.LoggedIn = true;
                _logger.Info("User " + user.Name + " logged in!");
            }
            else
            {
                _logger.Info("User" + user.Name + " not found!");
            }
        }

        public void LoadUsers(string filePath)
        {
            //the function needs to be commented when the service is updated.
            // TODO: yes, and maybe you should split it in smaller chunks of code (private methods)
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Can't find users file", filePath);

            SHA256 encoder = SHA256.Create();

            using (XmlReader reader = XmlReader.Create(filePath))
            {

                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "User")
                    {
                        var temp = new User
                                   {
                                       Id = _users.Count()
                                   };

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "Name")
                                temp.Name = reader.ReadString().Trim();
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Picture")
                            {
                                while (reader.Read())
                                {
                                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Src")
                                    {
                                        //temp.Picture = new BitmapImage(new Uri(reader.ReadString()));
                                        break;
                                    }
                                }
                            }
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Description")
                                temp.Description = reader.ReadString().Trim();
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "Role")
                            {
                                string authType = reader.ReadString().Trim();
                                if (authType == "Administrator")
                                    temp.AuthType = AuthTypes.Administrator;
                                if (authType == "Engineer")
                                    temp.AuthType = AuthTypes.Engineer;
                                if (authType == "Operator")
                                    temp.AuthType = AuthTypes.Operator;
                            }
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "PasswordHashed")
                            {
                                temp.Password = reader.ReadString().Trim().Replace(Environment.NewLine, string.Empty);
                                break;
                            }
                        }
                        _users.Add(temp);
                    }
                }
            }

        }

        public void SaveUsers(string filePath)
        {
            throw new NotImplementedException();
        }

        public void LogOut(string username)
        {
            var validUser = _users.FirstOrDefault(u => u.Name == username);

            if (validUser != null)
            {
                validUser.LoggedIn = false;
                _logger.Info("User " + username + " logged out!");
            }
            else
            {
                _logger.Info("User" + username + " not found!");
            }
        }

        public bool CanLogin(User user)
        {
            var tempUser = _users.FirstOrDefault(u => u.Name == user.Name);

            if (tempUser == null)
                return false;

            if (tempUser.LoggedIn)
                return false;

            if (tempUser.LoginFailedCount >= 5)
                return false;

            return true;
        }
    }
}
