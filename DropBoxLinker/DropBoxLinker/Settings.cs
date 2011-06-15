using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using DropBoxLinker.Properties;

namespace DropBoxLinker
{
    // app extension
    public partial class App : Application
    {
        // validate settings
        public static bool ValidateSettings()
        {
            // update
            Settings.Default.Reload();

            // check & correct Startup path
            var AS = AutoStartup.State;

            // check-up
            return
                Directory.Exists(Settings.Default.SyncDirectory) &&
                Settings.Default.SyncDirectory.EndsWith(@"\") &&
                Settings.Default.UserID != 0 &&
                Settings.Default.PopupTimeout >= 1.0 &&
                Settings.Default.PopupTimeout <= 5.0;
        }

        // try get dropbox path
        public static string GetDropboxPublicPath()
        {
            // get 'c:/users/.../AppData/' path
            var AppDataPath = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);

            // get 'host.db' path
            var HostDbPath = AppDataPath + @"\DropBox\host.db";

            // check for file existance
            if (!File.Exists(HostDbPath)) return null;

            // read 'host.db' lines (encoded with Base64)
            var HostDbLines = File.ReadAllLines(HostDbPath);
            if (HostDbLines.Count() == 0) return null;

            // get last line
            var HostDbLastLine = HostDbLines.Last();
            if (string.IsNullOrEmpty(HostDbLastLine))
                return null;

            // check for valid Base64 string
            var Base64Regex = @"(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})";
            if (!Regex.IsMatch(HostDbLastLine, Base64Regex))
                return null;

            // convert to Base64 bytes, then to string
            var HostDbBytes = Convert.FromBase64String(HostDbLastLine);
            var HostDbChars = HostDbBytes.Select(b => (char)b).ToArray();
            if (HostDbChars.Length == 0) return null;

            // check folder
            var Path = new string(HostDbChars);
            if (!Directory.Exists(Path)) return null;

            // append Public folder
            Path += (Path.EndsWith(@"\")) ?
                    @"Public\" : @"\Public\";

            // return
            return Path;
        }
    }

    // settings model
    public class SettingsModel : INotifyPropertyChanged
    {
        // private storage
        private string _SyncDirectory;
        private ulong _UserID;
        private bool _AutoStartup;
        private float _PopupTimeout;
        private bool _CleanClipboard;

        // public properties
        public string SyncDirectory
        {
            get { return _SyncDirectory; }
            set
            {
                if (_SyncDirectory == value) return;
                _SyncDirectory = value;
                NotifyPropertyChanged("SyncDirectory");
            }
        }
        public ulong UserID
        {
            get { return _UserID; }
            set
            {
                if (_UserID == value) return;
                _UserID = value;
                NotifyPropertyChanged("UserID");
            }
        }
        public bool AutoStartup
        {
            get { return _AutoStartup; }
            set
            {
                if (_AutoStartup == value) return;
                _AutoStartup = value;
                NotifyPropertyChanged("AutoStartup");
            }
        }
        public float PopupTimeout
        {
            get { return _PopupTimeout; }
            set
            {
                if (_PopupTimeout == value) return;
                _PopupTimeout = value;
                NotifyPropertyChanged("PopupTimeout");
            }
        }
        public bool CleanClipboard
        {
            get { return _CleanClipboard; }
            set
            {
                if (_CleanClipboard == value) return;
                _CleanClipboard = value;
                NotifyPropertyChanged("CleanClipboard");
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String PropertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

}
