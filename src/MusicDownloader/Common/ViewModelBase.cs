using System.ComponentModel;
using System.Runtime.CompilerServices;
using MusicDownloader.Properties;

namespace MusicDownloader.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase()
        {
            // Subscribe to Events
            EventSystem.Subscribe<DownloadMessage>(NewDownload);
            EventSystem.Subscribe<ClearMessage>(ClearDownload);
        }

        public IDialogService DialogService
        {
            get { return (IDialogService)ServiceContainer.Container.GetService(typeof(IDialogService)); }
        }

        protected virtual void NewDownload(DownloadMessage msg)
        {
        }

        protected virtual void ClearDownload(ClearMessage msg)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}