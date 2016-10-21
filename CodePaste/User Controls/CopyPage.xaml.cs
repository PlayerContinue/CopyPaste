using CodePaste.Controller;
using System;
using System.IO;
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
using System.Xml;
using System.Xml.Linq;
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
