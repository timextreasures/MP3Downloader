using MusicDownloader.Common;

namespace MusicDownloader
{
    public class Bootstrapper
    {
        public void Run()
        {
            AddServices();
            new MainWindow().Show();
        }

        private void AddServices()
        {
            ServiceContainer.Container.AddService(typeof(IDialogService), new DialogService());
        }
    }
}
