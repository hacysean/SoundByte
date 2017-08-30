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
using SoundByte.API.Endpoints;
using SoundByte.API.Exceptions;
using SoundByte.Core.Services;
using SoundByte.UWP.UserControls;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace SoundByte.UWP.Models
{
    public class YouTubeSearchModel : ObservableCollection<Track>, ISupportIncrementalLoading
    {
        /// <summary>
        /// The position of the track, will be 'eol'
        /// if there are no new trackss
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// What we are searching the soundcloud API for
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Filter the search
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Are there more items to load
        /// </summary>
        public bool HasMoreItems => Token != "eol";

        /// <summary>
        /// Refresh the list by removing any
        /// existing items and reseting the token.
        /// </summary>
        public void RefreshItems()
        {
            Token = null;
            Clear();
        }

        /// <summary>
        /// Loads search track items from the souncloud api
        /// </summary>
        /// <param name="count">The amount of items to load</param>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            // Return a task that will get the items
            return Task.Run(async () =>
            {
                if (string.IsNullOrEmpty(Query))
                    return new LoadMoreItemsResult { Count = 0 };

                // We are loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    (App.CurrentFrame?.FindName("YouTubeSearchModelInfoPane") as InfoPane)?.ShowLoading();
                });

                // Get the resource loader
                var resources = ResourceLoader.GetForViewIndependentUse();

                // only 10 tracks at once
                count = 10;

                try
                {
                    // Search for matching tracks
                    var searchTracks = await SoundByteService.Instance.GetAsync<YTRootObject>(
                        ServiceType.YouTube, "search", new Dictionary<string, string>
                        {
                            {"part", "snippet"},
                            {"maxResults", count.ToString() },
                            { "q", Query },
                            { "pageToken", Token }
                        });

                    // Parse uri for offset
                    //   var param = new QueryParameterCollection(searchTracks.NextList);
                    var offset = searchTracks.nextPageToken;

                    // Get the search offset
                    Token = string.IsNullOrEmpty(offset) ? "eol" : offset;

                    var client = new YoutubeClient();

                    // Make sure that there are tracks in the list
                    if (searchTracks.items.Count > 0)
                    {
                        // Set the count variable
                        count = (uint)searchTracks.items.Count;

                        foreach (var item in searchTracks.items)
                        {
                            if (item.id.kind == "youtube#video")
                            {
                                if (item.snippet.liveBroadcastContent == "none")
                                {
                                    VideoInfo video = await client.GetVideoInfoAsync((string)item.id.videoId);

                                    // Loop though all the tracks on the UI thread
                                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                                    {
                                        Add(new Track
                                        {
                                            ServiceType = ServiceType.YouTube,
                                            Id = item.id.videoId,
                                            Kind = "track",
                                            Duration = video.Duration.TotalMilliseconds,
                                            CreationDate = DateTime.Parse(item.snippet.publishedAt),
                                            Description = video.Description,
                                            LikesCount = video.LikeCount,
                                            PlaybackCount = video.ViewCount,
                                            ArtworkLink = video.ImageHighResUrl,
                                            Title = item.snippet.title, 
                                            Genre = "YouTube",
                                            VideoStreamUrl = video.VideoStreams.OrderBy(s => s.VideoQuality).Last()?.Url,
                                            StreamUrl = video.AudioStreams.OrderBy(q => q.AudioEncoding).Last()?.Url,
                                            User = new User
                                            {
                                                Username = item.snippet.channelTitle,
                                                ArtworkLink = video.Author.LogoUrl
                                            },
                                            PermalinkUri = $"https://www.youtube.com/watch?v={item.id.videoId}"
                                        });
                                    });
                                }
                            }
                        }
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
                            (App.CurrentFrame?.FindName("YouTubeSearchModelInfoPane") as InfoPane)?.ShowMessage(
                                resources.GetString("SearchTrack_Header"),
                                resources.GetString("SearchTrack_Content"), "", false);
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
                        (App.CurrentFrame?.FindName("YouTubeSearchModelInfoPane") as InfoPane)?.ShowMessage(
                            ex.ErrorTitle, ex.ErrorDescription, ex.ErrorGlyph);
                    });
                }

                // We are not loading
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    (App.CurrentFrame?.FindName("YouTubeSearchModelInfoPane") as InfoPane)?.ClosePane();
                });

                // Return the result
                return new LoadMoreItemsResult { Count = count };
            }).AsAsyncOperation();
        }


        // --- TEMP BECAUSE WE CANNOT USE DYNAMIC IN RELEASE MODE ---- //

        public class YTPageInfo
        {
            public int totalResults { get; set; }
            public int resultsPerPage { get; set; }
        }

        public class YTId
        {
            public string kind { get; set; }
            public string channelId { get; set; }
            public string videoId { get; set; }
            public string playlistId { get; set; }
        }

        public class YTDefault
        {
            public string url { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class YTMedium
        {
            public string url { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class YTHigh
        {
            public string url { get; set; }
            public int? width { get; set; }
            public int? height { get; set; }
        }

        public class YTThumbnails
        {
            public YTDefault @default { get; set; }
            public YTMedium medium { get; set; }
            public YTHigh high { get; set; }
        }

        public class YTSnippet
        {
            public string publishedAt { get; set; }
            public string channelId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public YTThumbnails thumbnails { get; set; }
            public string channelTitle { get; set; }
            public string liveBroadcastContent { get; set; }
        }

        public class YTItem
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public YTId id { get; set; }
            public YTSnippet snippet { get; set; }
        }

        public class YTRootObject
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public string nextPageToken { get; set; }
            public string regionCode { get; set; }
            public YTPageInfo pageInfo { get; set; }
            public List<YTItem> items { get; set; }
        }

    }
}
