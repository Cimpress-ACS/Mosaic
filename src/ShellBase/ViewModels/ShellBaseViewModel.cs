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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using Centigrade.Kit.StateMachine;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;
using VP.FF.PT.Common.WpfInfrastructure.ScreenActivation;
using VP.FF.PT.Common.WpfInfrastructure.Screens.Model;
using VP.FF.PT.Common.WpfInfrastructure.Screens.ViewModels;
using Timer = System.Timers.Timer;

namespace VP.FF.PT.Common.ShellBase.ViewModels
{
    public abstract class ShellBaseViewModel : Conductor<IScreen>.Collection.OneActive, IShell
    {
        #region States

        public IState LoginViewModelState
        {
            get { return StateMachine.GetState<LoginBaseViewModel.LoginDefaultState>(); }
        }

        public IState ShellViewModelState
        {
            get { return StateMachine.GetState<ShellState>(); }
        }

        public IState LogoutViewModelState
        {
            get { return StateMachine.GetState<LogoutState>(); }
        }

        #region Nested type: LogoutState

        public class LogoutState : State
        {
        }

        #endregion

        #region Nested type: ShellState

        public class ShellState : State
        {
        }

        #endregion

        #endregion

        [Import]
        protected CompositionContainer _container = null;

        [Import] 
        protected IModuleScreenRepository ModuleRepository;

        private LoadingScreenViewModel _loadingScreenViewModel;
        private StateMachine _stateMachine;
        private HeadBarViewModel _headBarViewModel;
        private AdminConsoleViewModel _adminConsoleViewModel;
        protected LoginViewModel _loginViewModel;
        private MessageViewModel _messageViewModel;

	#pragma warning disable 0649
        private HomeScreenBaseViewModel _homeScreen; // TODO: This is never assigned to
	#pragma warning restore 0649

        protected IProvideStatesForScreenActivation _states;
        private IStateTransitionCommand _loginCommand;
        protected ILogger _logger;
        protected IEventAggregator _eventAggregator;
        protected IModuleScreen _currentScreen;
        private string _moduleInInitialization;
       
        private bool _isEngineer;
        private bool _isAdministrator;
        private bool _hasGenericPlcView;
        private string _title;

        protected ShellBaseViewModel(
            IProvideStatesForScreenActivation states,
            ILogger logger,
            IEventAggregator eventAggregator)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionHandler;

            CacheViewsByDefault = true;

            _logger = logger;
            _logger.Info("starting Shell now...");
            _states = states;

            _eventAggregator = eventAggregator;
        }

        protected void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error("Application crash due to unhandled exception!");
            _logger.Error(e.ExceptionObject.ToString());
        }

        public string ModuleInInitialization
        {
            get { return _moduleInInitialization; }
            set
            {
                if (_moduleInInitialization != value)
                {
                    _moduleInInitialization = value;
                    NotifyOfPropertyChange(() => ModuleInInitialization);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current active screen.
        /// </summary>

        public IProvideStatesForScreenActivation States
        {
            get { return _states; }
        }

        protected void SendMessage(MessageType type, string message)
        {
            var messageType = type;
            var m = new WpfInfrastructure.Screens.Model.Message(messageType, message, 1);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_messageViewModel.AddMessageCommand.CanExecute(m))
                {
                    _messageViewModel.AddMessageCommand.Execute(m);
                }
            });
        }

        protected void RemoveMessage(MessageType type, string message)
        {
            var messageType = type;
            var m = new Common.WpfInfrastructure.Screens.Model.Message(messageType, message, 1);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_messageViewModel.RemoveMessageCommand.CanExecute(m))
                {
                    _messageViewModel.RemoveMessageCommand.Execute(m);
                }
            });
        }

        //when the window closes we need to logout the user otherwise it will remain logged in and can not be used anymore
        protected void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                Logout();
            }
            catch (FaultException)
            {
                //User logged out by himself; nothing to do
            }
        }

        public  string Title
        {
            get { return _title; }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyOfPropertyChange(() => Title);
                }
            }
        }

        public virtual bool AllModulesInitialized
        {
            get { return true; }
            set { }
        }

        public StateMachine StateMachine
        {
            get
            {
                if (_stateMachine == null)
                {
                    _stateMachine = new StateMachine();
                    _stateMachine.StateChanged +=
                        (sender, args) => { Console.WriteLine("CurrentState: " + StateMachine.CurrentState); };
                    _stateMachine.GoToState<LoginBaseViewModel.LoginDefaultState>();
                }
                return _stateMachine;
            }
        }

        public LoadingScreenViewModel LoadingScreenViewModel
        {
            get { return _loadingScreenViewModel ?? (_loadingScreenViewModel = new LoadingScreenViewModel(_states)); }
        }

        public HeadBarViewModel HeadBarViewModel
        {
            get { return _headBarViewModel ?? (_headBarViewModel = new HeadBarViewModel()); }
        }

        public AdminConsoleViewModel AdminConsoleViewModel
        {
            get
            {
                if (_adminConsoleViewModel == null)
                {
                    _adminConsoleViewModel = new AdminConsoleViewModel();
                }

                return _adminConsoleViewModel;
            }
        }

        public ICommand AdminCommand
        {
            get
            {
                return new DelegateCommand(() => AdminConsoleViewModel.Animation(HeadBarViewModel.Role.ToString()));
            }
        }

        public LoginBaseViewModel LoginViewModel
        {
            get { return _loginViewModel ?? (_loginViewModel = new LoginViewModel(StateMachine, LoginCommand)); }
        }

        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ??
                       (_loginCommand =
                           StateMachine
                               .GetStateTransitionCommand<LoginBaseViewModel.LoginDefaultState, ShellState, object>
                               (param => Login(), param => CanLogin()));
            }
        }

        public virtual bool CanLogin()
        {
            return true;
        }

        public virtual void Login()
        {

        }

        public ICommand LogoutCommand
        {
            get
            {
                return StateMachine.GetStateTransitionCommand<ShellState, LogoutState, object>(param =>
                {
                    var timer = new Timer(1500);
                    var dispatcher = Dispatcher.CurrentDispatcher;
                    timer.Elapsed += (sender, args) => dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        StateMachine.GoToState<LoginBaseViewModel.LoginDefaultState>();
                        timer.Close();
                        timer.Dispose();
                        timer = null;
                    }), DispatcherPriority.Background, null);
                    timer.AutoReset = false;

                    // start timer while logging out and change states
                    timer.Start();
                    Logout();

                }, param => CanLogout());
            }
        }

        public virtual bool CanLogout()
        {
            return true;
        }
        
        public virtual void Logout()
        {
            NavigateToHome();
            StateMachine.GoToState<LoginBaseViewModel.LoginDefaultState>();
        }

        public bool IsEngineer
        {
            get { return _isEngineer; }
            set
            {
                if (_isEngineer != value)
                {
                    _isEngineer = value;
                    NotifyOfPropertyChange(() => IsEngineer);
                }
            }
        }

        public bool IsAdministrator
        {
            get { return _isAdministrator; }
            set
            {
                if (_isAdministrator != value)
                {
                    _isAdministrator = value;
                    NotifyOfPropertyChange(() => IsAdministrator);
                }
            }
        }

        public bool HasGenericPlcView
        {
            get { return _hasGenericPlcView; }

            set
            {
                if (_hasGenericPlcView != value)
                {
                    _hasGenericPlcView = value;
                    NotifyOfPropertyChange(() => HasGenericPlcView);
                }
            }
        }

        public virtual bool IsHomeScreenActive
        {
            get { return false; }
            set { }
        }

        public MessageViewModel MessageViewModel
        {
            get { return _messageViewModel ?? (_messageViewModel = new MessageViewModel()); }
        }

        public void NavigateToDetail()
        {
            var viewModel = CurrentScreen as BaseViewModel;
            if (viewModel != null)
                if (viewModel.DetailViewModel != null)
                    NavigateToScreen(viewModel.DetailViewModel);
        }

        public void NavigateBack()
        {
            var genericPlcView = CurrentScreen as GenericPlcViewModel;
            if (genericPlcView != null)
            {
                NavigateToScreen(genericPlcView.ParentViewModel);
                //NavigateToScreen(BreadcrumbBarViewModel.BreadcrumbBarItems[1].RelatedScreen);
            }
            else
            {
                NavigateToHome();
            }
        }

        /// <summary>
        /// Looks for the navigation screen and navigates to it.
        /// </summary>
        public virtual void NavigateToConfiguration()
        {
            
        }

        public virtual void NavigateToHome()
        {
            NavigateToScreen(_homeScreen);
            AdminConsoleViewModel.LoadSession(this, "BaseScreen");
        }

        public void NavigateToScreen(IModuleScreen screen)
        {
            CurrentScreen = screen;
        }

        public void NavigateToScreen(string moduleKey)
        {
            try
            {
                var screen = ModuleRepository.GetModule(moduleKey);
                CurrentScreen = screen;
                AdminConsoleViewModel.CurrentModuleKey = moduleKey;
                _eventAggregator.Publish(AdminConsoleViewModel);
            }
            catch (Exception e)
            {
                _logger.ErrorFormat("Can't navigate to screen {0}", moduleKey, e);
            }
        }

        public virtual IModuleScreen CurrentScreen
        {
            get { return _currentScreen; }

            set
            {
                if (_currentScreen != value)
                {
                    bool hasGeneric = false;
                    if (value is GenericPlcViewModel)
                    {
                        HeadBarViewModel.BreadcrumbBarViewModel.ToDetailScreen(_currentScreen, value as GenericPlcViewModel);
                    }
                    else
                    {
                        HeadBarViewModel.BreadcrumbBarViewModel.ToModuleScreen(value);
                        var baseViewModel = value as BaseViewModel;
                        if (baseViewModel != null)
                        {
                            hasGeneric = baseViewModel.DetailViewModel != null;
                        }
                    }
                    HasGenericPlcView = hasGeneric;
                    _currentScreen = value;
                    ActiveItem = _currentScreen;
                    NotifyOfPropertyChange(() => CurrentScreen);
                    NotifyOfPropertyChange(() => IsHomeScreenActive);
                }
            }
        }

        protected override void OnInitialize()
        {
            _logger.Debug(string.Format("Initialize {0}", this));
            base.OnInitialize();
            TryInitializeModules();
        }

        public virtual void TryInitializeModules()
        {
            _logger.Debug(string.Format("{0} tries to initialize the view models", this));
            _states.ChangeToLoadingState();
            try
            {
                Action<string> updateInitializationMessage = m => ModuleInInitialization = m;
                InitializeScreens();
                HeadBarViewModel.BreadcrumbBarViewModel = new BreadcrumbBarViewModel(_homeScreen);
                NavigateToHome();
                _states.ChangeToContentState();
                LoginCommand.CanExecute(null);
                _logger.Debug(string.Format("{0} successfully initialized the modules.", this));

            }
            catch (Exception exception)
            {
                string errorMessage = new StringBuilder()
                    .AppendLine("An error occured while intializing the Remote UI:")
                    .Append(exception).ToString();
                _logger.ErrorFormat("{0} failed to initalize the modules: '{1}'", this, errorMessage);
                _states.ChangeToErrorState(errorMessage);
            }
        }

        protected override void OnDeactivate(bool close)
        {
            foreach (var moduleScreen in ModuleRepository.Modules)
            {
                try
                {
                    moduleScreen.Shutdown();
                }
                catch (Exception e)
                {
                    _logger.Error("Could not shutdown module " + moduleScreen.DisplayName, e);
                }
            }

            base.OnDeactivate(close);
        }

        public virtual double ScalingFactor
        {
            get { return 1.0; }
        }

        protected virtual void InitializeScreens()
        {
        }

        protected void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ScalingFactor")
            {
                NotifyOfPropertyChange(() => ScalingFactor);
                _logger.Debug("ScalingFactor changed to " + ScalingFactor);
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0}#{1}", GetType().Name, GetHashCode());
        }
    }
}
