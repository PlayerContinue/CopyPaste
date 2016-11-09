using CodePaste.Base_Classes;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.InteropServices;       //Microsoft Excel 14 object in references-> COM tab
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace CodePaste.User_Controls
{
    /// <summary>
    /// Interaction logic for CheckURLS.xaml
    /// </summary>
    public partial class CheckURLS : UserControl
    {
      

        public CheckURLS()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Select an excel document for updating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSelect(object sender, RoutedEventArgs e)
        {
            CheckUrlsModel _context = this.DataContext as CheckUrlsModel;

            if (_context != null)
            {
                _context.FileName = DocumentSelector.SingleSelectDocument("Excel Files|*.xls;*.xlsx;*.xlsm");
            }
        }

        /// <summary>
        /// Run the url check and save to excel file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="listOfValues"></param>

        private void LaunchExcelChecker(string filename, int[] listOfValues)
        {
            //Create a new thread to prevent lockup on the visual thread
            //Replace this with a Threadpool in the future for better setup
            //Perhaps change delegate type as well
            Thread _ExcelThread = new Thread(new ParameterizedThreadStart(delegate(object obs)
            {
                int[] _toFromOutputArray = obs as int[];//[0] = url column to send from, [1] = desired url destination, [2] = output column
                ExcelDocument _exDoc = null; //The Excel Document
                try
                {
                    _exDoc = new ExcelDocument(filename);//Open new document for reading
                    UrlChecker.CheckURLS(_exDoc, _toFromOutputArray[0], _toFromOutputArray[1], _toFromOutputArray[2]);
                    _exDoc.SaveChange();
                    _exDoc.CleanExcelDocument();
                    MessageBox.Show(filename, "Finshed");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Issue has occured" + ex.Message, "Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (_exDoc != null)
                    {
                        _exDoc.CleanExcelDocument();
                    }
                }
            }));
            _ExcelThread.Start(listOfValues);
        }

        private void CheckUrls(object sender, RoutedEventArgs e)
        {
            CheckUrlsModel _context = this.DataContext as CheckUrlsModel;
            Int32[] _fromToOutput = new Int32[(int)_context.FromToOutputColumn.Length];
            StringBuilder _errorMessage = new StringBuilder();
            bool _failure = false;
            //Confirm that the file is set
            if (String.IsNullOrWhiteSpace(_context.FileName))
            {
                _errorMessage.Append("No file selected, please select a file.");
                _errorMessage.AppendLine();
                _failure = true;
            }

            //Confirm that each of the values is a number and that it is greater than 0. 
            //Excel runs using columns as numbers, starting at 1 rather than 0
            for (int i = 0; i < _fromToOutput.Length; i++)
            {
                if (!Int32.TryParse(_context.FromToOutputColumn[i], out _fromToOutput[i]) || _fromToOutput[i] <1)
                {
                    _errorMessage.Append(String.Format("Please fill out {0} with value greater than 1\n", CheckUrlsModel.ColumnPosition[i]));
                    _context.FromToOutputColumnColor[i] = "Red";

                    _failure = true;
                }
                else
                {
                    _context.FromToOutputColumnColor[i] = "White";
                }
            }

            if (_failure)
            {
                MessageBox.Show(_errorMessage.ToString(), "Issue In Form", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_context != null)
            {
                LaunchExcelChecker(_context.FileName, _fromToOutput);
            }
        }
    }

    /// <summary>
    /// Excel Application Wrapper
    /// </summary>
    public class ExcelDocument
    {
        public Excel.Application xlApp;
        public Excel.Workbook xlWorkbook;
        public Excel.Worksheet xlWorksheet;
        public Excel.Range xlRange;

        public ExcelDocument(string path)
        {
            this.xlApp = new Excel.Application();
            this.xlWorkbook = xlApp.Workbooks.Open(@path);
            this.xlWorksheet = xlWorkbook.Sheets[1];
            this.xlRange = xlWorksheet.UsedRange;
        }

        public void SaveChange()
        {
            xlWorkbook.Save();
        }

        //Release an object if it is not null
        private void ReleaseObject(object excelObject){
            if (excelObject != null)
            {
                Marshal.ReleaseComObject(excelObject);
            }
        }

        public void CleanExcelDocument()
        {
            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            ReleaseObject(xlRange);
            ReleaseObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            ReleaseObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            ReleaseObject(xlApp);
        }
    }

    public static class UrlChecker
    {
        public static void CheckURLS(ExcelDocument excel, int row1, int row2, int outputColumn)
        {
            String _url;
            String _finalURL;
            Regex _urlRegex = new Regex(UsefulRegex.url, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            String _failToReturn = "fail";
            Match _urlMatch;
            String _outputValue = "";
            System.Drawing.Color _outputColorValue = System.Drawing.Color.Yellow;
            ResponseInformation _information = new ResponseInformation();

            for (int i = 1; i <= excel.xlRange.Rows.Count; i++)
            {
                _outputValue = "";
                //Confirm both cells contain values
                if (excel.xlRange.Cells[i, row1] != null && excel.xlRange.Cells[i, row1].Value2 != null && excel.xlRange.Cells[i, row2].Value2 != null)
                {
                    _url = excel.xlRange.Cells[i, row1].Value2.ToString().Trim();
                    _urlMatch = _urlRegex.Match(_url);
                    if (_urlMatch.Success)
                    {
                        if (!_url.Contains("http"))//Check that the url contains http
                        {
                            _url = "http://" + _url;
                        }
                        try
                        {
                            _finalURL = excel.xlRange.Cells[i, row2].Value2.ToString();

                            for (int j = 0; j < 2; j++)//Attempt twice. First time for single redirect, Second for multiple
                            {
                                TestURL(_url, ref _information, Convert.ToBoolean(j));
                                if (_information.RedirectUrl.Contains(_finalURL))
                                {
                                    if (j == 1)
                                    {
                                        _outputValue = "True, after multiple redirects";
                                    }
                                    else
                                    {
                                        _outputValue = "True";
                                    }

                                    _outputColorValue = System.Drawing.Color.Green;
                                    j = 2;
                                }
                                else
                                {
                                    if (j == 0)
                                    {
                                        _failToReturn = _information.ToString();
                                    }

                                    _outputValue = "False: " + _failToReturn;
                                    _outputColorValue = System.Drawing.Color.Red;
                                }
                            }
                        }
                        catch
                        {
                            _outputValue = "Issue Occured On Call";
                            _outputColorValue = System.Drawing.Color.Orange;
                        }
                    }
                    else
                    {
                        _outputValue = "Not a url";
                        _outputColorValue = System.Drawing.Color.Blue;
                    }
                }

                //Set the Cell Value
                excel.xlRange.Cells[i, outputColumn] = _outputValue;
                ((Excel.Range)excel.xlRange.Cells[i, outputColumn]).Font.Color = System.Drawing.ColorTranslator.ToOle(_outputColorValue);
            }
        }

        private static void TestURL(String url, ref ResponseInformation responseInformation, bool allowRedirect = false)
        {
            HttpWebResponse _response;

            responseInformation.URL = url;

            try
            {
                //Create the WebRequest
                HttpWebRequest _webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                _webRequest.Method = "GET";
                _webRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36";
                //Don't redirect
                _webRequest.AllowAutoRedirect = allowRedirect;
                _response = (HttpWebResponse)_webRequest.GetResponse();
                responseInformation.ResponseCode = (int)_response.StatusCode;

                if (responseInformation.ResponseCode == 301 || responseInformation.ResponseCode == 302)
                {
                    responseInformation.RedirectUrl = _response.Headers["Location"];
                }
                else if (responseInformation.ResponseCode == 200)//Response was successful after redirect
                {
                    responseInformation.RedirectUrl = _response.ResponseUri.AbsoluteUri.ToString();
                }
                _response.Dispose();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    _response = (HttpWebResponse)ex.Response;
                    responseInformation.ResponseCode = (int)_response.StatusCode;
                    _response.Dispose();
                }
                else
                {
                    responseInformation.ResponseCode = -1;
                }
                //Failure, so no redirect occurs
                responseInformation.RedirectUrl = "";
            }
        }
    }

    /// <summary>
    /// Container for new response information
    /// </summary>
    public class ResponseInformation
    {
        public String RedirectUrl;
        public int ResponseCode;
        public String URL;

        public override string ToString()
        {
            return String.Format("RedirectURL:{0},ResponseCode:{1}", RedirectUrl, ResponseCode);
        }
    }

    /// <summary>
    /// Basic WebClient which stores useful values for later persual
    /// </summary>
    internal class ResponseWebClient : WebClient
    {
        private Uri _responseUri;

        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            _responseUri = response.ResponseUri;
            return response;
        }
    }

    /// <summary>
    /// Storage for various Regex Strings
    /// </summary>
    public static class UsefulRegex
    {
        //public static String url = @"\b(?:https?://|www\.)\S+\b";
        public static String url = @"\b^(((http|ftp|https):\/{2})?(([0-9a-z_-]+\.)+(aero|asia|biz|cat|com|coop|edu|gov|info|int|jobs|mil|mobi|museum|name|net|org|pro|tel|travel|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cu|cv|cx|cy|cz|cz|de|dj|dk|dm|do|dz|ec|ee|eg|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mn|mn|mo|mp|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|nom|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ra|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|sj|sk|sl|sm|sn|so|sr|st|su|sv|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|yu|za|zm|zw|arpa)(:[0-9]+)?((\/([~0-9a-zA-Z\#\+\%@\.\/_-]+))?(\?[0-9a-zA-Z\+\%@\/&\[\];=_-]+)?)?))$\b";
        public static String numericOnly = "[^0-9.-]+";

        public static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex(numericOnly); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }
    }

    public class CheckUrlsModel : ModelBase
    {
        private string _FileName;

        private string[] _FromToOuputColumn = new string[3];

        private ObservableCollection<String> _FromToOutputColumnColor = new ObservableCollection<String> { "white", "white", "white" };

        public static readonly string[] ColumnPosition = new string[] { "From Column", "To Column", "Output Column" };

        public string[] FromToOutputColumn
        {
            get { return _FromToOuputColumn; }
        }

        /// <summary>
        /// Set the color value of the model by position
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        public void SetColor(String color, int pos)
        {
            if (pos < this._FromToOutputColumnColor.Count)
            {
                _FromToOuputColumn[pos] = color;
            }
            base.OnPropertyChanged("FromOutputColumnColor");
        }

        public ObservableCollection<String> FromToOutputColumnColor
        {
            get
            {
                return _FromToOutputColumnColor;
            }
        }

        public string FromColumn
        {
            get { return _FromToOuputColumn[0]; }
            set
            {
                if (_FromToOuputColumn[0] == value) return;
                _FromToOuputColumn[0] = value;
                base.OnPropertyChanged("FromColumn");
            }
        }

        public string ToColumn
        {
            get { return _FromToOuputColumn[1]; }
            set
            {
                if (_FromToOuputColumn[1] == value) return;
                _FromToOuputColumn[1] = value;
                base.OnPropertyChanged("ToColumn");
            }
        }

        public string OutputColumn
        {
            get { return _FromToOuputColumn[2]; }
            set
            {
                if (_FromToOuputColumn[2] == value) return;
                _FromToOuputColumn[2] = value;
                base.OnPropertyChanged("OutputColumn");
            }
        }

        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (_FileName == value) return;
                _FileName = value;
                base.OnPropertyChanged("FileName");
            }
        }
    }

    public class DocumentSelector
    {
        /// <summary>
        /// Create a document selector which allows for the selection of one file
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string SingleSelectDocument(String filter = "")
        {
            // Create an instance of the open file dialog box.
            OpenFileDialog _openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            _openFileDialog1.Filter = filter;
            _openFileDialog1.FilterIndex = 1;

            _openFileDialog1.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = _openFileDialog1.ShowDialog();

            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                return _openFileDialog1.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}