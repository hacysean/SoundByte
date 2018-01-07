﻿/* |----------------------------------------------------------------|
 * | Copyright (c) 2017 - 2018 Grid Entertainment                   |
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
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage.Streams;
using Windows.System;
using JetBrains.Annotations;
using SoundByte.Core;
using SoundByte.Core.Items.Generic;
using SoundByte.Core.Items.Track;
using SoundByte.Core.Services;
using SoundByte.Core.Sources;
using SoundByte.UWP.Converters;
using SoundByte.UWP.Extensions;
using YoutubeExplode;

namespace SoundByte.UWP.Services
{
    /// <summary>
    ///     UWP implementation of the Playback Service
    /// </summary>
    public partial class PlaybackV2Service : IPlaybackService
    {
        #region Delegates
        public delegate void TrackChangedEventHandler(BaseTrack newTrack);

        public delegate void StateChangedEventHandler(MediaPlaybackState mediaPlaybackState);
        #endregion

        #region Events
        /// <summary>
        ///     This event is fired when the current track changes.
        /// </summary>
        public event TrackChangedEventHandler OnTrackChange;

        /// <summary>
        ///     This event is fired when the current playback state changes
        ///     (e.g paused, playing, stopped, opening etc.)
        /// </summary>
        public event StateChangedEventHandler OnStateChange;
        #endregion

        #region Private Variables
        /// <summary>
        ///     Used for working with YouTube video streams. This is a shared
        ///     instance to increase performance.
        /// </summary>
        private YoutubeClient _youTubeClient;

        /// <summary>
        ///     Shared media player used throughout the app.
        /// </summary>
        private MediaPlayer _mediaPlayer;

        /// <summary>
        ///     The currently playing track
        /// </summary>
        [CanBeNull]
        private BaseTrack _currentTrack;

        /// <summary>
        ///     Media Playback List that allows queuing of songs and 
        ///     gapless playback.
        /// </summary>
        private MediaPlaybackList MediaPlaybackList => _mediaPlayer.Source as MediaPlaybackList;

        /// <summary>
        ///     The current playlist token (next items in the list)
        /// </summary>
        private string _playlistToken;

        /// <summary>
        ///     The source of items to load.
        /// </summary>
        private ISource<BaseTrack> _playlistSource;
        #endregion

        #region Constructor
        /// <summary>
        /// Setup the playback service class for use.
        /// </summary>
        public PlaybackV2Service()
        {
            // Only keep 5 items open and do not auto repeat
            // as we will be loading more items once we reach the
            // end of a list (or starting over if in playlist)
            var mediaPlaybackList = new MediaPlaybackList
            {
                MaxPlayedItemsToKeepOpen = 5,
                AutoRepeatEnabled = false
            };

            // Create the media player and disable auto play
            // as we are going to use a playback list. Set the
            // source to the media playback list.
            _mediaPlayer = new MediaPlayer
            {
                AutoPlay = false,
                Source = mediaPlaybackList
            };

            // Create the youtube client used to parse YouTube streams.
            _youTubeClient = new YoutubeClient();

            // Assign event handlers
            MediaPlaybackList.CurrentItemChanged += MediaPlaybackListOnCurrentItemChanged;


        }
        #endregion

        #region Private Event Handlers
        /// <summary>
        ///     Occurs when a current media playback item changes.
        /// </summary>
        private async void MediaPlaybackListOnCurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var track = args.NewItem?.Source.AsBaseTrack();

            // If there is no new item, don't do anything
            if (track == null)
                return;

            // Invoke the track change method
            OnTrackChange?.Invoke(track);

            await Task.Run(() =>
            {
                string currentUsageLimit;
                var memoryUsage = MemoryManager.AppMemoryUsage / 1024 / 1024;

                if (memoryUsage > 512)
                {
                    currentUsageLimit = "More than 512MB";
                }
                else if (memoryUsage > 256)
                {
                    currentUsageLimit = "More than 256MB";
                }
                else if (memoryUsage > 128)
                {
                    currentUsageLimit = "More than 128MB";
                }
                else
                {
                    currentUsageLimit = "Less than 128MB";
                }

                App.Telemetry.TrackEvent("Current Song Changed", new Dictionary<string, string>
                {
                    { "CurrentUsage", currentUsageLimit },
                    { "TrackType", track.ServiceType.ToString() ?? "Null" },
                    { "IsSoundCloudConnected", SoundByteService.Current.IsServiceConnected(ServiceType.SoundCloud).ToString() },
                    { "IsFanburstConnected", SoundByteService.Current.IsServiceConnected(ServiceType.Fanburst).ToString() },
                    { "IsYouTubeConnected", SoundByteService.Current.IsServiceConnected(ServiceType.YouTube).ToString() }
                });
            });
        }
        #endregion

        #region Track Controls

        #region Getters
        /// <summary>
        ///     Has the current playlist been shuffled.
        /// </summary>
        public bool IsPlaylistShuffled => MediaPlaybackList.ShuffleEnabled;

        /// <summary>
        ///     Is the current track muted
        /// </summary>
        public bool IsTrackMuted => _mediaPlayer.IsMuted;

        /// <summary>
        ///     Will the current track repeat when finished.
        /// </summary>
        public bool IsTrackRepeating => _mediaPlayer.IsLoopingEnabled;

        /// <summary>
        ///     Volume of the current playing track.
        /// </summary>
        public double TrackVolume => _mediaPlayer.Volume;

        public MediaPlaybackState CurrentPlaybackState => _mediaPlayer.PlaybackSession.PlaybackState;

        #endregion

        /// <summary>
        ///     Shuffle the playlist
        /// </summary>
        /// <param name="shuffle">True to shuffle, false to not.</param>
        public void ShufflePlaylist(bool shuffle)
        {
            // Shuffle the playlist
            MediaPlaybackList.ShuffleEnabled = true;

            // Start a random track
            StartRandomTrack();

            // Track event
            App.Telemetry.TrackEvent("Shuffle Playlist", new Dictionary<string, string>
            {
                { "IsShuffled", shuffle.ToString() }
            });
        }

        public void MuteTrack(bool mute)
        {
            _mediaPlayer.IsMuted = mute;
        }

        public void SetTrackVolume(double volume)
        {
            _mediaPlayer.Volume = volume;
        }

        public void SetTrackPosition(TimeSpan value) => _mediaPlayer.PlaybackSession.Position = value;

        public TimeSpan GetTrackPosition() => _mediaPlayer.PlaybackSession.Position;

        public TimeSpan GetTrackDuration() => _mediaPlayer.PlaybackSession.NaturalDuration;

        public void RepeatTrack(bool repeat)
        {
            _mediaPlayer.IsLoopingEnabled = repeat;
        }

        /// <summary>
        ///     Move to the next item.
        /// </summary>
        public void NextTrack()
        {
            _mediaPlayer.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
            MediaPlaybackList.MoveNext();
        }

        /// <summary>
        ///     Move to the previous item
        /// </summary>
        public void PreviousTrack()
        {
            _mediaPlayer.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
            MediaPlaybackList.MovePrevious();
        }

        /// <summary>
        ///     Pause the current track
        /// </summary>
        public void PauseTrack()
        {
            if (_mediaPlayer.CanPause)
                _mediaPlayer.Pause();

            // TODO: Remote features.
        }

        /// <summary>
        ///     Play the current track
        /// </summary>
        public void PlayTrack()
        {
            _mediaPlayer.Play();

            // TODO: Remote Features
        }
        #endregion



        public async Task StartTrackAsync(BaseTrack trackToPlay)
        {
            if (trackToPlay == null)
            {
                _mediaPlayer.Play();
                return;
            }

            var keepTrying = 0;

            while (keepTrying < 50)
            {
                try
                {
                    // find the index of the track in the playlist
                    var index = MediaPlaybackList.Items.ToList()
                        .FindIndex(item => item.Source.AsBaseTrack().Id ==
                                           trackToPlay.Id);

                    if (index == -1)
                    {
                        await Task.Delay(50);
                        keepTrying++;
                        continue;
                    }

                    // Move to the track
                    MediaPlaybackList.MoveTo((uint)index);

                    // Begin playing
                    _mediaPlayer.Play();

                    return;
                }
                catch (Exception)
                {
                    keepTrying++;
                    await Task.Delay(200);
                }
            }
            // Just play the first item
            _mediaPlayer.Play();
        }

        public void StartRandomTrack()
        {
            throw new NotImplementedException();
        }

        public BaseTrack GetCurrentTrack()
        {
            return MediaPlaybackList.CurrentItem.Source.AsBaseTrack();
        }

        public async Task<PlaybackInitilizeResponse> InitilizePlaylistAsync<T>(IEnumerable<BaseTrack> playlist = null, string token = null) where T : ISource<BaseTrack>
        {
            _playlistSource = Activator.CreateInstance<T>();
            _playlistToken = token;

            // We are changing media
            _mediaPlayer.SystemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;

            // Pause the media player and clear the currenst playlist
            _mediaPlayer.Pause();
            MediaPlaybackList.Items.Clear();

            // If the playlist does not exist, or was not passed in, we
            // need to load the first 50 items.
            if (playlist == null)
            {
                try
                {
                    // Get (up to) 50 items and update the token
                    var responseItems = await _playlistSource.GetItemsAsync(50, _playlistToken);
                    _playlistToken = responseItems.Token;
                    playlist = responseItems.Items;
                }
                catch (Exception e)
                {
                    return new PlaybackInitilizeResponse(false, "Error Loading Playlist: " + e.Message);
                }
            }

            // Loop through all the tracks and add them to the playlist
            foreach (var track in playlist)
            {
                try
                {
                    // Create a media binding for later (this is used to
                    // load the track streams as we need them).
                    var binder = new MediaBinder { Token = track.Id };
                    binder.Binding += BindMediaSource;

                    // Create the source, bind track metadata and use it to
                    // create a playback item
                    var source = MediaSource.CreateFromMediaBinder(binder);
                    var mediaPlaybackItem = new MediaPlaybackItem(track.AsMediaSource(source));

                    // Apply display properties to this item
                    var displayProperties = mediaPlaybackItem.GetDisplayProperties();
                    displayProperties.Type = MediaPlaybackType.Music;
                    displayProperties.MusicProperties.Title = track.Title;
                    displayProperties.MusicProperties.Artist = track.User.Username;
                    displayProperties.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(ArtworkConverter.ConvertObjectToImage(track)));

                    // Apply the properties
                    mediaPlaybackItem.ApplyDisplayProperties(displayProperties);

                    // Add this item to the list
                    MediaPlaybackList.Items.Add(mediaPlaybackItem);
                }
                catch (Exception e)
                {
                    App.Telemetry.TrackEvent("Playback Item Addition Failed", new Dictionary<string, string>
                    {
                        { "TrackID", track.Id },
                        { "TrackService", track.ServiceType.ToString() },
                        { "ErrorMessage", e.Message }
                    });
                }
            }

            // Everything loaded fine
            return new PlaybackInitilizeResponse();
        }

        #region Media Binding
        private async void BindMediaSource(MediaBinder sender, MediaBindingEventArgs args)
        {
            var deferal = args.GetDeferral();

            // Get the track data
            var track = MediaPlaybackList.Items.ToList()
                .FirstOrDefault(x => x.Source.AsBaseTrack().Id == args.MediaBinder.Token)
                ?.Source?.AsBaseTrack();

            // Only run if the track exists
            if (track != null)
            {
                // Get the audio stream url for this track
                var audioStreamUri = await track.GetAudioStreamAsync(_youTubeClient);

                // If we are live and youtube, we get an adaptive stream url
                if (track.ServiceType == ServiceType.YouTube && track.IsLive)
                {
                    var source = await AdaptiveMediaSource.CreateFromUriAsync(audioStreamUri);
                    if (source.Status == AdaptiveMediaSourceCreationStatus.Success)
                    {
                        args.SetAdaptiveMediaSource(source.MediaSource);
                    }
                }
                else
                {
                    // Set generic stream url.
                    args.SetUri(audioStreamUri);
                }
            }

            deferal.Complete();

        }
        #endregion
    }

    /// <summary>
    ///     UWP implementation of the Playback Service
    /// </summary>
    public partial class PlaybackV2Service
    {
        private static PlaybackV2Service _instance;

        /// <summary>
        ///     Singleton instance of <see cref="PlaybackV2Service"/>.
        /// </summary>
        public static PlaybackV2Service Instance => _instance ?? (_instance = new PlaybackV2Service());
    }
}