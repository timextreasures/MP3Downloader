using System;
using MusicDownloader.Common;
using MusicDownloader.Properties;

namespace MusicDownloader.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string directoryPath;

        /// <summary>
        /// Constructor
        /// </summary>
        public SettingsViewModel()
        {
            DirectoryPath = Settings.Default.DownloadsDirectory;
        }

        public string DirectoryPath
        {
            get { return directoryPath; }
            set
            {
                directoryPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Enables selection of a save path for downloads by browsing
        /// </summary>
        public void BrowseDirectory()
        {
            try
            {
                string path = DialogService.ShowFolderBrowserDialog();
                if (!String.IsNullOrEmpty(path))
                {
                    DirectoryPath = path;
                    Settings.Default.DownloadsDirectory = path;
                    Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowMessageBox(ex.Message);
            }
        }
    }
}