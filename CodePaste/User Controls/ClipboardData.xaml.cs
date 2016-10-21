using CodePaste.Base_Classes;
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

namespace CodePaste.User_Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ClipboardDataContainer : ModelBase
    {
        private String _StringValue;
        private ImageSource _ImageValue;

        public ImageSource ImageValue { get { return _ImageValue; } set { _ImageValue = value; } }
        public String StringValue { get { return _StringValue; } set { _StringValue = value; } }
        public Visibility IsImage { get { return (_ImageValue == null) ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility IsString { get { return (_StringValue == null) ? Visibility.Collapsed : Visibility.Visible; } }
        public String VisableType
        {
            get
            {
                if (_StringValue != null)
                {
                    return "string";
                }
                else if (_ImageValue != null)
                {
                    return "image";
                }
                else
                {
                    return null;
                }


            }
        }

        public ClipboardDataContainer()
        {

            _StringValue = null;
            _ImageValue = null;
            UpdateProperties();
        }

        public ClipboardDataContainer(String value)
        {
            UpdateValue(value);
        }

        public ClipboardDataContainer(ImageSource value)
        {
            UpdateValue(value);
        }

        /// <summary>
        /// Update Stored Value
        /// </summary>
        /// <param name="value"></param>
        public void UpdateValue(String value)
        {
            _StringValue = value;
            _ImageValue = null;
            UpdateProperties();
        }

        /// <summary>
        /// Update Stored Value
        /// </summary>
        /// <param name="value"></param>
        public void UpdateValue(ImageSource value)
        {
            _ImageValue = value;
            _StringValue = null;
            UpdateProperties();
        }

        /// <summary>
        /// Inform system the values have changed
        /// </summary>
        private void UpdateProperties()
        {
            OnPropertyChanged("StringValue");
            OnPropertyChanged("ImageValue");
            OnPropertyChanged("IsImage");
            OnPropertyChanged("IsString");
        }
    }

    /// <summary>
    /// Interaction logic for ClipboardData.xaml
    /// </summary>
    public partial class ClipboardData : UserControl
    {
        private Dictionary<String, UserControl> _Cache;

        public ClipboardData()
        {
            InitializeComponent();
            _Cache = new Dictionary<string, UserControl>();
            _Cache.Add("string", this._ClipText);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext != null)
            {
                ChangeDisplayType(this.DataContext as ClipboardDataContainer);
            }
        }

        private void ChangeDisplayType(ClipboardDataContainer container)
        {
            if (container.VisableType != null)
            {
                switch (container.VisableType)
                {
                    case "string":
                        UserControlCache.AddToCache<ClipboardImage>(container.VisableType, ref this._Cache);
                        break;
                    case "image":
                        UserControlCache.AddToCache<ClipboardImage>(container.VisableType, ref this._Cache);
                        break;
                }
                this._Docker.Children.Clear();
                this._Docker.Children.Add(this._Cache[container.VisableType]);
            }
        }


    }
}
