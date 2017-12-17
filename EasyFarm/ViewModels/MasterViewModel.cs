﻿// ///////////////////////////////////////////////////////////////////
// This file is a part of EasyFarm for Final Fantasy XI
// Copyright (C) 2013-2017 Mykezero
// 
// EasyFarm is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// EasyFarm is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// ///////////////////////////////////////////////////////////////////

using System;
using System.Windows.Input;
using EasyFarm.Classes;
using EasyFarm.Infrastructure;
using EasyFarm.Logging;
using EasyFarm.Persistence;
using EasyFarm.UserSettings;
using MediatR;
using Prism.Commands;
using Application = System.Windows.Application;

namespace EasyFarm.ViewModels
{
    /// <summary>
    ///     The view model for the main window.
    /// </summary>
    public class MasterViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        ///     Saves and loads settings from file.
        /// </summary>
        private readonly SettingsManager _settingsManager;

        /// <summary>
        ///     The text displayed on the start / pause button.
        /// </summary>
        private string _startStopHeader = "St_art";

        /// <summary>
        /// The view's main body content.
        /// </summary>
        public IViewModel ViewModel { get; set; }

        public MasterViewModel(MainViewModel mainViewModel, IMediator mediator)
        {
            _mediator = mediator;
            ViewModel = mainViewModel;

            _settingsManager = new SettingsManager("eup", "EasyFarm User Preference");

            AppServices.RegisterEvent<Events.TitleEvent>(e => MainWindowTitle = e.Message);
            AppServices.RegisterEvent<Events.StatusBarEvent>(e => StatusBarText = e.Message);
            AppServices.RegisterEvent<Events.PauseEvent>(x => StopEngine());
            AppServices.RegisterEvent<Events.ResumeEvent>(x => StartEngine());

            StartCommand = new DelegateCommand(Start);
            ExitCommand = new DelegateCommand(Exit);
            SaveCommand = new DelegateCommand(Save);
            LoadCommand = new DelegateCommand(Load);
            SelectProcessCommand = new DelegateCommand(SelectProcess);

            OnLoad();
        }

        private string _mainWindowTitle;

        public string MainWindowTitle
        {
            get { return _mainWindowTitle; }
            set { SetProperty(ref _mainWindowTitle, value); }
        }

        private string _statusBarText;

        public string StatusBarText
        {
            get { return _statusBarText; }
            set { SetProperty(ref _statusBarText, value); }
        }

        /// <summary>
        ///     Tells whether the bot is working or not.
        /// </summary>
        public bool IsWorking
        {
            get { return GameEngine.IsWorking; }
            set { SetProperty(ref GameEngine.IsWorking, value); }
        }

        /// <summary>
        ///     Binding for File -> Start/Pause text.
        /// </summary>
        public string StartPauseHeader
        {
            get { return _startStopHeader; }
            set { SetProperty(ref _startStopHeader, value); }
        }

        public bool MinimizeToTray
        {
            get { return Config.Instance.MinimizeToTray; }
            set
            {
                bool instanceMinimizeToTray = false;
                SetProperty(ref instanceMinimizeToTray, value);
                Config.Instance.MinimizeToTray = instanceMinimizeToTray;
            }
        }

        /// <summary>
        ///     Command to start the bot.
        /// </summary>
        public ICommand StartCommand { get; set; }

        /// <summary>
        ///     Command to shut down the program.
        /// </summary>
        public ICommand ExitCommand { get; set; }

        /// <summary>
        ///     Command to save the user's settings.
        /// </summary>
        public DelegateCommand SaveCommand { get; set; }

        /// <summary>
        ///     Command to load the user's settings.
        /// </summary>
        public DelegateCommand LoadCommand { get; set; }

        /// <summary>
        ///     Binding for selecting a PlayOnline process.
        /// </summary>
        public DelegateCommand SelectProcessCommand { get; set; }

        /// <summary>
        ///     Tells the program to start farming.
        /// </summary>
        public void Start()
        {
            // Return when the user has not selected a process.
            if (FFACE == null)
            {
                AppServices.InformUser("No process has been selected.");
                return;
            }

            if (GameEngine.IsWorking)
            {
                StopEngine();
            }
            else
            {
                StartEngine();
            }
        }

        private void StartEngine()
        {
            LogViewModel.Write("Bot now running");
            AppServices.InformUser("Program running.");
            var isStarted = GameEngine.Start();
            if (!isStarted) return;
            StartPauseHeader = "P_ause";
        }

        private void StopEngine()
        {
            LogViewModel.Write("Bot now paused");
            AppServices.InformUser("Program paused.");
            StartPauseHeader = "St_art";
            GameEngine.Stop();
        }

        /// <summary>
        ///     Saves the player's data to file.
        /// </summary>
        private void Save()
        {
            try
            {
                _settingsManager.TrySave(Config.Instance);
                AppServices.InformUser("Settings have been saved.");
                LogViewModel.Write("Settings saved");
            }
            catch (InvalidOperationException ex)
            {
                AppServices.InformUser("Failed to save settings.");
                Logger.Log(new LogEntry(LoggingEventType.Error, $"{GetType()}.{nameof(Save)} : Failure on save settings", ex));
            }
        }

        /// <summary>
        ///     Loads easyfarm settings from file.
        /// </summary>
        private void Load()
        {
            try
            {
                // Load the settings.
                var settings = _settingsManager.TryLoad<Config>();

                // Did we fail to load the settings?
                if (settings == null)
                {
                    AppServices.InformUser("Failed to load settings.");
                    return;
                }

                // Inform the user of our success.
                Config.Instance = settings;
                AppServices.InformUser("Settings have been loaded.");
                LogViewModel.Write("Settings loaded");
            }
            catch (InvalidOperationException ex)
            {
                AppServices.InformUser("Failed to load settings.");
                Logger.Log(new LogEntry(LoggingEventType.Error, $"{GetType()}.{nameof(Load)} : Failed to load settings", ex));
            }
        }

        /// <summary>
        ///     Selects a process to user for this application.
        /// </summary>
        private void SelectProcess()
        {
            _mediator.Send(new SelectProcessRequest())
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        ///     Shutsdown the program.
        /// </summary>
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        public void OnLoad()
        {
            MainWindowTitle = "EasyFarm";
            StatusBarText = "";
        }
    }
}
