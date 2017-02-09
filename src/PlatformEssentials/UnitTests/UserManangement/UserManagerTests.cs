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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.UserManagement;
using VP.FF.PT.Common.TestInfrastructure;

namespace VP.FF.PT.Common.PlatformEssentials.UnitTests.UserManangement
{
    [TestFixture]
    public class UserManagerTests
    {
        private IUserManager _userManager;
        private ILogger _logger;
        private User _adminUser;
        private User _engineerUser;
        private User _operatorUser;
        
        #region Create different type of users
        private User CreateRandomAdministratorUser()
        {
            return new User
            {
                AuthType = AuthTypes.Administrator,
                Description = CreateRandom.String(),
                Id = 1,
                LoggedIn = true,
                LoginFailedCount = CreateRandom.Int(),
                Name = "Admin",
                Password = "Adminpw"
            };
        }

        private User CreateRandomEngineerUser()
        {
            return new User
            {
                AuthType = AuthTypes.Engineer,
                Description = CreateRandom.String(),
                Id = 2,
                LoggedIn = false,
                LoginFailedCount = CreateRandom.Int(),
                Name = "Engineer",
                Password = "Engineerpw"
            };
        }

        private User CreateRandomOperatorUser()
        {
            return new User
            {
                AuthType = AuthTypes.Operator,
                Description = CreateRandom.String(),
                Id = 3,
                LoggedIn = false,
                LoginFailedCount = CreateRandom.Int(),
                Name = "Operator",
                Password = "operatorpw"
            };
        }
        #endregion

        [SetUp]
        public void SetUp()
        {
            _logger = new AggregatedLogger();
            _userManager = new UserManager(_logger);
            _adminUser = CreateRandomAdministratorUser();
            _engineerUser = CreateRandomEngineerUser();
            _operatorUser = CreateRandomOperatorUser();
        }

        [Test]
        public void ValidUserCanLoginSuccesfully()
        {
            var testUser=new User
            {
                AuthType = AuthTypes.Administrator,
                Description = CreateRandom.String(),
                Id = 4,
                LoggedIn = false,
                LoginFailedCount = 2,
                Name = "SuperUser",
                Password = "super"
            };

            _userManager.AddUser(testUser);

            Assert.That(_userManager.CanLogin(testUser), Is.True);

            _userManager.RemoveUser(testUser);
        }

        [Test]
        public void MultipleUsersCanLoginAtTheSameTime()
        {

            var testUser1 = new User
            {
                AuthType = AuthTypes.Administrator,
                Description = CreateRandom.String(),
                Id = 4,
                LoggedIn = false,
                LoginFailedCount = 2,
                Name = CreateRandom.String(),
                Password = "super"
            };

            var testUser2 = new User
            {
                AuthType = AuthTypes.Engineer,
                Description = CreateRandom.String(),
                Id = 4,
                LoggedIn = false,
                LoginFailedCount = 0,
                Name = CreateRandom.String(),
                Password = "eng"
            };

            _userManager.AddUser(testUser1);
            _userManager.AddUser(testUser2);

            Assert.That(_userManager.CanLogin(testUser1),Is.True);
            Assert.That(_userManager.CanLogin(testUser2), Is.True);
            _userManager.LogIn(testUser1);
            _userManager.LogIn(testUser2);

            _userManager.RemoveUser(testUser1);
            _userManager.RemoveUser(testUser2);

        }

        [Test]
        public void SameUserCanNotLoginMoreThanOnce()
        {
            var testUser=new User
            {
                AuthType = AuthTypes.Administrator,
                Description = CreateRandom.String(),
                Id = 4,
                LoggedIn = false,
                LoginFailedCount = 0,
                Name = "SuperUser",
                Password = "super"
            };

            _userManager.AddUser(testUser);

            Assert.That(_userManager.CanLogin(testUser), Is.True);
            _userManager.LogIn(testUser);
            Assert.That(_userManager.CanLogin(testUser), Is.False);

            _userManager.RemoveUser(testUser);
        }

        [Test]
        public void ValidUserWithValidPasswordPassCredentialsChecking()
        {
            _userManager.AddUser(_adminUser);

            Assert.That(_userManager.CheckCredentials(_adminUser.Name, _adminUser.Password), Is.True);

            _userManager.RemoveUser(_adminUser);
        }

        [Test]
        public void ValidUserWithWrongPasswordFailsCredentialsChecking()
        {
            _userManager.AddUser(_engineerUser);

            var testUser = new User(_engineerUser);
            testUser.Password = CreateRandom.String();

            Assert.That(_userManager.CheckCredentials(testUser.Name, testUser.Password), Is.False);

            _userManager.RemoveUser(_engineerUser);

        }

        [Test]
        public void NonExistingUserTriesToLogInFails()
        {
            var testUser = new User
            {
                AuthType = AuthTypes.Operator,
                Description = CreateRandom.String(),
                Id = 99,
                LoggedIn = false,
                LoginFailedCount = 0,
                Name = CreateRandom.String(),
                Password = CreateRandom.String()
            };

            Assert.That(_userManager.CheckCredentials(testUser.Name, testUser.Password), Is.False);
            Assert.DoesNotThrow(() => _userManager.LogIn(testUser));
        }

        [Test]
        public void NonExistingUserTriesToLogOutFails()
        {
            var testUser = new User
            {
                AuthType = AuthTypes.Other,
                Description = CreateRandom.String(),
                Id = 99,
                LoggedIn = false,
                LoginFailedCount = 0,
                Name = CreateRandom.String(),
                Password = CreateRandom.String()
            };
            
            Assert.DoesNotThrow(()=>_userManager.LogOut(testUser.Name));
        }

        [Test]
        public void UserCanChangeHisPassword()
        {
            _userManager.AddUser(_operatorUser);
            string oldpassword = _operatorUser.Password;

            string newpassword = "newpassword";
            Assert.DoesNotThrow(() => _userManager.ChangePassword(_operatorUser.Name, newpassword));
            Assert.That(_userManager.CheckCredentials(_operatorUser.Name,newpassword), Is.True);
            Assert.DoesNotThrow(() => _userManager.ChangePassword(_operatorUser.Name, oldpassword));
            Assert.That(_userManager.CheckCredentials(_operatorUser.Name, oldpassword), Is.True);
            
            _userManager.RemoveUser(_operatorUser);
        }

        [Test]
        public void UserLoginAfterHeIntroducedMultipleTimesTheWrongPasswordFails()
        {
            var testUser = new User
            {
                AuthType = AuthTypes.Other,
                Description = CreateRandom.String(),
                Id = 10,
                LoggedIn = false,
                LoginFailedCount = 0,
                Name = CreateRandom.String(),
                Password = CreateRandom.String()
            };

            _userManager.AddUser(testUser);

            Assert.That(_userManager.CanLogin(testUser), Is.True);
            for (int i = 0; i < 5; i++)
                _userManager.CheckCredentials(testUser.Name, "testpassword");
            Assert.That(_userManager.CanLogin(testUser), Is.False);
           
            _userManager.RemoveUser(testUser);
        }

    }
}
