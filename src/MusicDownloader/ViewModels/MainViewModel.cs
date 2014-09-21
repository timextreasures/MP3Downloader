using MusicDownloader.Common;

namespace MusicDownloader.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly DownloadsViewModel downloadsViewModel = new DownloadsViewModel();
        private readonly ManualDownloadViewModel manualDownloadViewModel = new ManualDownloadViewModel();
        private readonly SearchViewModel searchViewModel = new SearchViewModel();
        private readonly SettingsViewModel settingsViewModel = new SettingsViewModel();
        private ViewModelBase currentPageViewModel;
        private bool isDownloadsChecked;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            // Set starting page
            CurrentPageViewModel = searchViewModel;
        }

        #region Properties / Commands

        public ViewModelBase CurrentPageViewModel
        {
            get { return currentPageViewModel; }
            set
            {
                if (currentPageViewModel != value)
                {
                    currentPageViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDownloadsChecked
        {
            get { return isDownloadsChecked; }
            set
            {
                isDownloadsChecked = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public void OpenManual()
        {
            CurrentPageViewModel = manualDownloadViewModel;
        }

        public void OpenDownloads()
        {
            CurrentPageViewModel = downloadsViewModel;
        }

        public void OpenSettings()
        {
            CurrentPageViewModel = settingsViewModel;
        }

        public void OpenSearch()
        {
            CurrentPageViewModel = searchViewModel;
        }

        protected override void NewDownload(DownloadMessage msg)
        {
            OpenDownloads();
            IsDownloadsChecked = true;
        }

        #endregion
    }
}