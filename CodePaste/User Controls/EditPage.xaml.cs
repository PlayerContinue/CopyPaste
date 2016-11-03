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
    public partial class EditPage : UserControl
    {
        public EditPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Copy the information to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                EditXMLDocument.EditXML(((MainControlModel)this.DataContext).Document, ((MainControlModel)this.DataContext).ConnectionView.CodeEntry, this.Title.Text, this.Code_Data_Value.Text);
            }
            catch
            {
                MessageBoxResult rsltMessageBox = MessageBox.Show("Could not find node with that name", "Fail");
            }
        }


        

    }
}
