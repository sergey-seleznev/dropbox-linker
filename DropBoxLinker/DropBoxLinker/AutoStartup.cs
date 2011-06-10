using System;
using Microsoft.Win32;
using System.Reflection;

namespace DropBoxLinker
{
    public static class AutoStartup
    {
        private static readonly string run_path = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string exe_path = Assembly.GetExecutingAssembly().Location;
        private static readonly string exe_name = @"DropBox Linker";
        
        public static Boolean State
        {
            get
            {
                // open and check registry key
                var key = Registry.CurrentUser.OpenSubKey(run_path, true);
                if (key == null) return false;

                // get and check key value
                var value = key.GetValue(exe_name) as string;
                if (value == null) return false;
                
                // exists - check exe_path
                if (value != exe_path)
                    key.SetValue(exe_name, exe_path);
                return true;
            }
            set
            {
                // get & correct current state, if needed
                var current_state = State;
                if (value && !current_state)
                {
                    // enable
                    var key = Registry.CurrentUser.CreateSubKey(run_path, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    key.SetValue(exe_name, exe_path);
                }
                else if (!value && current_state)
                {
                    // disable
                    var key = Registry.CurrentUser.CreateSubKey(run_path, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    key.DeleteValue(exe_name);
                }
            }
        }

    }
}
