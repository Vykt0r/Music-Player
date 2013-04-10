using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Shell;
using Library;
using System.IO.IsolatedStorage;
using System.IO;
using System.Resources;
using System.Xml.Serialization;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;

namespace AudioPlayerApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public Playlist ActivePlaylist
        {
            get { return (Playlist)GetValue(ActivePlaylistProperty); }
            set { SetValue(ActivePlaylistProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActivePlaylist.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActivePlaylistProperty =
            DependencyProperty.Register("ActivePlaylist", typeof(Playlist), typeof(MainPage), new PropertyMetadata(null));

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            BackgroundAudioPlayer.Instance.PlayStateChanged += Instance_PlayStateChanged; 

        }
       
        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            //slider
            if (BackgroundAudioPlayer.Instance.Track != null)
            {
                SongSlider.Value = 0;
                SongSlider.Minimum = 0;
                SongSlider.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalMilliseconds;
                string text = BackgroundAudioPlayer.Instance.Track.Duration.ToString();
                EndTextBlock.Text = text.Substring(0, 8);
            }
            updateAppBarStatus();
            updateSelection();
            
        }

        private void StartTimer()
        {
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dispatcherTimer.Tick+=new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
            {
                SongSlider.Minimum = 0;
                SongSlider.Value = BackgroundAudioPlayer.Instance.Position.TotalMilliseconds;
                SongSlider.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalMilliseconds;

                string text = BackgroundAudioPlayer.Instance.Position.ToString();
                StartTextBlock.Text = text.Substring(0, 8);
                text = BackgroundAudioPlayer.Instance.Track.Duration.ToString();
                EndTextBlock.Text = text.Substring(0, 8);

            }
        }

        private void SongSlider_Manipuation(object sender, ManipulationCompletedEventArgs e)
        {
            int sliderValue = (int)SongSlider.Value;
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, sliderValue);
            BackgroundAudioPlayer.Instance.Position = timeSpan;
        }
        
        

        private void updateSelection()
        {
            int activeTrackNumber = GetActiveTrackIndex();

            if (activeTrackNumber != -1)
            {
                lstTracks.SelectedIndex = activeTrackNumber;
            }
        }

        private int GetActiveTrackIndex()
        {
            int track = -1;
            if (null != BackgroundAudioPlayer.Instance.Track)
            {
                track = int.Parse(BackgroundAudioPlayer.Instance.Track.Tag);
                
            }
            return track;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Stream playlistStream = Application.GetResourceStream(new Uri("Xml/Playlist.xml", UriKind.Relative)).Stream;

            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Playlist));
            ActivePlaylist = (Playlist)serializer.Deserialize(playlistStream);

            using (IsolatedStorageFile isoStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = isoStorage.OpenFile("playlist.xml", FileMode.OpenOrCreate))
                {
                    var writer = new StreamWriter(file);

                    serializer.Serialize(writer, ActivePlaylist);
                }
            }

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                StartTimer();
            }
            StartTimer();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            dispatcherTimer.Stop();
            dispatcherTimer.Tick -= new EventHandler(dispatcherTimer_Tick);
            BackgroundAudioPlayer.Instance.PlayStateChanged -= new EventHandler(Instance_PlayStateChanged);
        }

        private void appbar_previous(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipPrevious();
            SongSlider.Value = 0;
        }

        private void appbar_playpause(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == Microsoft.Phone.BackgroundAudio.PlayState.Playing)
            {
                BackgroundAudioPlayer.Instance.Pause();
                
            }
            else
            {
                BackgroundAudioPlayer.Instance.Play();
            }
        }

        private void appbar_stop(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.Stop();
        }

        private void appbar_next(object sender, EventArgs e)
        {
            BackgroundAudioPlayer.Instance.SkipNext();
            SongSlider.Value = 0;
        }

        private void updateAppBarStatus()
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    //Prev Button
                    if (GetActiveTrackIndex() > 0)
                        (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

                    //Play/Pause Button
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = "pause";
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IconUri = new Uri("/Images/pause.png", UriKind.Relative);

                    //Stop Button
                    (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;

                    //Next button
                    if (GetActiveTrackIndex() < ActivePlaylist.Tracks.Count - 1)
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = false;
                    break;

                case PlayState.Paused:
                    //Previous Button
                    if (GetActiveTrackIndex() > 0)
                        (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

                    //Play/Pause Button
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = "play";
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IconUri = new Uri("/Images/play.png", UriKind.Relative);

                    //Stop Button
                    (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;

                    //Next button
                    if (GetActiveTrackIndex() < ActivePlaylist.Tracks.Count - 1)
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = false;
                    break;

                case PlayState.Stopped:
                    //Previous Button
                    if (GetActiveTrackIndex() > 0)
                        (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

                    //Play/Pause Button
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = "play";
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IconUri = new Uri("/Images/play.png", UriKind.Relative);

                    //Stop Button
                    (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = false;

                    //Next button
                    if (GetActiveTrackIndex() < ActivePlaylist.Tracks.Count - 1)
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = false;
                    break;

                case PlayState.Unknown:
                    //Previous button
                    (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;

                    //Play/Pause button
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).Text = "play";
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IconUri = new Uri("/Images/play.png", UriKind.Relative);

                    //Stop button
                    (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = false;

                    //Next button
                    if (GetActiveTrackIndex() < ActivePlaylist.Tracks.Count - 1)
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = true;
                    else
                        (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = false;
                    break;
                
                default:
                    break;
            }
        }

        private void lstTracks_Loaded(object sender, RoutedEventArgs e)
        {
            updateSelection();
        }

        
    }
}