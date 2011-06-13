using System;
using System.Windows;
using DropBoxLinker.Properties;

namespace DropBoxLinker
{


    public partial class frmNotify : Window
    {
        public frmNotify()
        {
            InitializeComponent();
        }

        private String NotifyTypeText(NotifyType type)
        {
            switch (type)
            {
                case NotifyType.Preview:    return "that's just a popup preview";

                case NotifyType.Set:        return "public link copied to clipboard";
                case NotifyType.Append:     return "public link appended to clipboard";
                case NotifyType.Update:     return "public link updated in clipboard";
                case NotifyType.Remove:     return "public link removed from clipboard";

                default: return "";
            }
        }

        public void Launch(int index, string title, string ext, NotifyType type, float? timeout = null)
        {
            // set file info
            if (type != NotifyType.Preview) {
                txtTitle.Text = title;
                txtExt.Text = ext; }
            else {
                txtTitle.Text = "some-sample-file";
                txtExt.Text = ".txt"; }
            
            // set state
            txtState.Text = NotifyTypeText(type);
                        
            // set timeout
            PopupHideAnimation.BeginTime =
                TimeSpan.FromSeconds((timeout.HasValue) ?
                timeout.Value : Settings.Default.PopupTimeout);

            // set opacity
            Opacity = 0.0d;

            // change location on loaded
            Loaded += (o, e) =>
            {
                // screen resolution (w/o menu)
                var screen_width = SystemParameters.WorkArea.Width;
                var screen_height = SystemParameters.WorkArea.Height;

                // set position
                Top = screen_height - ActualHeight - 5 - (index - 1) * (ActualHeight + 10);
                Left = screen_width - ActualWidth - 5;
            };
            
            // start-up
            Show();
        }

        private void AnimationComplete(object sender, EventArgs e)
        {
            // finished
            Close();
        }
        
    }

    public enum NotifyType
    {
        Preview,    // preview the popup

        Set,        // set to new link
        Append,     // append new link
        Update,     // update old link
        Remove,     // remove old link
    }
}
