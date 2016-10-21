using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
