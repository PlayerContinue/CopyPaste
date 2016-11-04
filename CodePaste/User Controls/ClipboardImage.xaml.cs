using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CodePaste.User_Controls
{
    /// <summary>
    /// Interaction logic for ClipboardImage.xaml
    /// </summary>
    public partial class ClipboardImage : UserControl
    {
        public ClipboardImage()
        {
            InitializeComponent();
        }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            ClipboardDataContainer _container = this.DataContext as ClipboardDataContainer;

            Clipboard.SetImage(_container.ImageValue as BitmapSource);
        }
    }
}