using CodePaste.Base_Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public bool IsEqual(String value)
        {
            if (!this.VisableType.Equals("string"))
            {
                //Current object isn't a string, so they can't be compared
                return false;
            }

            return this.StringValue.Equals(value);
        }

        public bool IsEqual(ImageSource image)
        {
            if (!this.VisableType.Equals("image"))
            {
                //Not an image, so return it
                return false;
            }

            //Convert to a BitmapSource
            //BitmapImage _image1 = (this.ImageValue as BitmapSource).ToBitmapImage();
            // BitmapImage _image2 = (image as BitmapSource).ToBitmapImage();
            //bool _temp = _image1.IsEqual(_image2);
            return false;
        }
    }

    /// <summary>
    /// Interaction logic for ClipboardData.xaml
    /// </summary>
    public partial class ClipboardData : UserControl
    {
        private Dictionary<String, UserControl> _Cache;
        private static readonly Dictionary<String, Type> _UserControlDic = new Dictionary<string, Type>() { { "string", typeof(ClipboardText) }, { "image", typeof(ClipboardImage) } };

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

        /// <summary>
        /// Change the displayed type depending on if it is an image or string
        /// </summary>
        /// <param name="container"></param>
        private void ChangeDisplayType(ClipboardDataContainer container)
        {
            if (container.VisableType != null)
            {
                Type _userType;
                _UserControlDic.TryGetValue(container.VisableType, out _userType);
                if (_userType != null)
                {
                    UserControlCache.AddToCache(container.VisableType, _userType, ref this._Cache);
                }
                this._Docker.Children.Clear();
                this._Docker.Children.Add(this._Cache[container.VisableType]);
            }
        }
    }

    /// <summary>
    /// Convert images to byte arrays
    /// Can currently convert BitmapImages
    /// </summary>
    public static class BitmapImageExtender
    {
        public static bool IsEqual(this BitmapImage image1, BitmapImage image2)
        {
            return image1.ToBytes().SequenceEqual(image2.ToBytes());
        }

        public static byte[] ToBytes(this BitmapImage image)
        {
            byte[] data = new byte[] { };
            if (image != null)
            {
                try
                {
                    var encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        data = ms.ToArray();
                    }
                    return data;
                }
                catch (Exception ex)
                {
                }
            }
            return data;
        }

        public static bool IsEqual(this Bitmap image1, Bitmap image2)
        {
            MemoryStream _ms = new MemoryStream();

            image1.Save(_ms, System.Drawing.Imaging.ImageFormat.Png);

            String _firstBitmap = Convert.ToBase64String(_ms.ToArray());

            _ms.Position = 0;

            image2.Save(_ms, System.Drawing.Imaging.ImageFormat.Png);
            String _secondBitmap = Convert.ToBase64String(_ms.ToArray());

            return _firstBitmap.Equals(_secondBitmap);
        }

        public static BitmapImage ToBitmapImage(this BitmapSource source)
        {
            JpegBitmapEncoder _encoder = new JpegBitmapEncoder();
            MemoryStream _memoryStream = new MemoryStream();
            BitmapImage _bImg = new BitmapImage();

            _encoder.Frames.Add(BitmapFrame.Create(source));
            _encoder.Save(_memoryStream);

            _memoryStream.Position = 0;
            _bImg.BeginInit();
            _bImg.StreamSource = _memoryStream;
            _bImg.EndInit();

            _memoryStream.Close();

            return _bImg;
        }

        public static Bitmap ToBitmap(this BitmapSource source)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(source));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }
    }
}