using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MusicDownloader.Common;
using MusicDownloader.Data;
using MusicDownloader.Models;
using YoutubeExtractor;

namespace MusicDownloader.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly YouTube youtube = new YouTube();
        private bool isPlaying;
        private Uri mediaSource;
        private MediaState mediaState;
        private string searchText;

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchViewModel()
        {
            Videos = new ObservableCollection<YoutubeVideo>();

            SearchCommand = new RelayCommand(Search, () => !String.IsNullOrEmpty(SearchText));
            ListenCommand = new RelayCommand(ListenToSong, () => SelectedVideo != null);
            DownloadCommand = new RelayCommand(Download, () => SelectedVideo != null);
        }

        public ObservableCollection<YoutubeVideo> Videos { get; set; }
        public YoutubeVideo SelectedVideo { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand ListenCommand { get; set; }
        public ICommand DownloadCommand { get; set; }

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                OnPropertyChanged();
            }
        }

        public MediaState MediaState
        {
            get { return mediaState; }
            set
            {
                mediaState = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                OnPropertyChanged();
            }
        }

        public Uri MediaSource
        {
            get { return mediaSource; }
            set
            {
                mediaSource = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Sends a new message that a new download is to be added
        /// </summary>
        private void Download()
        {
            // Broadcast Events
            EventSystem.Publish(new DownloadMessage {Url = SelectedVideo.VideoUrl});
        }

        /// <summary>
        /// Plays sound from selected YouTube video
        /// </summary>
        private async void ListenToSong()
        {
            if (MediaState == MediaState.Play) MediaState = MediaState.Stop;

            else
            {
                VideoInfo video = await GetVideoInfoForStreaming(SelectedVideo.VideoUrl);

                if (video != null && video.RequiresDecryption)
                {
                    await Task.Run(() => DownloadUrlResolver.DecryptDownloadUrl(video));
                }

                if (video != null)
                {
                    MediaSource = new Uri(video.DownloadUrl);
                    MediaState = MediaState.Play;
                }
            }
        }

        /// <summary>
        /// Retrieves streamable/downloadable url for a YouTube video
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <returns></returns>
        private async Task<VideoInfo> GetVideoInfoForStreaming(string videoUrl)
        {
            var videoInfos = await Task.Run(() => DownloadUrlResolver.GetDownloadUrls(videoUrl, false));
            var videos = videoInfos.Where(info => info.VideoType == VideoType.Mp4 && !info.Is3D && info.AdaptiveType == AdaptiveType.None);
            return videos.OrderByDescending(x => x.Resolution).FirstOrDefault();
        }

        /// <summary>
        /// Stops playback of sound
        /// </summary>
        public void StopListening()
        {
            MediaState = MediaState.Stop;
            IsPlaying = false;
        }

        /// <summary>
        /// Searches for a YouTube 
        /// </summary>
        private async void Search()
        {
            var result = await youtube.SearchYoutubeVideo(SearchText);

            Videos.Clear();
            foreach (var video in result)
            {
                Videos.Add(video);
            }
        }
    }
}