using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using DropBoxLinker.Properties;

namespace DropBoxLinker
{
    public partial class frmNotify : Window
    {
        // launchers
        public static void Launch(string file, NotifyType type)
        {
            // create window
            var n = new frmNotify();

            // take a slot
            n.AssignSlot();

            // set file info
            var info = new FileInfo(file);
            n.txtTitle.Text = info.Name.Replace(info.Extension, "");
            n.txtExt.Text = info.Extension.ToLower();

            // set state
            n.txtState.Text = NotifyTypeText(type);

            // set timeout
            n.PopupHideAnimation.BeginTime =
                TimeSpan.FromSeconds(
                Settings.Default.PopupTimeout);

            // set opacity
            n.Opacity = 0.0d;

            // set location on loaded
            n.Loaded += n.ArrangeWindow;

            // start-up
            n.Show();
        }
        public static void Preview(float timeout)
        {
            // create window
            var n = new frmNotify();

            // take a slot
            n.AssignSlot();

            // set file info
            n.txtTitle.Text = "popup preview";
            n.txtExt.Text = String.Format("  {0:F1} sec.", timeout);

            // set state
            n.txtState.Text = NotifyTypeText(NotifyType.Preview);

            // set timeout
            n.PopupHideAnimation.BeginTime =
                TimeSpan.FromSeconds(timeout);

            // set opacity
            n.Opacity = 0.0d;

            // set location on loaded
            n.Loaded += n.ArrangeWindow;

            // start-up
            n.Show();
        }     

        // static stuff
        private static List<int> BusySlots = new List<int>();
        private static string NotifyTypeText(NotifyType type)
        {
            switch (type)
            {
                case NotifyType.Preview: return "you can preview popup timeout";

                case NotifyType.Set: return "public link copied to clipboard";
                case NotifyType.Append: return "public link appended to clipboard";
                case NotifyType.Update: return "public link updated in clipboard";
                case NotifyType.Remove: return "public link removed from clipboard";

                default: return "";
            }
        }    

        // instance stuff
        public frmNotify()
        {
            InitializeComponent();
        }
        private int CurrentSlot = 0;
        private void AssignSlot()
        {
            lock (BusySlots)
            {
                // find first free slot
                while (BusySlots.Contains(CurrentSlot)) {
                    CurrentSlot++; }

                // mark it busy
                BusySlots.Add(CurrentSlot);
            }
        }
        private void ArrangeWindow(object sender, EventArgs e)
        {
            // screen resolution (w/o menu)
            var screen_width = SystemParameters.WorkArea.Width;
            var screen_height = SystemParameters.WorkArea.Height;

            // set position
            Top = screen_height - ActualHeight - 5 - CurrentSlot * (ActualHeight + 10);
            Left = screen_width - ActualWidth - 5;
        }
        private void AnimationComplete(object sender, EventArgs e)
        {
            // free slot
            lock (BusySlots) {
                BusySlots.Remove(CurrentSlot); }

            Close();
        }  
    }

    // notification types
    public enum NotifyType
    {
        Preview,    // preview the popup

        Set,        // set to new link
        Append,     // append new link
        Update,     // update old link
        Remove,     // remove old link
    }
}
