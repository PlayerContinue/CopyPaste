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
            Clipboard.SetText(Code_Data_Value.Text);
        }
    }
}