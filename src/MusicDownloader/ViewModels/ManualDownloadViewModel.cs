using System;
using System.Windows.Input;
using MusicDownloader.Common;

namespace MusicDownloader.ViewModels
{
    public class ManualDownloadViewModel : ViewModelBase
    {
        private string downloadLink;

        public ManualDownloadViewModel()
        {
            DownloadCommand = new RelayCommand(Download, CanDownload);
        }

        private bool CanDownload()
        {
            return !String.IsNullOrEmpty(DownloadLink) && DownloadLink.StartsWith("https://www.youtube.com/");
        }

        public string DownloadLink
        {
            get { return downloadLink; }
            set
            {
                downloadLink = value;
                OnPropertyChanged();
            }
        }

        public ICommand DownloadCommand { get; set; }

        /// <summary>
        /// Sends message that a new to download is to be added
        /// </summary>
        private void Download()
        {
            // Broadcast Events
            EventSystem.Publish(new DownloadMessage { Url = DownloadLink });
        }
    }
}