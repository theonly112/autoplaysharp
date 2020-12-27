using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Emulators;
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace autoplaysharp.App.UI.Setup
{
    internal class SetupViewModel : INotifyPropertyChanged
    {
        private readonly ILogger _logger;
        private readonly ITextRecognition _textRecognition;
        private readonly ISettings _settings;
        private string _selectedWindow;
        public ObservableCollection<EmulatorType> EmulatorTypes { get; }
        public EmulatorType SelectedEmulator { get; set; }

        public ICommand TryFindEmulator
        {
            get;
        }

        public ObservableCollection<string> Windows { get; }

        public string SelectedWindow
        {
            get => _selectedWindow;
            set
            {
                _selectedWindow = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand SaveSettings { get; }

        public event Action Saved;

        public SetupViewModel(ILogger logger, ITextRecognition textRecognition, ISettings settings)
        {
            _logger = logger;
            _textRecognition = textRecognition;
            _settings = settings;
            EmulatorTypes = new ObservableCollection<EmulatorType>(Enum.GetValues<EmulatorType>());
            Windows = new ObservableCollection<string>();
            TryFindEmulator = new DelegateCommand(ExecuteTryFindEmulator);
            SaveSettings = new DelegateCommand(ExecuteCloseAndSave, CanSaveAndClose);
            SelectedEmulator = settings.EmulatorType;
        }

        private bool CanSaveAndClose()
        {
            return !string.IsNullOrWhiteSpace(SelectedWindow);
        }

        private void ExecuteCloseAndSave()
        {
            _settings.WindowName = SelectedWindow;
            _settings.EmulatorType = SelectedEmulator;
            Saved?.Invoke();
        }

        private void ExecuteTryFindEmulator()
        {
            IReadOnlyList<string> possibleWindows;
            if (SelectedEmulator == EmulatorType.BlueStacks)
            {
                var blueStacks = new BluestacksWindow(_logger, _textRecognition, null);
                possibleWindows = blueStacks.FindPossibleWindows().ToList();
            }
            else
            {
                var nox = new NoxWindow(null);
                possibleWindows = nox.FindPossibleWindows().ToList();
            }

            if (!possibleWindows.Any())
            {
                MessageBox.Show("Could not find any emulator window. Are you sure its running?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Windows.Clear();
            foreach (var findPossibleWindow in possibleWindows)
            {
                Windows.Add(findPossibleWindow);
            }

            SelectedWindow = Windows.FirstOrDefault();
            SaveSettings.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
