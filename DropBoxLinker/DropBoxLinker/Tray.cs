using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using DropBoxLinker.Properties;
using WinForms = System.Windows.Forms;

namespace DropBoxLinker
{
    public partial class App : Application
    {
        private static WinForms.NotifyIcon tray;

        // launch / destroy
        public static void LaunchTrayIcon()
        {
            // load icon
            var uri = new Uri(@"/graphics/tray.ico", UriKind.Relative);
            var resource = Application.GetResourceStream(uri);

            // create menu items
            var menu = new WinForms.MenuItem[] {
                new WinForms.MenuItem("Get links...", OnTrayGetLinks) { DefaultItem = true },
                new WinForms.MenuItem("Open Public", OnTrayOpenFolder),
                new WinForms.MenuItem("-"),
                new WinForms.MenuItem("Settings", OnTraySettings),
                new WinForms.MenuItem("About", OnTrayAbout),
                new WinForms.MenuItem("-"),
                new WinForms.MenuItem("Exit", OnTrayExit) };

            // create tray icon
            tray = new WinForms.NotifyIcon {
                Text = "DropBox Linker",
                Icon = new Icon(resource.Stream),
                ContextMenu = new WinForms.ContextMenu(menu),
                Visible = true };
            tray.DoubleClick += OnTrayGetLinks;
        }
        public static void DestroyTrayIcon()
        {
            if (tray != null)
            {
                tray.Visible = false;
                tray.Dispose();
                tray = null;
            }
        }

        // menu item click handlers
        private static void OnTrayGetLinks(object sender, EventArgs e)
        {
            new frmGetLinks().ShowDialog();
        }
        private static void OnTrayOpenFolder(object sender, EventArgs e)
        {
            Process.Start(Settings.Default.SyncDirectory);
        }
        private static void OnTraySettings(object sender, EventArgs e)
        {
            new frmSettings().ShowDialog();
        }
        private static void OnTrayAbout(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }
        private static void OnTrayExit(object sender, EventArgs e)
        {
            StopWatcher();
            DestroyTrayIcon();
            Application.Current.Shutdown();
        }

    }
}
