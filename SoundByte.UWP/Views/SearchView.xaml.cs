﻿using Windows.UI.Xaml.Navigation;
using SoundByte.UWP.ViewModels.Search;

namespace SoundByte.UWP.Views
{
    /// <summary>
    ///     This page lets the user search for tracks/playlists/people
    ///     within SoundCloud.
    /// </summary>
    public sealed partial class SearchView
    {
        // The view model for the page
        public SearchViewModel ViewModel { get; } = new SearchViewModel();

        /// <summary>
        ///     Setup the page
        /// </summary>
        public SearchView()
        {
            // Initialize XAML Components
            InitializeComponent();

            Unloaded += (s, e) =>
            {
                ViewModel?.Dispose();
            };
        }

        /// <summary>
        ///     Called when the user navigates to the page
        /// </summary>
        /// <param name="e">Args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.SearchQuery = e.Parameter != null ? e.Parameter as string : string.Empty;
            DataContext = ViewModel;
            PageTitle.Text = $"Results for \"{ViewModel.SearchQuery}\"";

            // Track Event
            App.Telemetry.TrackPage("Search View");
        }
    }
}