using System;
using System.IO;
using System.Linq;
using System.Windows;
using DropBoxLinker.Properties;

namespace DropBoxLinker
{
    public partial class App : Application
    {
        public static FileSystemWatcher watcher;

        // launch & stop
        public static void LaunchWatcher()
        {
            // create watcher
            var dir = Settings.Default.SyncDirectory;
            watcher = new FileSystemWatcher(dir)
                { IncludeSubdirectories = true };

            // add event handlers
            watcher.Renamed += OnFileRenamed;
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileDeleted;

            // launch watching
            watcher.EnableRaisingEvents = true;
        }
        public static void StopWatcher()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher = null;
            }
        }

        // file event handlers
        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // incomplete file
            if (!File.Exists(e.FullPath)) return;

            App.Current.Dispatcher.Invoke(new Action(() =>
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
                        frmNotify.Launch(e.FullPath, NotifyType.Append);
                    }

                    // something special
                    else
                    {
                        // overwrite with url
                        Clipboard.SetText(url);
                        frmNotify.Launch(e.FullPath, NotifyType.Set);
                    }
                }
                else
                {
                    // overwrite with url
                    Clipboard.SetText(url);
                    frmNotify.Launch(e.FullPath, NotifyType.Set);
                }

            }));
        }
        private static void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            // incomplete file
            if (!File.Exists(e.FullPath)) return;

            App.Current.Dispatcher.Invoke(new Action(() =>
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
                        frmNotify.Launch(e.FullPath, NotifyType.Update);
                    }

                    // are there any dropbox links?
                    else if (text.Contains(Settings.Default.HttpPath) && !Settings.Default.CleanClipboard)
                    {
                        // append new-url to the end
                        Clipboard.SetText(text + "\r\n" + new_url);
                        frmNotify.Launch(e.FullPath, NotifyType.Append);
                    }

                    // something special
                    else
                    {
                        // overwrite with new-url
                        Clipboard.SetText(new_url);
                        frmNotify.Launch(e.FullPath, NotifyType.Set);
                    }
                }
                else
                {
                    // overwrite with new-url
                    Clipboard.SetText(new_url);
                    frmNotify.Launch(e.FullPath, NotifyType.Set);
                }

            }));
        }
        private static void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
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
                    frmNotify.Launch(e.FullPath, NotifyType.Remove);
                }
            }));
        }

        // get valid URL for file
        private static string GetPublicURL(string path)
        {
            // convert to relative
            path = path.Replace(Settings.Default.SyncDirectory, "");

            // replace Windows slashes with network slashes
            path = path.Replace('\\', '/');

            // create full URL
            return String.Format(@"{0}/{1}/{2}",
                Settings.Default.HttpPath,
                Settings.Default.UserID,
                EncodePath(path));
        }
        private static string EncodePath(string path)
        {
            // get unique chars that require encoding
            var CharsToEncode = path
                .Distinct().Where(c =>
                    !Char.IsLetterOrDigit(c) && c != '%' &&
                    !ValidSpecialChars.Contains(c))
                .ToList();

            // first, encode '%' chars
            path = path.Replace("%", "%25");

            // then encode other invalid chars
            foreach (var c in CharsToEncode)
                path = path.Replace(new string(c, 1),
                        string.Format("%{0:X2}", (int)c));

            return path;
        }
        private static string ValidSpecialChars = @".-_/";

    }
}
