using System.Windows;
using MusicDownloader.Common;

namespace MusicDownloader
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new Bootstrapper().Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cleaner.CleanWorkDirectory();
            base.OnExit(e);
        }
    }
}