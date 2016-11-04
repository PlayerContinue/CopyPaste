using CodePaste.Controller;
using System;
using System.Windows;
using System.Windows.Controls;
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
            EditXMLDocument.AddToXML(((MainControlModel)this.DataContext).Document, this.Title.Text, this.Description.Text);
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
        /// <summary>
        /// Add a new XML Node to the document
        /// </summary>
        /// <param name="document">The document to be added to</param>
        /// <param name="title">The new name of the node</param>
        /// <param name="description">The new description of the node</param>
        public static void AddToXML(XMLDocumentModel document, string title, string description)
        {
            try
            {
                XmlNode _root = document.Document.GetElementsByTagName("root")[0];
                XmlElement _newElement = document.Document.CreateElement("node");
                _newElement.InnerText = description;
                _newElement.SetAttribute("name", title);
                _root.AppendChild(_newElement);
                document.SaveXML();
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        /// <summary>
        /// Replace the value of a node with a new value
        /// </summary>
        /// <param name="document">The document to be edited</param>
        /// <param name="previousTitle">The title of the node to be replaced</param>
        /// <param name="newtitle">The new title</param>
        /// <param name="newDescription">The new description</param>
        public static void EditXML(XMLDocumentModel document, string previousTitle, string newtitle, string newDescription)
        {
            //Select the node with the given title
            XmlNode _node = document.Document.SelectSingleNode(String.Format("root/node[@name='{0}']", previousTitle));
            //Replace the name attribute with the new title
            _node.Attributes["name"].Value = newtitle;
            //Replace the description
            _node.InnerText = newDescription;
            document.SaveXML();
        }
    }
}