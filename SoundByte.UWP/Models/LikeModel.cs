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
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using Microsoft.Toolkit.Uwp.Helpers;
using SoundByte.API.Exceptions;
using SoundByte.API.Holders;
using SoundByte.API.Items.Track;
using SoundByte.API.Items.User;
using SoundByte.UWP.Services;
using SoundByte.UWP.UserControls;

namespace SoundByte.UWP.Models
{
    /// <summary>
    ///     Model for user likes
    /// </summary>
    public class LikeModel : ObservableCollection<BaseTrack>, ISupportIncrementalLoading
    {
        /// <summary>
        ///     Setsup the like view model for a user
        /// </summary>
        /// <param name="user">The user to retrieve likes for</param>
        public LikeModel(BaseUser user)
        {
            User = user;
        }
        // User object that we will used to get the likes for

        public BaseUser User { get; set; }

        /// <summary>
        ///     The position of the track, will be 'eol'
        ///     if there are no new tracks
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        ///     Are there more items to load
        /// </summary>
        public bool HasMoreItems => Token != "eol";

        /// <summary>
        ///     Loads stream items from the souncloud api
        /// </summary>
        /// <param name="count">The amount of items to load</param>
        // ReSharper disable once RedundantAssignment
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            // Return a task that will get the items
            return Task.Run(async () =>
            {
                // We are loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    (App.CurrentFrame?.FindName("LikeModelInfoPane") as InfoPane)?.ShowLoading();
                });

                // Get the resource loader
                var resources = ResourceLoader.GetForViewIndependentUse();

                // Check if the user is not logged in
                if (User != null)
                {
                    try
                    {
                        // At least 10 tracks at once
                        if (count < 10)
                            count = 10;

                        // Get the like tracks
                        var likeTracks = await SoundByteService.Instance.GetAsync<TrackListHolder>(
                            $"/users/{User.Id}/favorites", new Dictionary<string, string>
                            {
                                {"limit", count.ToString()},
                                {"cursor", Token},
                                {"linked_partitioning", "1"}
                            });

                        // Parse uri for offset
                        var param = new QueryParameterCollection(likeTracks.NextList);
                        var cursor = param.FirstOrDefault(x => x.Key == "cursor").Value;

                        // Get the likes cursor
                        Token = string.IsNullOrEmpty(cursor) ? "eol" : cursor;

                        // Make sure that there are tracks in the list
                        if (likeTracks.Tracks.Count > 0)
                        {
                            // Set the count variable
                            count = (uint) likeTracks.Tracks.Count;

                            // Loop though all the tracks on the UI thread
                            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                            {
                                foreach (var track in likeTracks.Tracks)
                                {
                                    Add(track.ToBaseTrack());
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
                                (App.CurrentFrame?.FindName("LikeModelInfoPane") as InfoPane)?.ShowMessage(
                                    resources.GetString("LikeTracks_Header"),
                                    resources.GetString("LikeTracks_Content"), "", false);
                            });
                        }
                    }
                    catch (SoundByteException ex)
                    {
                        // Exception, most likely did not add any new items
                        count = 0;

                        // Exception, display error to the user
                        await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                        {
                            (App.CurrentFrame?.FindName("LikeModelInfoPane") as InfoPane)?.ShowMessage(
                                ex.ErrorTitle, ex.ErrorDescription, ex.ErrorGlyph);
                        });
                    }
                }
                else
                {
                    // Not logged in, so no new items
                    count = 0;

                    // Reset the token
                    Token = "eol";

                    // No items tell the user
                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        (App.CurrentFrame?.FindName("LikeModelInfoPane") as InfoPane)?.ShowMessage(
                            resources.GetString("ErrorControl_LoginFalse_Header"),
                            resources.GetString("ErrorControl_LoginFalse_Content"), "", false);
                    });
                }

                // We are not loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    (App.CurrentFrame?.FindName("LikeModelInfoPane") as InfoPane)?.ClosePane();
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