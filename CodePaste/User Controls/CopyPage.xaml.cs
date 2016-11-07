using CodePaste.Controller;
using System.Windows;
using System.Windows.Controls;

namespace CodePaste.User_Controls
{
    /// <summary>
    /// Interaction logic for CopyPage.xaml
    /// </summary>
    public partial class CopyPage : UserControl
    {
        public CopyPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Copy the information to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Copy(object sender, RoutedEventArgs e)
        {
            MainControlModel _model = this.DataContext as MainControlModel;

            Clipboard.SetText(_model.ConnectionView.Code.Copy);
        }
    }
}