using System;
using System.Windows;

namespace DropBoxLinker
{
    public partial class frmNotify : Window
    {
        public frmNotify()
        {
            InitializeComponent();
        }

        public void Launch(int index, string title, string ext, bool added = true)
        {
            // set information
            txtTitle.Text = title;
            txtExt.Text = ext;
            txtState.Text = added ?
                "public link copied to clipboard" :
                "public link removed from clipboard";

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
}
