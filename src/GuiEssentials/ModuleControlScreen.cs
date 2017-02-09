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
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using VP.FF.PT.Common.Infrastructure.Logging;
using VP.FF.PT.Common.PlatformEssentials.Entities;
using VP.FF.PT.Common.PlatformEssentials.Entities.DTOs;
using VP.FF.PT.Common.WpfInfrastructure.CaliburnIntegration;
using VP.FF.PT.Common.WpfInfrastructure.Screens.Model;
using VP.FF.PT.Common.WpfInfrastructure.Screens.ViewModels;
using Message = VP.FF.PT.Common.WpfInfrastructure.Screens.Model.Message;

namespace VP.FF.PT.Common.GuiEssentials
{
    public abstract class ModuleControlScreen : BaseViewModel, IModuleScreen, IPartImportsSatisfiedNotification
    {
        private AlarmSummaryViewModel _alarmSummaryViewModel;
        protected MessageViewModel _messageViewModel;
        private int _overallItemCount;
        private int _currentItemCount;
        // Track whether Dispose has been called. 
        private bool _disposed;
        private int _downTime;
        private GenericPlcViewModel _genericPageViewModel;
        private bool _isEnabled = true;
        private ModuleState _definedModuleState;
        private bool _isPending;
        private bool _hideItemStream;
        private bool _hideAlarmSummary;
        private bool _hideStatistics;
        private bool _hideModuleControl;
        private bool _hideModuleState;
        private bool _canMinimizeAlarmSummary;
        private int _maxCapacity;
        private bool _canStandby;
        private ModuleState _moduleState;
        protected const string CommonErrorMessageSuggestionText = "Check communication, try to restart the client";

        private string _state;
        private string _subState;
        private double _throughput;
        private double _throughputMax;
        private double _throughputMin;
        private int _upTime;

        [Import(AllowDefault = true)]
        protected IIgnoreDownstreamModule _ignoreDownstreamModuleHandler;

        protected ModuleControlScreen()
        {
            SetupEnabledListener();
        }

        private void SetupEnabledListener()
        {
            // bridge between IModuleScreen and a specific ModuleControlScreen since the IModuleScreen works on the IsEnabled flag,
            // but the ModuleControlScreen performs additional operations on a DisableModule function.
            PropertyChanged += (sender, args) =>
            {
                Expression<Func<bool>> func = () => IsEnabled;
                var enabledProperty = func.GetMemberInfo().Name;
                if (args.PropertyName == enabledProperty && !IsEnabled)
                {
                    DisableModule(null, null);
                }
            };
        }

        public bool CanMinimizeAlarmSummary
        {
            get { return _canMinimizeAlarmSummary; }

            set
            {
                if (value != _canMinimizeAlarmSummary)
                {
                    _canMinimizeAlarmSummary = value;
                    NotifyOfPropertyChange(() => CanMinimizeAlarmSummary);
                }
            }
        }

        public bool HideItemStream
        {
            get { return _hideItemStream; }

            set
            {
                if (value != _hideItemStream)
                {
                    _hideItemStream = value;
                    NotifyOfPropertyChange(() => HideItemStream);
                }
            }
        }

        public bool HideAlarmSummary
        {
            get { return _hideAlarmSummary; }

            set
            {
                if (value != _hideAlarmSummary)
                {
                    _hideAlarmSummary = value;
                    NotifyOfPropertyChange(() => HideAlarmSummary);
                }
            }
        }

        public bool HideStatistics
        {
            get { return _hideStatistics; }

            set
            {
                if (value != _hideStatistics)
                {
                    _hideStatistics = value;
                    NotifyOfPropertyChange(() => HideStatistics);
                }
            }
        }

        public bool HideModuleControl
        {
            get { return _hideModuleControl; }

            set
            {
                if (value != _hideModuleControl)
                {
                    _hideModuleControl = value;
                    NotifyOfPropertyChange(() => HideModuleControl);
                }
            }
        }

        public bool HideModuleState
        {
            get { return _hideModuleState; }

            set
            {
                if (value != _hideModuleState)
                {
                    _hideModuleState = value;
                    NotifyOfPropertyChange(() => HideModuleState);
                }
            }
        }

        public AlarmSummaryViewModel AlarmSummaryViewModel
        {
            get { return _alarmSummaryViewModel; }

            set
            {
                if (_alarmSummaryViewModel != value)
                {
                    _alarmSummaryViewModel = value;
                    NotifyOfPropertyChange(() => AlarmSummaryViewModel);
                }
            }
        }

        private bool _canIgnoreDownstreamModule;

        public bool CanIgnoreDownstreamModule
        {
            get { return _canIgnoreDownstreamModule; }

            set
            {
                if (value != _canIgnoreDownstreamModule)
                {
                    _canIgnoreDownstreamModule = value;
                    NotifyOfPropertyChange(() => CanIgnoreDownstreamModule);
                }
            }
        }

        private bool _ignoreDownstreamModule;

        public virtual bool IgnoreDownstreamModule
        {
            get
            {
                return _ignoreDownstreamModule;
            }

            set
            {
                if (_ignoreDownstreamModuleHandler == null)
                    return;

                _ignoreDownstreamModuleHandler.SetIgnoreDowstreamModule(ModuleKey, value);
                _ignoreDownstreamModule = value;
                NotifyOfPropertyChange(() => IgnoreDownstreamModule);
            }
        }

        public int CurrentItemCount
        {
            get { return _currentItemCount; }

            set
            {
                if (value != _currentItemCount)
                {
                    _currentItemCount = value;
                    NotifyOfPropertyChange(() => CurrentItemCount);
                }
            }
        }

        public int OverallItemCount
        {
            get { return _overallItemCount; }

            set
            {
                if (value != _overallItemCount)
                {
                    _overallItemCount = value;
                    NotifyOfPropertyChange(() => OverallItemCount);
                }
            }
        }

        public override GenericPlcViewModel DetailViewModel
        {
            get { return _genericPageViewModel; }
        }

        public bool HasGenericPlcView { get { return DetailViewModel != null; } }

        public abstract Task Shutdown();

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

        public bool IsPending
        {
            get { return _isPending; }


            set
            {
                if (_isPending != value)
                {
                    _isPending = value;
                    NotifyOfPropertyChange(() => IsPending);
                }
            }
        }

        public int MaxCapacity
        {
            get { return _maxCapacity; }

            set
            {
                if (value != _maxCapacity)
                {
                    _maxCapacity = value;
                    NotifyOfPropertyChange(() => MaxCapacity);
                }
            }
        }

        public ModuleState ModuleState
        {
            get { return _moduleState; }

            set
            {
                if (value != _moduleState)
                {
                    _moduleState = value;
                    NotifyOfPropertyChange(() => ModuleState);
                }
            }
        }

        public string ModuleKey { get; set; }
        public int ModuleTypeId { get; set; }
        public int ModuleInstance { get; set; }

        public abstract int SortOrder { get; }

        /// <summary>
        /// Example states:
        /// RUN, STOP, STANDBY, ERROR, OFF, RUN_BUSY, STOP_BUSY, ERROR_BUSY, OFF_BUSY
        /// </summary>
        public string State
        {
            get { return _state; }

            set
            {
                if (value != _state)
                {
                    _state = value;
                    NotifyOfPropertyChange(() => State);
                }
            }
        }

        /// <summary>
        /// Prestates:
        /// ....... anything .......
        /// </summary>
        public string SubState
        {
            get { return _subState; }

            set
            {
                if (_subState != value)
                {
                    _subState = value;
                    NotifyOfPropertyChange(() => SubState);
                }
            }
        }

        public double Throughput
        {
            get { return _throughput; }

            set
            {
                if (!_throughput.Equals(value))
                {
                    _throughput = value;
                    NotifyOfPropertyChange(() => Throughput);
                }
            }
        }

        public double ThroughputMax
        {
            get { return _throughputMax; }

            set
            {
                if (!_throughputMax.Equals(value))
                {
                    _throughputMax = value;
                    NotifyOfPropertyChange(() => ThroughputMax);
                }
            }
        }

        public double ThroughputMin
        {
            get { return _throughputMin; }

            set
            {
                if (!_throughputMin.Equals(value))
                {
                    _throughputMin = value;
                    NotifyOfPropertyChange(() => ThroughputMin);
                }
            }
        }

        public int UpTime
        {
            get { return _upTime; }

            set
            {
                if (_upTime != value)
                {
                    _upTime = value;
                    NotifyOfPropertyChange(() => UpTime);
                }
            }
        }

        public int DownTime
        {
            get { return _downTime; }

            set
            {
                if (_downTime != value)
                {
                    _downTime = value;
                    NotifyOfPropertyChange(() => DownTime);
                }
            }
        }

        public MessageViewModel MessageViewModel
        {
            get { return _messageViewModel ?? (_messageViewModel = new MessageViewModel()); }
        }

        public async void SendUserNotification(Common.PlatformEssentials.MessageType type, string message, TimeSpan duration)
        {
            SendMessage(type, message);
            await Task.Delay(duration);
            RemoveMessage(type, message);
        }

        public void SendMessage(Common.PlatformEssentials.MessageType type, string message)
        {
            var messageType = (MessageType)type;
            var m = new Message(messageType, message, 1);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (MessageViewModel.AddMessageCommand.CanExecute(m))
                {
                    MessageViewModel.AddMessageCommand.Execute(m);
                }
            });
        }

        public void RemoveMessage(Common.PlatformEssentials.MessageType type, string message)
        {
            var messageType = (MessageType)type;
            var m = new Message(messageType, message, 1);
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_messageViewModel.RemoveMessageCommand.CanExecute(m))
                {
                    _messageViewModel.RemoveMessageCommand.Execute(m);
                }
            });
        }

        /// <summary>
        /// Called when activating.
        /// </summary>
        protected override async void OnActivate()
        {
            base.OnActivate();

            if (AlarmSummaryViewModel != null)
            {
                await AlarmSummaryViewModel.Activate();
            }
        }

        /// <summary>
        /// Called when deactivating.
        /// </summary>
        /// <param name="close">Inidicates whether this instance will be closed.</param>
        protected async override void OnDeactivate(bool close)
        {
            RemoveMessage(Common.PlatformEssentials.MessageType.Error,
                "The module was not properly initialized.\n Please restart the application!");
            base.OnDeactivate(close);
            if (AlarmSummaryViewModel != null)
                await AlarmSummaryViewModel.Deactivate();
        }

        /// <summary>
        /// Creates a task which handles WCF client and establish connection (optional).
        /// Put other critical/potential error stuff into this task. Do NOT use the constructor for initialization.
        /// The application will continue to run, even if an exception is thrown in the initialize phase, but just the failed module disabled (and log entries).
        /// The application will crash if an exception is thrown in the constructor, so use Initialize()!
        /// </summary>
        /// <remarks>
        /// To improve startup performance head module has to provide this initialization Task.
        /// </remarks>
        public abstract Task Initialize();

        protected void SetGenericPlcViewModel(GenericPlcViewModel viewModel)
        {
            if (_genericPageViewModel != viewModel)
            {
                _genericPageViewModel = viewModel;
                NotifyOfPropertyChange(() => DetailViewModel);
                NotifyOfPropertyChange(() => HasGenericPlcView);
            }
        }

        public virtual void Start()
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void Standby()
        {

        }

        public bool CanStandby
        {
            get { return _canStandby; }

            set
            {
                if (value != _canStandby)
                {
                    _canStandby = value;
                    NotifyOfPropertyChange(() => CanStandby);
                }
            }
        }

        public virtual void ResetAlarms()
        {
        }

        public virtual void RefreshPlatformItems()
        {
        }

        public virtual bool AutomaticRefreshPlatformItems { get; set; }

        protected void RefreshPlatformModule(PlatformModuleDTO module)
        {
            State = module.State.ToString().ToUpper();

            if (!string.IsNullOrEmpty(module.SubState))
                SubState = module.SubState.Replace("cSTA_", string.Empty).Replace("SUB_", string.Empty);
            else
                SubState = string.Empty;

            if (module.State == PlatformModuleState.Run)
                ModuleState = ModuleState.Run;
            else if (module.State == PlatformModuleState.RunBusy)
                ModuleState = ModuleState.RunBusy;
            else if (module.State == PlatformModuleState.OffBusy)
                ModuleState = ModuleState.OffBusy;
            else if (module.State == PlatformModuleState.Standby)
                ModuleState = ModuleState.Standby;
            else if (module.State == PlatformModuleState.StandbyBusy)
                ModuleState = ModuleState.StandbyBusy;
            else
                ModuleState = ModuleState.Off;

            MaxCapacity = module.MaxCapacity;
            CurrentItemCount = module.PlatformItems.Count;
        }

        public void DisableModule(Exception e, ILogger logger)
        {
            IsEnabled = false;

            // there are times when this method is called from the IsEnabled property changed event (part of this class)
            // and the logger is only known by subclasses. Additionally, there wouldn't be an exception from the IsEnabled
            // changed and the actual reason should have been logged by the code that initiated the IsEnabled changed.
            // Todo: Bigger topic: Move the DisableModule to the interface, or have this abstract class know of the logger, or push this method down, or ...
            if (logger != null)
            {
                logger.Error("No keep alive to server! Module disabled.", e);
            }
            ShowFatalErrorMessage("No keep alive to server! Module disabled.", CommonErrorMessageSuggestionText, e);

            SendMessage(Common.PlatformEssentials.MessageType.Error,
                "The module was not properly initialized.\n Please check the logs and restart the application!");
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// Show when something unplanned happens, e.g. unhandled exceptions.
        /// The user might need to restart the client.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        protected void ShowFatalErrorMessage(string title, string text)
        {
            // TODO: show dialog
        }

        /// <summary>
        /// Show when something unplanned happens, e.g. unhandled exceptions.
        /// The user might need to restart the client.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="e"></param>
        protected void ShowFatalErrorMessage(string title, string text, Exception e)
        {
            // TODO: show dialog
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        ~ModuleControlScreen()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        public void OnImportsSatisfied()
        {
            if (_ignoreDownstreamModuleHandler == null)
                return;

            _ignoreDownstreamModuleHandler.ResetIgnoreDownstreamModuleEvent += (sender, args) =>
            {
                _ignoreDownstreamModule = false;
                NotifyOfPropertyChange(() => IgnoreDownstreamModule);
            };
        }
    }
}
