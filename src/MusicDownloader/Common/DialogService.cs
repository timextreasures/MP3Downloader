using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MusicDownloader.Common
{
    public class DialogService : IDialogService
    {
        public MessageBoxResult ShowMessageBox(string message, string title = null,
            MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return MessageBox.Show(message, title, buttons);
        }

        public IEnumerable<string> ShowOpenFileDialog(string initialDirectory = null)
        {
            return null;
        }

        public string ShowFolderBrowserDialog()
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
            }
        }

        public void ShowFileInExplorer(string path)
        {
            Process.Start("explorer.exe", "/select," + path);
        }

        public void OpenFolder(string folder)
        {
            Process.Start(folder);
        }
    }
}