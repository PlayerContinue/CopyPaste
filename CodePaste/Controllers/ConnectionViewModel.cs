using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.ComponentModel;
using CodePaste.Base_Classes;
using System.Xml;
using System.Windows;
namespace CodePaste.Controller
{

    public class XMLDocumentModel : ModelBase
    {
        private XmlDocument _Document;
        public XmlDocument Document{get{return _Document;} set{_Document=value;}}
        private string _Folder;

        public XMLDocumentModel(string folder)
        {
            _Document = new XmlDocument();
            _Folder = folder;
            try
            {
                //Attempt to load the document
                _Document.Load(folder);
            }
            catch//Cannot find the file, create a new one
            {
                string sMessageBoxText = "We cannot find test.xml, would you like to create one?";
                string sCaption = "";

                MessageBoxButton btnMessageBox = MessageBoxButton.YesNo;
                MessageBoxImage icnMessageBox = MessageBoxImage.Warning;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        CreateNewXMLDocument(folder);
                        break;

                    case MessageBoxResult.No:
                        Application.Current.Shutdown();
                        break;

                }
                
                
            }
        }

        /// <summary>
        /// Create a new XML doc if one does not exist
        /// </summary>
        /// <param name="folder">The location to save the file to</param>
        public void CreateNewXMLDocument(String folder)
        {
            _Document = new XmlDocument();
            XmlDeclaration xmlDeclaration = _Document.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = _Document.DocumentElement;
            _Document.InsertBefore(xmlDeclaration, root);
           
            //Create Root Node
            XmlElement element1 = _Document.CreateElement(string.Empty, "root", string.Empty);
            _Document.AppendChild(element1);
            
            _Document.Save(folder);
           
        }

        public void SaveXML(){
            _Document.Save(_Folder);
        }


    }

    /// <summary>
    /// The container of the code information
    /// </summary>
    public class CodeEntry : ModelBase
    {
        private string _name;
        private string _copy;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
        public string Copy { get { return _copy; } set { _copy = value; } }

        public CodeEntry(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Create a new copy of the CodeEntry
        /// </summary>
        /// <param name="name">The name of the data</param>
        /// <param name="copy">The value of the data</param>
        public CodeEntry(string name, string copy)
        {
            _name = name;
            _copy = copy;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    /// <summary>
    /// Model of Copyable values
    /// </summary>
   public class ConnectionViewModel : ModelBase
    {
        private readonly CollectionView _codeEntries;
        private string _codeEntry;
        private string _codeData;
        private CodeEntry _code;

        public ConnectionViewModel()
        {
            IList<CodeEntry> list = new List<CodeEntry>
            {
            };
            _codeEntries = new CollectionView(list);

        }

        /// <summary>
        /// Create a new ConnectionViewModel containing the provided list values
        /// </summary>
        /// <param name="list">List of CodeEntry Values to fill the system with</param>
        public ConnectionViewModel(IEnumerable<CodeEntry> list)
        {
            _codeEntries = new CollectionView(list.OrderBy(s => s.Name).ToList());

        }

        


        public CodeEntry Code
        {
            get { return _code; }
            set
            {
                if (_code == value) return;
                _code = value;
                OnPropertyChanged("Code");
            }
        }

        public CollectionView CodeEntries
        {
            get { return _codeEntries; }
        }

        public string CodeData
        {
            get { return _codeData; }
            set
            {
                if (_codeData == value) return;
                _codeData = value;
                OnPropertyChanged("CodeData");
            }
        }

        public string CodeEntry
        {
            get { return _codeEntry; }
            set
            {
                if (_codeEntry == value) return;
                _codeEntry = value;
                OnPropertyChanged("CodeEntry");
                Code = FindCodeEntry(value);
            }
        }

        private CodeEntry FindCodeEntry(String code)
        {
            foreach (CodeEntry _code in _codeEntries)
            {
                if (_code.Name.CompareTo(code) == 0)
                {
                    return _code;
                }
            }

            return null;
        }




    }








}
