using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using DropBoxLinker.Properties;
using WinForms = System.Windows.Forms;

namespace DropBoxLinker
{
    public partial class frmSettings : Window
    {
        public frmSettings()
        {
            InitializeComponent();

            // if sync path unset/not-exists, try to get it from db
            if (String.IsNullOrEmpty(Settings.Default.SyncDirectory) ||
               !Directory.Exists(Settings.Default.SyncDirectory))
                Settings.Default.SyncDirectory = App.GetDropboxPublicPath();

            // if sync path is still unknown, display input
            panSyncDirectory.Visibility =
                Settings.Default.SyncDirectory == null ?
                    Visibility.Visible : Visibility.Collapsed;

            // load & show settings
            settings = new SettingsModel {
                SyncDirectory = Settings.Default.SyncDirectory,
                UserID = Settings.Default.UserID,
                PopupTimeout = Settings.Default.PopupTimeout,
                CleanClipboard = Settings.Default.CleanClipboard,
                AutoStartup = AutoStartup.State };
            DataContext = settings;
        }
        private SettingsModel settings;

        // general handlers
        private void OnApply(object sender, RoutedEventArgs e)
        {
            var h = this.ActualHeight;

            // check sync folder
            if (!Directory.Exists(settings.SyncDirectory)) {
                panSyncDirectory.Visibility = Visibility.Visible;
                MessageBox.Show("Selected sync directory not exists!", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
                txtSyncDirectory.Focus(); txtSyncDirectory.SelectAll(); return;
            }

            // check user id
            ulong user_id; var ok = ulong.TryParse(txtUserID.Text, out user_id);
            if (!ok) {
                MessageBox.Show("Invalid user id!", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
                txtUserID.Focus(); txtUserID.SelectAll(); return; }

            // save
            AutoStartup.State = settings.AutoStartup;
            Settings.Default.SyncDirectory = settings.SyncDirectory;
            Settings.Default.UserID = user_id;
            Settings.Default.PopupTimeout = settings.PopupTimeout;
            Settings.Default.CleanClipboard = settings.CleanClipboard;
            Settings.Default.Save();

            // restart watcher, if started
            if (App.watcher != null) {
                App.StopWatcher();
                App.LaunchWatcher(); }

            // info
            MessageBox.Show("User settings successfully saved", "Done",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // internal handlers
        private void OnSelectSyncDirectory(object sender, MouseButtonEventArgs e)
        {
            // create dialog
            var dlg = new WinForms.FolderBrowserDialog
                { Description = "Select DropBox public sync directory" };

            // get entered and saved paths
            var entered = txtSyncDirectory.Text;
            var setted = Settings.Default.SyncDirectory;

            // use first existing path
            if (Directory.Exists(entered)) dlg.SelectedPath = entered;
            else if (Directory.Exists(setted)) dlg.SelectedPath = setted;
            
            // show and apply path
            if (dlg.ShowDialog() == WinForms.DialogResult.OK)
                txtSyncDirectory.Text = dlg.SelectedPath;
        }
        private void OnWhereToGetUserID(object sender, MouseButtonEventArgs e)
        {
            // wait
            Cursor = Cursors.Wait;

            // open image
            var uri = new Uri(@"/graphics/WhereToGetUserID.png", UriKind.Relative);
            var resource = Application.GetResourceStream(uri);
            
            // read data
            var len = (int)resource.Stream.Length;
            var data = new Byte[len];
            resource.Stream.Read(data, 0, len);

            // write to temp file
            var path = Path.GetTempFileName().Replace(".tmp", ".png");
            File.WriteAllBytes(path, data);

            // done
            Cursor = Cursors.Arrow;

            // launch
            Process.Start(path);
        }
        private void OnPopupTimeoutChanged(object sender, MouseButtonEventArgs e)
        {
            frmNotify.Preview(settings.PopupTimeout);
        }
        private void OnToggleStartup(object sender, MouseButtonEventArgs e)
        {
            var s = (DataContext as SettingsModel);
            s.AutoStartup = !s.AutoStartup;
        }
 
    }
}
