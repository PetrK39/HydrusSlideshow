using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydrus_Slideshow.Services;
using PreferenceManagerLibrary.Preferences.Base;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Hydrus_Slideshow.ViewModels
{
    [INotifyPropertyChanged]
    public partial class ConfigViewModel
    {
        private readonly IConfigService configService;

        public event EventHandler? OnCloseRequest;
        public ObservableCollection<PreferenceBase> Preferences => configService.PreferenceManager.Preferences;

        public ConfigViewModel(IConfigService configService)
        {
            this.configService = configService;
            configService.PreferenceManager.BeginEdit();
            configService.PreferenceManager.OnIsEditableValidChanged += (_, _) => SaveCommand.NotifyCanExecuteChanged();
        }
        
        [DesignOnly(true)]
        public ConfigViewModel()
        {
            configService = new MainConfigService();
        }

        [RelayCommand(CanExecute = nameof(SaveCommandCanExecute))]
        public void Save()
        {
            configService.PreferenceManager.EndEdit();

            configService.PreferenceManager.SavePreferences();

            OnCloseRequest?.Invoke(this, EventArgs.Empty);
        }
        [RelayCommand]
        public void Defaults()
        {
            configService.PreferenceManager.CancelEdit();
            configService.PreferenceManager.DefaultPreferences();
            configService.PreferenceManager.BeginEdit();
        }
        [RelayCommand]
        public void Cancel()
        {
            configService.PreferenceManager.CancelEdit();
            OnCloseRequest?.Invoke(this, EventArgs.Empty);
        }
        public bool SaveCommandCanExecute() => configService.PreferenceManager.IsEditableValid;
    }
}
