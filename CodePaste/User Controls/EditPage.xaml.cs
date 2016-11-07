using System;
using System.Windows;
using System.Windows.Controls;

namespace CodePaste.User_Controls
{
    /// <summary>
    /// Interaction logic for CopyPage.xaml
    /// </summary>
    public partial class EditPage : UserControl
    {
        private String _PreviousTitle;

        public EditPage()
        {
            InitializeComponent();
        }

        private void OnSelectionChange(object sender, RoutedEventArgs e)
        {
          this._PreviousTitle = ((MainControlModel)this.DataContext).ConnectionView.CodeEntry;
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
                EditXMLDocument.EditXML(((MainControlModel)this.DataContext).Document, this._PreviousTitle, ((MainControlModel)this.DataContext).ConnectionView.NewCodeEntry, ((MainControlModel)this.DataContext).ConnectionView.Code.Copy);
            }
            catch
            {
                MessageBoxResult rsltMessageBox = MessageBox.Show("Could not find node with that name", "Fail");
            }
        }
    }
}