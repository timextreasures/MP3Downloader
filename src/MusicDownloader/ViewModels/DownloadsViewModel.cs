using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MusicDownloader.Common;
using MusicDownloader.Properties;
using MusicDownloader.Views;

namespace MusicDownloader.ViewModels
{
    public class DownloadsViewModel : ViewModelBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadsViewModel()
        {
            DownloadingItems = new ObservableCollection<DownloadItemView>();
            LoadSettings();
        }

        #region Properties

        public ObservableCollection<DownloadItemView> DownloadingItems { get; set; }

        public string Name
        {
            get { return "Downloads"; }
        }

        #endregion

        /// <summary>
        /// Loads basic settings for the applications work path and save path
        /// </summary>
        private void LoadSettings()
        {
            if (String.IsNullOrEmpty(Settings.Default.DownloadsDirectory))
            {
                string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string pathDownload = Path.Combine(pathUser, "Downloads");
                Settings.Default.DownloadsDirectory = pathDownload;
            }

            if (String.IsNullOrEmpty(Settings.Default.WorkDirectory))
            {
                Settings.Default.WorkDirectory = Environment.CurrentDirectory;
            }

            Settings.Default.Save();
        }

        /// <summary>
        /// Adds a new download task
        /// </summary>
        /// <param name="msg"></param>
        protected override void NewDownload(DownloadMessage msg)
        {
            try
            {
                DownloadingItems.Add(new DownloadItemView { DataContext = new DownloadItemViewModel(msg.Url) });
            }

            catch (Exception ex)
            {
                DialogService.ShowMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// Opens the folder where downloads are saved
        /// </summary>
        public void OpenDownloads()
        {
            try
            {
                DialogService.OpenFolder(Settings.Default.DownloadsDirectory);
            }
            catch (Exception ex)
            {
                DialogService.ShowMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// Clears a selected task
        /// </summary>
        /// <param name="msg"></param>
        protected override void ClearDownload(ClearMessage msg)
        {
            foreach (var task in DownloadingItems.Where(task => task.DataContext == msg.DownloadItemViewModel).ToList())
            {
                var viewModel = (DownloadItemViewModel)task.DataContext;
                viewModel.Dispose();
                DownloadingItems.Remove(task);
            }
        }

        /// <summary>
        /// Clears all tasks that are not in progress
        /// </summary>
        public void Clear()
        {
            // Get all tasks which have either status finished, canceled or error
            var tasksToClear = from task in DownloadingItems
                               let dataContext = (DownloadItemViewModel)task.DataContext
                               where !dataContext.IsDownloading
                               select task;

            foreach (var task in tasksToClear.ToList())
            {
                var viewModel = (DownloadItemViewModel)task.DataContext;
                viewModel.Dispose();
                DownloadingItems.Remove(task);
            }
        }
    }
}