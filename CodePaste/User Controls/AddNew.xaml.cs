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
            EditXMLDocument.AddToXML(((MainControlModel)this.DataContext).Document,this.Title.Text,this.Description.Text);
            ClearText();
        }

        public void ClearText()
        {
            this.Description.Text = "";
            this.Title.Text = "";
        }
        

        

    }

    /// <summary>
    /// Object containing function for editing an xmldocument for the current program
    /// </summary>
    public class EditXMLDocument
    {
        public static void AddToXML(XMLDocumentModel document,string title,string description){
            try
            {
                
                XmlNode _root = document.Document.GetElementsByTagName("root")[0];
                XmlElement _newElement = document.Document.CreateElement("node");
                _newElement.InnerText = description;
                _newElement.SetAttribute("name", title);
                _root.AppendChild(_newElement);
                document.SaveXML();
                
            }
            catch(Exception x) {
                throw x; 
            }
        }

        public static void EditXML(XMLDocumentModel document, string previousTitle, string newtitle, string newDescription)
        {

        }
    }
}
