using CodePaste.Controller;
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
using System.Xml;

namespace CodePaste.User_Controls
{
    /// <summary>
    /// Interaction logic for AddNew.xaml
    /// </summary>
    public partial class AddNew : UserControl
    {
         
        public AddNew()
        {
            InitializeComponent();
        }

        private void AddToXML(object sender, RoutedEventArgs e)
        {
            try
            {
                XMLDocumentModel _document = ((MainControlModel)this.DataContext).Document;
                XmlNode _root = _document.Document.GetElementsByTagName("root")[0];
                XmlElement _newElement = _document.Document.CreateElement("node");
                _newElement.InnerText = this.Description.Text;
                _newElement.SetAttribute("name", this.Title.Text);
                _root.AppendChild(_newElement);
                _document.SaveXML();
                ClearText();
            }
            catch(Exception x) {
                throw x; 
            }
        }

        public void ClearText()
        {
            this.Description.Text = "";
            this.Title.Text = "";
        }
        

        

    }
}
