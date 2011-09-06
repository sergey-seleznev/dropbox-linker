using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using DropBoxLinker.Properties;
using WinForms = System.Windows.Forms;
using System.Drawing.Imaging;

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
                new WinForms.MenuItem("Capture screenshot", OnTrayCaptureScreenshot),
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
        private static void OnTrayCaptureScreenshot(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(300);

            //var name = String.Format("Screenshot-{0:dd.MM.yyyy}-{0:HH-mm-ss}")

            //var name = DateTime.Now.ToString("Screen")

            var w = WinForms.Screen.GetBounds(new System.Drawing.Point(0, 0)).Width;
            int h = WinForms.Screen.GetBounds(new System.Drawing.Point(0, 0)).Height;

            var bmp = new Bitmap(w, h);
            var gfx = Graphics.FromImage(bmp as Image);

            gfx.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(w, h));

            bmp.Save(@"D:\screen_" + DateTime.Now.Ticks.ToString() + ".png", ImageFormat.Png);

            //tray.ContextMenu.Collapse += OnTrayMenuCollapsed;

            
        }
        private static void OnTrayMenuCollapsed(object sender, EventArgs e)
        {
            tray.ContextMenu.Collapse -= OnTrayMenuCollapsed;

            
            //new frmGetLinks().ShowDialog();
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
