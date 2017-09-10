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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Uwp.Helpers;
using SoundByte.Core;
using SoundByte.Core.Exceptions;
using SoundByte.Core.Holders;
using SoundByte.Core.Items.Playlist;
using SoundByte.Core.Services;
using SoundByte.UWP.Services;
using SoundByte.UWP.UserControls;

namespace SoundByte.UWP.Models
{
    public class SearchPlaylistModel : ObservableCollection<BasePlaylist>, ISupportIncrementalLoading
    {
        /// <summary>
        ///     The position of the track, will be 'eol'
        ///     if there are no new trackss
        /// </summary>
        public string Token { get; protected set; }

        /// <summary>
        ///     What we are searching the soundcloud API for
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        ///     Are there more items to load
        /// </summary>
        public bool HasMoreItems => Token != "eol";

        /// <summary>
        ///     Loads search track items from the souncloud api
        /// </summary>
        /// <param name="count">The amount of items to load</param>
        // ReSharper disable once RedundantAssignment
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            // Return a task that will get the items
            return Task.Run(async () =>
            {
                // If the query is empty, tell the user that they can search something
                if (string.IsNullOrEmpty(Query))
                    return new LoadMoreItemsResult {Count = 0};

                // We are loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    (App.CurrentFrame?.FindName("SearchPlaylistModelInfoPane") as InfoPane)?.ShowLoading();
                });

                // Get the resource loader
                var resources = ResourceLoader.GetForViewIndependentUse();

                try
                {
                    // Get the searched playlists
                    var searchPlaylists = await SoundByteV3Service.Current.GetAsync<SearchPlaylistHolder>(ServiceType.SoundCloud, "/playlists",
                        new Dictionary<string, string>
                        {
                            {"limit", SettingsService.TrackLimitor.ToString()},
                            {"linked_partitioning", "1"},
                            {"offset", Token},
                            {"q", WebUtility.UrlEncode(Query)}
                        });

                    // Parse uri for offset
                    var param = new QueryParameterCollection(searchPlaylists.NextList);
                    var offset = param.FirstOrDefault(x => x.Key == "offset").Value;

                    // Get the search playlists offset
                    Token = string.IsNullOrEmpty(offset) ? "eol" : offset;

                    // Make sure that there are playlists in the list
                    if (searchPlaylists.Playlists.Count > 0)
                    {
                        // Set the count variable
                        count = (uint) searchPlaylists.Playlists.Count;

                        // Loop though all the search playlists on the UI thread
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            foreach (var playlist in searchPlaylists.Playlists)
                            {
                                Add(playlist.ToBasePlaylist());
                            }
                        });
                    }
                    else
                    {
                        // There are no items, so we added no items
                        count = 0;

                        // Reset the token
                        Token = "eol";

                        // No items tell the user
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            (App.CurrentFrame?.FindName("SearchPlaylistModelInfoPane") as InfoPane)?.ShowMessage(
                                resources.GetString("SearchPlaylist_Header"),
                                resources.GetString("SearchPlaylist_Content"), "", false);
                        });
                    }
                }
                catch (SoundByteException ex)
                {
                    // Exception, most likely did not add any new items
                    count = 0;

                    // Reset the token
                    Token = "eol";

                    // Exception, display error to the user
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        (App.CurrentFrame?.FindName("SearchPlaylistModelInfoPane") as InfoPane)?.ShowMessage(
                            ex.ErrorTitle, ex.ErrorDescription, ex.ErrorGlyph);
                    });
                }


                // We are not loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    (App.CurrentFrame?.FindName("SearchPlaylistModelInfoPane") as InfoPane)?.ClosePane();
                });

                // Return the result
                return new LoadMoreItemsResult {Count = count};
            }).AsAsyncOperation();
        }

        /// <summary>
        ///     Refresh the list by removing any
        ///     existing items and reseting the token.
        /// </summary>
        public void RefreshItems()
        {
            Token = null;
            Clear();
        }
    }
}