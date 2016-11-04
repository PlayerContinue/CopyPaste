using CodePaste.Base_Classes;
using CodePaste.Controller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;

namespace CodePaste.User_Controls
{
    public class MainControlModel : ModelBase
    {
        private XMLDocumentModel _Document;
        //The name of the current control in command

        private ConnectionViewModel _ConnectionView;

        private string _CurrentView;
        private CaptureClipboard _Clipboard;
        public XMLDocumentModel Document { get { return _Document; } set { _Document = value; } }
        public ConnectionViewModel ConnectionView { get { return _ConnectionView; } set { _ConnectionView = value; } }
        public CaptureClipboard Clipboard { get { return _Clipboard; } set { _Clipboard = value; } }

        public string CurrentView
        {
            get { return _CurrentView; }
            set
            {
                if (_CurrentView == value) return;

                _CurrentView = value;
                OnPropertyChanged("CurrentView");
            }
        }

        public MainControlModel(string folder, Window window)
        {
            _Document = new XMLDocumentModel(folder);
            _ConnectionView = LoadXMLFromFile("test.xml");
            _Clipboard = new CaptureClipboard(window, 9);
        }

        private ConnectionViewModel LoadXMLFromFile(String relativePath)
        {
            string _folder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), relativePath);

            List<CodeEntry> _listOfValues = new List<CodeEntry>();

            ReadFromXML(_folder, "node", ref _listOfValues);

            return new ConnectionViewModel(_listOfValues);
        }

        /// <summary>
        /// Read through and parse an xml page
        /// </summary>
        /// <param name="folder">The folder of the xml file</param>
        /// <param name="parentNode">The name of the parent node which contains the information</param>
        /// <param name="listOfValues">The storage list</param>
        private void ReadFromXML(String folder, String parentNode, ref List<CodeEntry> listOfValues)
        {
            String _name;
            String _value;
            XmlTextReader _reader = new XmlTextReader(folder);
            //Traverse the xmlfile for it's values
            while (_reader.Read())
            {
                _reader.ReadToFollowing(parentNode);
                //Store the data from nodes

                _name = _reader["name"];
                //Store values in the list if they are not whitespace
                if (!String.IsNullOrWhiteSpace(_name) && _reader.Read())
                {
                    _value = _reader.ReadContentAsString();
                    _value = _value.TrimEnd('\r', '\n').TrimStart('\r', '\n');
                    listOfValues.Add(new CodeEntry(_name, _value));
                }
            }
            _reader.Close();
        }

        public void reloadXML()
        {
            _ConnectionView = LoadXMLFromFile("test.xml");
        }
    }

    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        private Dictionary<String, UserControl> _Cache;
        private Dictionary<String, ModelBase> _BaseCache;

        private static readonly Dictionary<String, Type> _UserControlTypes = new Dictionary<string, Type>() {
        { "AddNew", typeof(AddNew) },
        { "AddCopy", typeof(CopyPage) },
        { "Clipboard", typeof(ClipboardCapture) },
        { "Edit", typeof(EditPage) },
        { "CheckURLS", typeof(CheckURLS) } };

        private static readonly Dictionary<String, Type> _DataContextTypes = new Dictionary<string, Type>() {
            {"CheckURLS",
                typeof(CheckUrlsModel)
            }};

        public static readonly DependencyProperty _CurrentControl = DependencyProperty.Register("CurrentControl", typeof(string), typeof(MainControl), new PropertyMetadata(string.Empty, OnCurrentControlChanged));

        public MainControl()
        {
            InitializeComponent();
            _Cache = new Dictionary<string, UserControl>();
            _BaseCache = new Dictionary<string, ModelBase>();
            _Cache.Add("AddCopy", this.CopyPage);
        }

        /// <summary>
        /// Action to perform on changing the string
        /// </summary>
        /// <param name="dependencyObject"></param>
        /// <param name="e"></param>
        private static void OnCurrentControlChanged(DependencyObject dependencyObject,
                DependencyPropertyChangedEventArgs e)
        {
            MainControl _controller = dependencyObject as MainControl;
            if (e.NewValue != null)
            {
                _controller.MainGrid.Children.Clear();//Clear the grid

                //Retrieve the type of UserControl to fill the old window
                Type _userType;

                if (_UserControlTypes.TryGetValue(e.NewValue.ToString(), out _userType))
                {
                    //Fill the window with the new control either from cache or not
                    UserControlCache.AddToCache(e.NewValue.ToString(), _userType, ref _controller._Cache);
                }

                if (_DataContextTypes.TryGetValue(e.NewValue.ToString(), out _userType))
                {
                    UserControlCache.AddToCache(e.NewValue.ToString(), _userType, ref _controller._BaseCache);
                    _controller._Cache[e.NewValue.ToString()].DataContext = _controller._BaseCache[e.NewValue.ToString()];
                }

                _controller.MainGrid.Children.Add(_controller._Cache[e.NewValue.ToString()]);
            }
        }

        public String CurrentControl
        {
            get { return GetValue(_CurrentControl).ToString(); }
            set
            {
                SetValueDP(_CurrentControl, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetValueDP(DependencyProperty property, object value, [System.Runtime.CompilerServices.CallerMemberName] String p = null)
        {
            SetValue(property, value);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
    }

    public class UserControlCache
    {
        /// <summary>
        /// Either add a new page to cache or pull up the version which was already cached
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        public static void AddToCache<T, Type>(String name, ref Dictionary<String, T> cache) where T : UserControl
        {
            T _control;

            if (!cache.TryGetValue(name, out _control))
            {
                _control = (T)Activator.CreateInstance(typeof(Type), new object[] { });
                cache.Add(name, _control);
            }
        }

        /// <summary>
        /// Either add a new page to the cache or pull up the version which was already cached
        /// </summary>
        /// <param name="name"></param>
        /// <param name="_userType"></param>
        /// <param name="cache"></param>
        public static void AddToCache<T>(String name, Type _userType, ref Dictionary<String, T> cache)
        {
            T _control;

            if (!cache.TryGetValue(name, out _control))
            {
                _control = (T)Activator.CreateInstance(_userType, new object[] { });
                cache.Add(name, _control);
            }
        }
    }
}