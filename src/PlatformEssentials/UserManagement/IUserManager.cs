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


using System.Collections.Generic;

namespace VP.FF.PT.Common.PlatformEssentials.UserManagement
{
    public interface IUserManager
    {
        bool CheckCredentials(string username, string password);

        bool ChangePassword(string username, string newPassword);

        void AddUser(User user);

        void RemoveUser(User user);

        User GetUser(string username);

        IEnumerable<string> GetUserNames(); 

        void LogIn(User user);

        bool CanLogin(User user);

        void LogOut(string username);

        void LoadUsers(string filePath);

        void SaveUsers(string filePath);

    }
}
