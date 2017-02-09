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


using System.ComponentModel.Composition;
using System.ServiceModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.WpfInfrastructure;
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;
using VP.FF.PT.Common.WpfInfrastructure.Screens.ViewModels;

namespace VP.FF.PT.Common.ShellBase.ViewModels
{
    [CallbackBehavior]
    public class HomeScreenBaseViewModel : BaseViewModel, IModuleScreen
    {
        protected ILogger _logger;
        private bool _isEnabled;

        [ImportingConstructor]
        public HomeScreenBaseViewModel(ILogger logger, IEventAggregator eventAggregator)
        {
            DisplayName = "Home";

            _logger = logger;
            _logger.Init(GetType());
            _logger.Info("CTOR of HomeScreenBaseViewModel called");
            eventAggregator.Subscribe(this);

        }

        public HomeScreenBaseViewModel()
        {
            //TODO: Remove test data
            if (DesignTimeHelper.IsInDesignModeStatic)
            {
                // UseTestData();
            }
        }

        /// <summary>
        /// Activates this instance.
        /// </summary>
        public virtual void Activate()
        {
            
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Inidicates whether this instance will be closed.</param>
        protected override void OnDeactivate(bool close)
        {
           
        }

        public virtual Task Initialize()
        {
            return Task.FromResult(0);
        }

        public virtual Task Shutdown()
        {
            return Task.FromResult(0);
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }

            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    NotifyOfPropertyChange(() => IsEnabled);
                }
            }
        }

        public string ModuleKey { get { return "homescreen"; } set { } }
        public int ModuleTypeId { get; set; }
        public int ModuleInstance { get; set; }

        // Homescreen is the FIRST screen
        public int SortOrder { get { return -999999; } }

        public virtual void Dispose()
        {
          
        }

        public override string IconKey { get { return "Home"; } }

        public override GenericPlcViewModel DetailViewModel { get { return null; } }
    }
}
