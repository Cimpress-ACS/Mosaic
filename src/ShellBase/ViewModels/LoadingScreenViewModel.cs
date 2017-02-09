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


using Caliburn.Micro;
using VP.FF.PT.Common.WpfInfrastructure.ScreenActivation;

namespace VP.FF.PT.Common.ShellBase.ViewModels
{
    public class LoadingScreenViewModel : Screen
    {
        private IProvideStatesForScreenActivation _states;
        private string _moduleInInitialization;

        public LoadingScreenViewModel(IProvideStatesForScreenActivation states)
        {
            _states = states;
        }

        public LoadingScreenViewModel()
        {
        }

        public string ModuleInInitialization
        {
            get { return null; }
            set
            {
                if (value != _moduleInInitialization)
                {
                    _moduleInInitialization = value;
                    NotifyOfPropertyChange(() => ModuleInInitialization);
                }
            }
        }
    }
}
