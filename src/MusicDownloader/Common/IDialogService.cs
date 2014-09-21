using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusicDownloader.Common
{
    public interface IDialogService
    {
        /// <summary>
        ///     Shows a message box with a message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        MessageBoxResult ShowMessageBox(string message, string title = null,
            MessageBoxButton buttons = MessageBoxButton.OK);

        /// <summary>
        ///     Shows the open file dialog
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> ShowOpenFileDialog(string initialDirectory);

        /// <summary>
        ///     Shows the open folder dialog
        /// </summary>
        /// <returns>Returns the path of the folder or null if canceled</returns>
        string ShowFolderBrowserDialog();

        /// <summary>
        ///     Attempts to show a particular file in Windows Explorer
        /// </summary>
        /// <param name="path">The full path of the file including its name</param>
        void ShowFileInExplorer(string path);

        /// <summary>
        /// Opens a folder in Windows Explorer
        /// </summary>
        /// <param name="folder"></param>
        void OpenFolder(string folder);
    }
}
