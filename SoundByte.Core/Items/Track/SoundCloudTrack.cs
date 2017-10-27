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
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SoundByte.Core.Helpers;
using SoundByte.Core.Items.Comment;
using SoundByte.Core.Items.User;
using SoundByte.Core.Services;
using System.Threading;
using System.Net;

namespace SoundByte.Core.Items.Track
{
    /// <summary>
    /// SoundCloud Specific Item
    /// </summary>
    [JsonObject]
    public class SoundCloudTrack : ITrack
    {
        public SoundCloudTrack()
        { 
        }

        public SoundCloudTrack(string id)
        {
            Id = int.Parse(id);
        }

        [JsonProperty("artwork_url")]
        // ReSharper disable once InconsistentNaming
        private string _artworkUrl { get; set; }

        public string ArtworkUrl
        {
            set => _artworkUrl = value;
            get
            {
                if (string.IsNullOrEmpty(_artworkUrl))
                {
                    return User.AvatarUrl;
                }
                else
                {
                    return _artworkUrl;
                }
            }
        }

        [JsonProperty("commentable")]
        public bool IsCommentable { get; set; }

        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("likes_count")]
        public int LikesCount { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonProperty("playback_count")]
        public int PlaybackCount { get; set; }

        [JsonProperty("@public")]
        public bool IsPublic { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("reposts_count")]
        public int RepostsCount { get; set; }

        [JsonProperty("secret_token")]
        public object SecretToken { get; set; }

        [JsonProperty("tag_list")]
        public string TagList { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("user")]
        public SoundCloudUser User { get; set; }

        public BaseTrack AsBaseTrack => ToBaseTrack();

        /// <summary>
        /// Convert this SoundCloud specific track to a universal track.
        /// </summary>
        /// <returns></returns>
        public BaseTrack ToBaseTrack()
        {
            return new BaseTrack
            {
                ServiceType = ServiceType.SoundCloud,
                Id = Id.ToString(),
                Kind = Kind,
                Link = PermalinkUrl,
                AudioStreamUrl = string.Empty,
                VideoStreamUrl = string.Empty,
                ArtworkUrl = ArtworkUrl,
                Title = Title,
                Description = Description,
                Duration = TimeSpan.FromMilliseconds(Duration), 
                Created = DateTime.Parse(CreatedAt), 
                LikeCount = LikesCount,
                DislikeCount = 0,
                ViewCount = PlaybackCount, 
                CommentCount = CommentCount,
                Genre = Genre,
                User = User.ToBaseUser()
            };
        }

        /// <summary>
        /// Search the SoundCloud API for tracks
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="count">Amount of items to return</param>
        /// <param name="token"></param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns></returns>
        public static async Task<(IEnumerable<BaseTrack> Tracks, string Token)> SearchAsync(string searchTerm, uint count, string token, CancellationTokenSource cancellationTokenSource = null)
        {
            if (count <= 10)
                count = 10;

            // Search for matching tracks
            var searchTracks = await SoundByteV3Service.Current.GetAsync<TrackListHolder>(ServiceType.SoundCloud, "/tracks",
                new Dictionary<string, string>
                {
                    {"limit", count.ToString()},
                    {"linked_partitioning", "1"},
                    {"offset", token},
                    {"q", WebUtility.UrlEncode(searchTerm)}
                });

            // Parse uri for offset
            var param = new QueryParameterCollection(searchTracks.NextList);
            var nextToken = param.FirstOrDefault(x => x.Key == "offset").Value;

            // Convert our list of SoundCloud comments to base comments
            var baseTrackList = new List<BaseTrack>();
            searchTracks.Tracks.ForEach(x => baseTrackList.Add(x.ToBaseTrack()));

            // Return the data
            return (baseTrackList, nextToken);
        }

        /// <summary>
        /// Gets a list of base comments for this track.
        /// </summary>
        /// <param name="count">The amount of comments to get.</param>
        /// <param name="token">Position in the comments (depends on service)</param>
        /// <param name="cancellationTokenSource"></param>
        /// <returns>A list of base comments and the next token</returns>
        public async Task<(IEnumerable<BaseComment> Comments, string Token)> GetCommentsAsync(uint count, string token, CancellationTokenSource cancellationTokenSource = null)
        {
            // Grab a list of SoundCloud comments
            var soundCloudComments = await SoundByteV3Service.Current.GetAsync<CommentListHolder>(ServiceType.SoundCloud,
                $"/tracks/{Id}/comments", new Dictionary<string, string>
                {
                    {"limit", count.ToString()},
                    {"offset", token},
                    {"linked_partitioning", "1"}
                }, cancellationTokenSource);

            // Parse the next URL and grab the token
            var nextUri = new QueryParameterCollection(soundCloudComments.NextList);
            var nextToken = nextUri.FirstOrDefault(x => x.Key == "offset").Value;

            // Convert our list of SoundCloud comments to base comments
            var baseCommentList = new List<BaseComment>();
            soundCloudComments.Items.ForEach(x => baseCommentList.Add(x.ToBaseComment()));

            // Return the data
            return (baseCommentList, nextToken);
        }

        [JsonObject]
        private class CommentListHolder
        {
            /// <summary>
            ///     List of comments
            /// </summary>
            [JsonProperty("collection")]
            public List<SoundCloudComment> Items { get; set; }

            /// <summary>
            ///     Next items in the list
            /// </summary>
            [JsonProperty("next_href")]
            public string NextList { get; set; }
        }

        private class TrackListHolder
        {
            /// <summary>
            ///     Collection of tracks
            /// </summary>
            [JsonProperty("collection")]
            public List<SoundCloudTrack> Tracks { get; set; }

            /// <summary>
            ///     The next list of items
            /// </summary>
            [JsonProperty("next_href")]
            public string NextList { get; set; }
        }
    }
}
