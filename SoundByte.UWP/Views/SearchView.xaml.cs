﻿/* |----------------------------------------------------------------|
 * | Copyright (c) 2017, Grid Entertainment                         |
 * | All Rights Reserved                                            |
 * |                                                                |
 * | This source code is to only be used for educational            |
 * | purposes. Distribution of SoundByte source code in             |
 * | any form outside this repository is forbidden. If you          |
 * | would like to contribute to the SoundByte source code, you     |
 * | are welcome.                                                   |
 * |----------------------------------------------------------------|
 */

using Windows.UI.Xaml.Navigation;
using SoundByte.UWP.Services;
using SoundByte.UWP.ViewModels;

namespace SoundByte.UWP.Views
{
    /// <summary>
    ///     This page lets the user search for tracks/playlists/people
    ///     within SoundCloud.
    /// </summary>
    public sealed partial class SearchView
    {
        // The view model for the page
        public SearchViewModel ViewModel = new SearchViewModel();

        /// <summary>
        ///     Setup the page
        /// </summary>
        public SearchView()
        {
            // Initialize XAML Components
            InitializeComponent();
            // Set the data context
            DataContext = ViewModel;

            Unloaded += (s, e) =>
            {
                ViewModel.Dispose();
            };
        }

        /// <summary>
        ///     Called when the user navigates to the page
        /// </summary>
        /// <param name="e">Args</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                return;

            // Set the search string
            ViewModel.SearchQuery = e.Parameter != null ? e.Parameter as string : string.Empty;
            // Track Event
            TelemetryService.Instance.TrackPage("Search View");
        }
    }
}