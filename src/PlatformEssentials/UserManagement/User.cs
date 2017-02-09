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


using System.Runtime.Serialization;

namespace VP.FF.PT.Common.PlatformEssentials.UserManagement
{
    public enum AuthTypes
    {
        Operator,
        Engineer,
        Administrator,
        Other
    }

    [DataContract]
    public class User
    {
        [DataMember]
        private string _password;

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool LoggedIn { get; set; }

        [DataMember]
        public AuthTypes AuthType { get; set; }

        [DataMember]
        public int LoginFailedCount { get; set; }

        [DataMember]
        public string Password
        {
            get
            {
                return _password; 
            }
             set
            {
                _password = value;
            }
        }

        public User(int id, string name, string description, bool loggedIn, AuthTypes authType,
            int loginFailedCount, string password)
        {
            Id = id;
            Name = name;
            Description = description;
            LoggedIn = loggedIn;
            AuthType = authType;
            LoginFailedCount = loginFailedCount;
            Password = password;

        }

        public User()
        {
            
        }

        public User(User user)
        {
            Id = user.Id;
            Name = user.Name;
            Description = user.Description;
            LoggedIn = user.LoggedIn;
            AuthType = user.AuthType;
            LoginFailedCount = user.LoginFailedCount;
            Password = user.Password;
        }

    }
}
