using System;
using System.Windows;
using Microsoft.Phone.BackgroundAudio;
using Library;
using System.IO.IsolatedStorage;
using System.IO;

namespace PlayBackAgent
{
    public class AudioPlayer : AudioPlayerAgent
    {
        static int currentTrackNumber = 0;
        static Playlist playlist;

        public AudioPlayer()
            : base()
        {

            //Load from IsoStore & deserialize 
            using (IsolatedStorageFile isoStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = isoStorage.OpenFile("playlist.xml", FileMode.Open))
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Playlist));
                    var reader = new StreamReader(file);

                    playlist = (Playlist)serializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Called when the playstate changes, except for the Error state (see OnError)
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time the playstate changed</param>
        /// <param name="playState">The new playstate of the player</param>
        /// <remarks>
        /// Play State changes cannot be cancelled. They are raised even if the application
        /// caused the state change itself, assuming the application has opted-in to the callback
        /// </remarks>
        protected override void OnPlayStateChanged(BackgroundAudioPlayer player, AudioTrack track, PlayState playState)
        {
            switch (playState)
            {
                case PlayState.TrackEnded:
                    PlayNext(player);
                    break;
                case PlayState.TrackReady:
                    player.Play();
                    break;
                default:
                    break;
            }

            NotifyComplete();
        }

        private void PlayNext(BackgroundAudioPlayer player)
        {
            var songsCount = playlist.Tracks.Count;

            if (++currentTrackNumber >= songsCount)
            {
                currentTrackNumber = 0;
            }
            Play(player);


        }
        private void PlayPrev(BackgroundAudioPlayer player)
        {
            var songsCount = playlist.Tracks.Count;
            if (--currentTrackNumber < 0)
            {
                currentTrackNumber = songsCount - 1;
            }
            Play(player);
        }


        /// <summary>
        /// Called when the user requests an action using system-provided UI and the application has requesed
        /// notifications of the action
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track playing at the time of the user action</param>
        /// <param name="action">The action the user has requested</param>
        /// <param name="param">The data associated with the requested action.
        /// In the current version this parameter is only for use with the Seek action,
        /// to indicate the requested position of an audio track</param>
        /// <remarks>
        /// User actions do not automatically make any changes in system state; the agent is responsible
        /// for carrying out the user actions if they are supported
        /// </remarks>
        protected override void OnUserAction(BackgroundAudioPlayer player, AudioTrack track, UserAction action, object param)
        {
            switch (action)
            {
                case UserAction.FastForward:
                    player.FastForward();
                    break;
                case UserAction.Pause:
                    player.Pause();
                    break;
                case UserAction.Play:
                    if (player.PlayerState == PlayState.Paused)
                    {
                        player.Play();
                    }
                    else
                    {
                        Play(player);
                    }
                    break;
                case UserAction.Rewind:
                    player.Rewind();
                    break;
                case UserAction.Seek:
                    player.Position = (TimeSpan)param;
                    break;
                case UserAction.SkipNext:
                    PlayNext(player);
                    break;
                case UserAction.SkipPrevious:
                    PlayPrev(player);
                    break;
                case UserAction.Stop:
                    player.Stop();
                    break;
                default:
                    break;
            }

            NotifyComplete();
        }

        /// <summary>
        /// Called whenever there is an error with playback, such as an AudioTrack not downloading correctly
        /// </summary>
        /// <param name="player">The BackgroundAudioPlayer</param>
        /// <param name="track">The track that had the error</param>
        /// <param name="error">The error that occured</param>
        /// <param name="isFatal">If true, playback cannot continue and playback of the track will stop</param>
        /// <remarks>
        /// This method is not guaranteed to be called in all cases. For example, if the background agent 
        /// itself has an unhandled exception, it won't get called back to handle its own errors.
        /// </remarks>
        protected override void OnError(BackgroundAudioPlayer player, AudioTrack track, Exception error, bool isFatal)
        {
            NotifyComplete();
        }

        private void Play(BackgroundAudioPlayer player)
        {
            var currentTrack = playlist.Tracks[currentTrackNumber];
            Uri tileUri = (currentTrack.Tile == null ? new Uri("Shared/Media/no-art.jpg", UriKind.Relative) :
                                                       (currentTrack.Tile.IsAbsoluteUri ? new Uri("Shared/Media/no-art.jpg", UriKind.Relative) :
                                                                                          new Uri(currentTrack.TileString.Replace("/Images", "Shared/Media"), UriKind.Relative)));

            var audioTrack = new AudioTrack(currentTrack.Source,
                                          currentTrack.Title,
                                          currentTrack.Artist,
                                          currentTrack.Album,
                                          tileUri,
                                          currentTrackNumber.ToString(),
                                          EnabledPlayerControls.All);
            player.Track = audioTrack;
        }


        /// <summary>
        /// Called when the agent request is getting cancelled
        /// </summary>
        protected override void OnCancel()
        {
            NotifyComplete();
        }
    }
}
