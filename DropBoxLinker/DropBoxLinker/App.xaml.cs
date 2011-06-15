using System.Windows;

namespace DropBoxLinker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // validate settings
            if (!ValidateSettings())
            {
                // by default, enable autostart
                AutoStartup.State = true;
                
                // try to get settings from user
                var res = new frmSettings().ShowDialog();
                if (res != true) { Shutdown(); return; }
            }

            // launch tray & watcher
            LaunchTrayIcon();
            LaunchWatcher();
        }
    }
}
