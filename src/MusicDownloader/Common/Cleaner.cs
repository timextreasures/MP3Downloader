using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MusicDownloader.Properties;

namespace MusicDownloader.Common
{
    public static class Cleaner
    {
        /// <summary>
        /// Deletes video files from uncompleted downloads if any exists
        /// </summary>
        public static void CleanWorkDirectory()
        {
            try
            {
                // Broadcast events to cancel any ongoing downloads
                EventSystem.Publish(new ClearMessage());

                // If any video files exists they'll be in the application's work folder
                var workDirectory = new DirectoryInfo(Path.GetFullPath(Settings.Default.WorkDirectory));

                // The application only downloads .mp4-files
                var files = workDirectory.GetFiles("*").Where(f => f.Extension == ".mp4").ToList();

                // Delete found files
                if (files.Any())
                {
                    foreach (FileInfo file in files)
                    {
                        // File might still be in use so wait for it to be available for deletion 
                        while (IsFileLocked(file))
                            Thread.Sleep(1000);
                        
                        file.Delete();
                    }
                }
            }

            catch (Exception)
            {
                // Suppress
            }
        }

        /// <summary>
        /// Checks if a file is busy 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // The file is unavailable
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            // The file is available
            return false;
        }
    }
}
