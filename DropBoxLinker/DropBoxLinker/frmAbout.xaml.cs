using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DropBoxLinker
{
    public partial class frmAbout : Window
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        // links
        private void OpenAboutMe(object sender, MouseButtonEventArgs e)
        {
            Process.Start(@"http://about.me/sergey-seleznev");
        }
        private void OpenMosinterNet(object sender, MouseButtonEventArgs e)
        {
            Process.Start(@"http://sersel.mosinter.net/");
        }
        private void OpenMail(object sender, MouseButtonEventArgs e)
        {
            Process.Start(@"mailto://webdesigner@russia.ru");
        }
        private void OpenICQ(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText("339097495");
            MessageBox.Show("Author's ICQ # has been copied to your clipboard.",
                "ICQ", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // commands
        private void OnClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
