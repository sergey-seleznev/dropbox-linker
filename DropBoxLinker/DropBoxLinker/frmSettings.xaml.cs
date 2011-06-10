using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using DropBoxLinker.Properties;
using WinForms = System.Windows.Forms;

namespace DropBoxLinker
{
    public partial class frmSettings : Window
    {
        private App app;
        private class SettingsModel : INotifyPropertyChanged
        {
            private string _SyncDirectory;
            private ulong _UserID;
            private bool _AutoStartup;

            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged(String PropertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }

            public string SyncDirectory
            {
                get { return _SyncDirectory; }
                set { _SyncDirectory = value;
                    NotifyPropertyChanged("SyncDirectory"); }
            }
            public ulong UserID
            {
                get { return _UserID; }
                set { _UserID = value;
                    NotifyPropertyChanged("UserID"); }
            }
            public bool AutoStartup
            {
                get { return _AutoStartup; }
                set { _AutoStartup = value;
                    NotifyPropertyChanged("AutoStartup"); }
            }
        }
        private SettingsModel settings;

        public frmSettings()
        {
            InitializeComponent();

            // get application instance
            app = Application.Current as App;

            // load & show settings
            settings = new SettingsModel {
                SyncDirectory = Settings.Default.SyncDirectory,
                UserID = Settings.Default.UserID,
                AutoStartup = AutoStartup.State };
            DataContext = settings;
        }

        private void OnApply(object sender, RoutedEventArgs e)
        {
            var h = this.ActualHeight;

            // check sync folder
            if (!Directory.Exists(settings.SyncDirectory)) {
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
            Settings.Default.SyncDirectory = txtSyncDirectory.Text;
            Settings.Default.UserID = user_id;
            Settings.Default.Save();
            AutoStartup.State = settings.AutoStartup;

            // restart watcher, if started
            if (app.watcher != null) {
                app.StopWatcher();
                app.LaunchWatcher(); }

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
        private void OnToggleStartup(object sender, MouseButtonEventArgs e)
        {
            var s = (DataContext as SettingsModel);
            s.AutoStartup = !s.AutoStartup;
        }


    }

}
