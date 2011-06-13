using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using DropBoxLinker.Properties;
using WinForms = System.Windows.Forms;

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
                // add to auto startup
                AutoStartup.State = true;

                // try to get settings
                var res = new frmSettings().ShowDialog();
                if (res != true) { Shutdown(); return; }
            }

            // launch tray & watcher
            LaunchTrayIcon();
            LaunchWatcher();
        }

        // settings
        public bool ValidateSettings()
        {
            // update
            Settings.Default.Reload();

            // check & correct Startup path
            var AS = AutoStartup.State;

            // check-up
            return
                Directory.Exists(Settings.Default.SyncDirectory) &&
                Settings.Default.UserID != 0 &&
                Settings.Default.PopupTimeout >= 1.0 &&
                Settings.Default.PopupTimeout <= 5.0;
        }

        // tray
        private WinForms.NotifyIcon tray;
        public void LaunchTrayIcon()
        {
            // load icon
            var uri = new Uri(@"/graphics/tray.ico", UriKind.Relative);
            var resource = Application.GetResourceStream(uri);

            // create menu items
            var menu = new WinForms.MenuItem[] {
                new WinForms.MenuItem("Open Public", OnTrayOpenFolder) { DefaultItem = true },
                new WinForms.MenuItem("Settings", OnTraySettings),
                new WinForms.MenuItem("About", OnTrayAbout),
                new WinForms.MenuItem("-"),
                new WinForms.MenuItem("Exit", OnTrayExit) };

            // create tray icon
            tray = new WinForms.NotifyIcon {
                Icon = new Icon(resource.Stream),
                ContextMenu = new WinForms.ContextMenu(menu),
                Visible = true };
            tray.DoubleClick += OnTrayOpenFolder;
        }
        public void DestroyTrayIcon()
        {
            if (tray != null)
            {
                tray.Visible = false;
                tray.Dispose();
                tray = null;
            }
        }
        private void OnTrayOpenFolder(object sender, EventArgs e)
        {
            Process.Start(Settings.Default.SyncDirectory);
        }
        private void OnTraySettings(object sender, EventArgs e)
        {
            new frmSettings().ShowDialog();
        }
        private void OnTrayAbout(object sender, EventArgs e)
        {
            new frmAbout().ShowDialog();
        }
        private void OnTrayExit(object sender, EventArgs e)
        {
            StopWatcher();
            DestroyTrayIcon();
            Application.Current.Shutdown();
        }

        // watching
        public FileSystemWatcher watcher;
        public void LaunchWatcher()
        {
            // create watcher
            var dir = Settings.Default.SyncDirectory;
            watcher = new FileSystemWatcher(dir);

            // add event handlers
            watcher.Renamed += OnFileRenamed;
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileDeleted;

            // launch watching
            watcher.EnableRaisingEvents = true;
        }
        public void StopWatcher()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher = null;
            }
        }
        private string GetPublicURL(string name)
        {
            return String.Format(@"{0}/{1}/{2}",
                Settings.Default.HttpPath,
                Settings.Default.UserID,
                name);
        }
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // incomplete file
            if (!File.Exists(e.FullPath)) return;

            this.Dispatcher.Invoke(new Action(() =>
            {
                // create link
                var url = GetPublicURL(e.Name);

                // update clipboard
                if (Clipboard.ContainsText())
                {
                    // get existing text
                    var text = Clipboard.GetText();

                    // already contains url?
                    if (text.Contains(url))
                    {
                        // do nothing
                        return;
                    }

                    // are there any dropbox links?
                    else if (text.Contains(Settings.Default.HttpPath) && !Settings.Default.CleanClipboard)
                    {
                        // append new-url to the end
                        Clipboard.SetText(text + "\r\n" + url);
                        LaunchNotification(e.FullPath, NotifyType.Append);
                    }

                    // something special
                    else
                    {
                        // overwrite with url
                        Clipboard.SetText(url);
                        LaunchNotification(e.FullPath, NotifyType.Set);
                    }
                }
                else
                {
                    // overwrite with url
                    Clipboard.SetText(url);
                    LaunchNotification(e.FullPath, NotifyType.Set);
                }

            }));
        }
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            // incomplete file
            if (!File.Exists(e.FullPath)) return;

            this.Dispatcher.Invoke(new Action(() =>
            {
                // create links
                var old_url = GetPublicURL(e.OldName);
                var new_url = GetPublicURL(e.Name);

                // update clipboard
                if (Clipboard.ContainsText())
                {
                    // get existing text
                    var text = Clipboard.GetText();

                    // already contains new-url?
                    if (text.Contains(new_url))
                    {
                        // do nothing
                        return;
                    }

                    // contains old-url?
                    else if (text.Contains(old_url))
                    {
                        // replace with new-url
                        Clipboard.SetText(text.Replace(old_url, new_url));
                        LaunchNotification(e.FullPath, NotifyType.Update);
                    }

                    // are there any dropbox links?
                    else if (text.Contains(Settings.Default.HttpPath) && !Settings.Default.CleanClipboard)
                    {
                        // append new-url to the end
                        Clipboard.SetText(text + "\r\n" + new_url);
                        LaunchNotification(e.FullPath, NotifyType.Append);
                    }

                    // something special
                    else
                    {
                        // overwrite with new-url
                        Clipboard.SetText(new_url);
                        LaunchNotification(e.FullPath, NotifyType.Set);
                    }
                }
                else
                {
                    // overwrite with new-url
                    Clipboard.SetText(new_url);
                    LaunchNotification(e.FullPath, NotifyType.Set);
                }

            }));
        }
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                // create links
                var url = GetPublicURL(e.Name);

                // update clipboard
                if (Clipboard.ContainsText())
                {
                    // get existing text
                    var text = Clipboard.GetText();

                    // does it contain deleted url?
                    if (!text.Contains(url)) return;

                    // replace (w and w/o line-break)
                    text = text.Replace(url + "\r\n", "");
                    text = text.Replace(url, "");
                    Clipboard.SetText(text);

                    // notify user
                    LaunchNotification(e.FullPath, NotifyType.Remove);
                }
            }));
        }

        // notifications
        private int notifications = 0;
        private void LaunchNotification(string file, NotifyType type)
        {
            // notifications++
            Interlocked.Increment(ref notifications);

            // create notifier
            var notify = new frmNotify();

            // call notifications-- on close
            notify.Closed += (o, a) => {
                Interlocked.Decrement(ref notifications); };

            // get name and extension
            var info = new FileInfo(file);
            var name = info.Name.Replace(info.Extension, "");
            var ext = info.Extension.ToLower();

            // launch
            notify.Launch(notifications, name, ext, type);
        }

    }
}
