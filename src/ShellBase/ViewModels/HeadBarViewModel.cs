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
using System.Windows.Threading;
using Caliburn.Micro;
using VP.FF.PT.Common.PlatformEssentials.UserManagement;
using VP.FF.PT.Common.WpfInfrastructure.Screens.ViewModels;

namespace VP.FF.PT.Common.ShellBase.ViewModels
{
    public class HeadBarViewModel : Screen
    {
        private BreadcrumbBarViewModel _breadcrumbBarViewModel;
        private DateTime _date;
        private AuthTypes _role;
        private string _userName;
        private IScreen _homeScreen;

        public HeadBarViewModel()
        {
            _breadcrumbBarViewModel = new BreadcrumbBarViewModel();

            _userName = "Dummy User";
            _role = AuthTypes.Other;

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DateTimeUpdater;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
        }

        private void DateTimeUpdater(object sender, EventArgs e)
        {
            Date = DateTime.Now;
        }

        public DateTime Date
        {
            get
            {
                return _date;
            }

            set
            {
                if (_date != value)
                {
                    _date = value;
                    NotifyOfPropertyChange(() => Date);
                }
            }
        }

        public AuthTypes Role
        {
            get { return _role; }
            set
            {
                if (_role != value)
                {
                    _role = value;
                    NotifyOfPropertyChange(() => Role);
                }
            }
        }

        public String UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    NotifyOfPropertyChange(() => UserName);
                }
            }
        }

        public BreadcrumbBarViewModel BreadcrumbBarViewModel
        {
            get { return _breadcrumbBarViewModel; }

            set
            {
                if (_breadcrumbBarViewModel != value)
                {
                    _breadcrumbBarViewModel = value;
                    NotifyOfPropertyChange(() => BreadcrumbBarViewModel);
                }
            }
        }

        public IScreen HomeScreenViewModel
        {
            get { return _homeScreen; }
            set
            {
                _homeScreen = value;
                NotifyOfPropertyChange(() => HomeScreenViewModel);
            }
        }
    }
}
