using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MusicDownloader.Common;
using MusicDownloader.Data;
using MusicDownloader.Properties;
using NAudio.MediaFoundation;
using NAudio.Wave;
using YoutubeExtractor;

namespace MusicDownloader.ViewModels
{
    public class DownloadItemViewModel : ViewModelBase, IDisposable
    {
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private bool cancelDownload;
        private int downloadProgress;
        private WaveFormat inputWaveFormat;
        private bool isDownloading;
        private bool isIndeterminate;
        private string name;
        private string status;
        private VideoDownloader videoDownloader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="url"></param>
        public DownloadItemViewModel(string url)
        {
            Name = url;
            StartDownload(url);
        }

        #region Properties

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public int DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        public bool IsDownloading
        {
            get { return isDownloading; }
            set
            {
                isDownloading = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Updates download progress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void videoDownloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            if (!cancelDownload)
            {
                DownloadProgress = (int)e.ProgressPercentage;
            }

            else
            {
                e.Cancel = true;
                DownloadProgress = 0;
            }
        }

        /// <summary>
        /// Start download and conversion of YouTube video
        /// </summary>
        /// <param name="url"></param>
        private async void StartDownload(string url)
        {
            await Task.Factory.StartNew(() =>
            {
                try
                {       
                    DownloadVideo(url);
                }

                catch (OperationCanceledException)
                {
                    UpdateWorkStatus(WorkStatus.Canceled);
                }

                catch (Exception ex)
                {
                    UpdateWorkStatus(WorkStatus.Error);
                    DialogService.ShowMessageBox(ex.Message);
                }
            }, tokenSource.Token);
        }     

        /// <summary>
        /// Cancellation of the current task
        /// </summary>
        public void Cancel()
        {
            try
            {
                UpdateWorkStatus(WorkStatus.Canceled);
                cancelDownload = true;
                tokenSource.Cancel();
            }

            catch (Exception ex)
            {
                DialogService.ShowMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// Send message to remove this task from the downloads list in DownloadsViewModel
        /// </summary>
        public void Clear()
        {
            // Broadcast Events
            EventSystem.Publish(new ClearMessage { DownloadItemViewModel = this });
        }

        /// <summary>
        /// Downloads YouTube video to a given folder. Removes illegal chars from the title of the video,
        /// then calls method to convert the video file (.mp4) to MP3-format using NAudio.
        /// </summary>
        /// <param name="url"></param>
        private void DownloadVideo(string url)
        {
            try
            {
                UpdateWorkStatus(WorkStatus.Locating);
                VideoInfo video = GetVideo(url);

                if (tokenSource.IsCancellationRequested)
                    tokenSource.Token.ThrowIfCancellationRequested();

                if (video != null)
                {
                    UpdateWorkStatus(WorkStatus.Downloading);
                    Name = video.Title;
                    string fileName = video.Title + video.VideoExtension;

                    /* The title of the YouTube video is used as file name for the downloaded file. YouTube videos may have
                     * titles that contain illegal chars which must be removed from the file name */
                    string input = Path.Combine(Settings.Default.WorkDirectory, CleanNameFromIllegalChars(fileName.Trim()));

                    // Download the video
                    videoDownloader = new VideoDownloader(video, input);
                    videoDownloader.DownloadProgressChanged += videoDownloader_DownloadProgressChanged;
                    videoDownloader.Execute();

                    // Cleanup before throwing cancellation 
                    if (tokenSource.IsCancellationRequested)
                    {
                        File.Delete(input);
                        tokenSource.Token.ThrowIfCancellationRequested();
                    }

                    UpdateWorkStatus(WorkStatus.Creating);
                    string output = Path.Combine(Settings.Default.DownloadsDirectory, CleanNameFromIllegalChars(video.Title.Trim()) + ".mp3");
                    int bytesPerSecond = ConvertBitrate(video.AudioBitrate);

                    // Convert downloaded video to mp3
                    if (TryOpenInputFile(input))
                        PerformConversion(bytesPerSecond, input, output);
                }
                else
                {
                    DialogService.ShowMessageBox("Unable to download this video");
                    UpdateWorkStatus(WorkStatus.Error);
                }
            }

            catch (OperationCanceledException)
            {
                // Supress
            }
        }

        /// <summary>
        /// Retrieves the YouTube video of the highest possible quality.
        /// The YouTube video must be in .mp4-format since the NAudio converter may not be able to handle it otherwise.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private VideoInfo GetVideo(string url)
        {
            var results = DownloadUrlResolver.GetDownloadUrls(url).ToList();

            VideoInfo video = null;
            if (results.Any())
                video = results
                    .Where(v => v.VideoExtension == ".mp4")
                    .OrderByDescending(v => v.AudioBitrate)
                    .ThenBy(v => v.Resolution)
                    .FirstOrDefault();

            return video;
        }

        /// <summary>
        /// Cleans a string from chars that cannot be used as a file name
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string CleanNameFromIllegalChars(string title)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            string input = r.Replace(title, "");
            return input;
        }

        /// <summary>
        /// Validates downloaded video file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool TryOpenInputFile(string file)
        {
            bool isValid = false;
            try
            {
                using (var reader = new MediaFoundationReader(file))
                {
                    inputWaveFormat = reader.WaveFormat;
                    isValid = true;
                }
            }
            catch (Exception e)
            {
                DialogService.ShowMessageBox(String.Format("The video is not in a supported format. ({0})", e.Message));
            }
            return isValid;
        }

        /// <summary>
        /// Creates an MP3-file from the downloaded video file
        /// </summary>
        /// <param name="bytesPerSecond">Audio bitrate in bytes per second</param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        private void PerformConversion(int bytesPerSecond, string input, string output)
        {
            var allMediaTypes = new Dictionary<Guid, List<MediaType>>();
            var list = MediaFoundationEncoder.GetOutputMediaTypes(AudioSubtypes.MFAudioFormat_MP3).ToList();
            allMediaTypes[AudioSubtypes.MFAudioFormat_MP3] = list;

            // Keep audio properties from the original video file
            var supportedMediaTypes = allMediaTypes[AudioSubtypes.MFAudioFormat_MP3]
                .Where(m => m != null)
                .Where(m => m.SampleRate == inputWaveFormat.SampleRate)
                .Where(m => m.ChannelCount == inputWaveFormat.Channels)
                .ToList();

            var mediaType = supportedMediaTypes.FirstOrDefault(m => m.AverageBytesPerSecond == bytesPerSecond) ??
                            supportedMediaTypes.FirstOrDefault();

            if (mediaType != null)
            {
                using (var reader = new MediaFoundationReader(input))
                {
                    using (var encoder = new MediaFoundationEncoder(mediaType))
                    {
                        encoder.Encode(output, reader);
                    }
                }
            }

            // Cleanup before throwing cancellation 
            if (tokenSource.IsCancellationRequested)
            {
                File.Delete(output);
                File.Delete(input);
                tokenSource.Token.ThrowIfCancellationRequested();
            }

            UpdateWorkStatus(WorkStatus.Finished);
        }

        /// <summary>
        /// Convert the audio bitrate from kbps to bps (bytes per second)
        /// </summary>
        /// <param name="audioBitrate"></param>
        /// <returns></returns>
        private int ConvertBitrate(int audioBitrate)
        {
            if (audioBitrate >= 320)
                return 40000;
            if (audioBitrate >= 256)
                return 32000;
            if (audioBitrate >= 192)
                return 24000;
            if (audioBitrate >= 160)
                return 20000;

            // Assume that the bitrate is 192 if audioBitrate is equal to 0
            return audioBitrate >= 128 ? 16000 : 24000;
        }

        /// <summary>
        /// Handles progress feedback to GUI
        /// </summary>
        /// <param name="workStatus"></param>
        private void UpdateWorkStatus(WorkStatus workStatus)
        {
            switch (workStatus)
            {
                case WorkStatus.Locating:
                    IsDownloading = true;
                    IsIndeterminate = true;
                    Status = "Locating YouTube video...";
                    break;
                case WorkStatus.Downloading:
                    IsIndeterminate = false;
                    Status = "Downloading media...";
                    break;
                case WorkStatus.Creating:
                    IsIndeterminate = true;
                    Status = "Creating MP3...";
                    break;
                case WorkStatus.Finished:
                    IsIndeterminate = false;
                    Status = "Done";
                    IsDownloading = false;
                    break;
                case WorkStatus.Error:
                    IsIndeterminate = false;
                    Status = "An error occured";
                    IsDownloading = false;
                    break;
                case WorkStatus.Canceled:
                    IsIndeterminate = false;
                    DownloadProgress = 0;
                    Status = "Download was canceled";
                    IsDownloading = false;
                    break;
            }
        }

        /// <summary>
        /// Should be called upon application shutdown to cancel any ongoing downloads
        /// </summary>
        /// <param name="msg"></param>
        protected override void ClearDownload(ClearMessage msg)
        {
            // No specified 'DownloadItemViewModel' means that all instances of it should be canceled
            if (msg.DownloadItemViewModel == null)
            {
                Cancel();
                Dispose();
                videoDownloader = null;
            }
        }

        /// <summary>
        /// Cancellation token should be disposed
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                    tokenSource = null;
                }
            }
        }
    }
}