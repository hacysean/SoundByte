﻿using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using SoundByte.Core.Items;
using SoundByte.UWP.Dialogs;
using SoundByte.UWP.Helpers;
using SoundByte.UWP.Services;
using SoundByte.UWP.ViewModels;

namespace SoundByte.UWP.Views.Settings
{
    /// <summary>
    ///     This is the main settings/about page for the app.
    ///     is handled here
    /// </summary>
    public sealed partial class SettingsView
    {
        // View model for the settings page
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        /// <summary>
        ///     Setup the page
        /// </summary>
        public SettingsView()
        {
            // Initialize XAML Components
            InitializeComponent();
            // Set the datacontext
            DataContext = ViewModel;
            // Page has been unloaded from UI
            Unloaded += (s, e) => ViewModel.Dispose();
        }

        // The settings object, we bind to this to change values
        public SettingsService SettingsService { get; set; } = SettingsService.Instance;

        /// <summary>
        ///     Called when the user navigates to the page
        /// </summary>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Track Event
            App.Telemetry.TrackPage("Info View");
            // TEMP, Load the page
            LoadSettingsPage();

            // Set the app version
            AppVersion.Text = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}";
            AppBuild.Text = $"{Package.Current.Id.Version.Build}";
            AppId.Text = SettingsService.AppId;

            AppBuildTime.Text = "...";

            var dataFile = await Package.Current.InstalledLocation.GetFileAsync(@"Assets\build_info.json");
            var buildData =
                await Task.Run(() => JsonConvert.DeserializeObject<BuildInformation>(File.ReadAllText(dataFile.Path)));

            AppBuildTime.Text = buildData.BuildTime;
        }

        public async void NavigateNew()
        {
            await NavigationService.Current.CallDialogAsync<WhatsNewDialog>();
        }

        /// <summary>
        ///     Called when the user taps on the rate_review button
        /// </summary>
        public async void RateAndReview()
        {
            App.Telemetry.TrackPage("Rate and Review App");

            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store:REVIEW?PFN={Package.Current.Id.FamilyName}"));
        }

        private void LoadSettingsPage()
        {
            ViewModel.IsComboboxBlockingEnabled = true;
            // Get the saved language
            var appLanguage = SettingsService.Instance.CurrentAppLanguage;
            // Check that the string is not empty
            if (!string.IsNullOrEmpty(appLanguage))
                switch (appLanguage)
                {
                    case "en-US":
                        LanguageComboBox.SelectedItem = Language_English_US;
                        break;
                    case "fr":
                        LanguageComboBox.SelectedItem = Language_French_FR;
                        break;
                    case "nl":
                        LanguageComboBox.SelectedItem = Language_Dutch_NL;
                        break;
                    default:
                        LanguageComboBox.SelectedItem = Language_English_US;
                        break;
                }
            else
                LanguageComboBox.SelectedItem = Language_English_US;

            switch (SettingsService.Instance.ApplicationThemeType)
            {
                case AppTheme.Default:
                    ThemeComboBox.SelectedItem = DefaultTheme;
                    break;
                case AppTheme.Light:
                    ThemeComboBox.SelectedItem = LightTheme;
                    break;
                case AppTheme.Dark:
                    ThemeComboBox.SelectedItem = DarkTheme;
                    break;
                default:
                    ThemeComboBox.SelectedItem = DefaultTheme;
                    break;
            }

            // Enable combo boxes
            ViewModel.IsComboboxBlockingEnabled = false;
        }

        private void AppThemeComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.IsComboboxBlockingEnabled)
                return;

            switch (((ComboBoxItem) (sender as ComboBox)?.SelectedItem)?.Name.ToLower())
            {
                case "defaulttheme":
                    SettingsService.Instance.ApplicationThemeType = AppTheme.Default;
                    ((AppShell) Window.Current.Content).RequestedTheme = ElementTheme.Default;
                    break;
                case "darktheme":
                    SettingsService.Instance.ApplicationThemeType = AppTheme.Dark;
                    ((AppShell) Window.Current.Content).RequestedTheme = ElementTheme.Dark;
                    break;
                case "lighttheme":
                    SettingsService.Instance.ApplicationThemeType = AppTheme.Light;
                    ((AppShell) Window.Current.Content).RequestedTheme = ElementTheme.Light;
                    break;
                default:
                    SettingsService.Instance.ApplicationThemeType = AppTheme.Default;
                    ((AppShell) Window.Current.Content).RequestedTheme = ElementTheme.Default;
                    break;
            }

            var textColor = ((AppShell)Window.Current.Content).RequestedTheme == ElementTheme.Dark ? Colors.White : Colors.Black;

            if (DeviceHelper.IsDesktop)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                // Update Title bar colors
                ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Transparent;
                ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor =
                    new Color { R = 0, G = 0, B = 0, A = 20 };
                ApplicationView.GetForCurrentView().TitleBar.ButtonPressedBackgroundColor =
                    new Color { R = 0, G = 0, B = 0, A = 60 };
                ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                ApplicationView.GetForCurrentView().TitleBar.ForegroundColor = textColor;
                ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = textColor;
                ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = textColor;
                ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = textColor;
            }
        }
    }
}