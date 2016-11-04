using CodePaste.Base_Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;       //Microsoft Excel 14 object in references-> COM tab
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using Excel = Microsoft.Office.Interop.Excel;
namespace CodePaste.User_Controls
{
    /// <summary>
    /// Interaction logic for CheckURLS.xaml
    /// </summary>
    public partial class CheckURLS : UserControl
    {
        private ExcelDocument _ExDoc;//Stored excel document

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



        private void CheckUrls(object sender, RoutedEventArgs e)
        {
            CheckUrlsModel _context = this.DataContext as CheckUrlsModel;
            Int32[] _fromToOutput = new Int32[(int)_context.FromToOutputColumn.Length];
         
            //Confirm that the file is set
            if (String.IsNullOrWhiteSpace(_context.FileName))
            {
                MessageBox.Show("No file selected, please select a file.", "Select File", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            
           /* if (!Int32.TryParse(_context.FromColumn, out _fromToOutput[0]) || !Int32.TryParse(_context.ToColumn, out _fromToOutput[1]) || !Int32.TryParse(_context.OutputColumn, out _fromToOutput[2]))
            {

                
            }*/
            //Confirm that each of the values is a number
            for (int i = 0; i < _fromToOutput.Length; i++)
            {
                if(!Int32.TryParse(_context.FromToOutputColumn[i], out _fromToOutput[i])){
                    MessageBox.Show("Please Fill Out All Three Value With Numbers Greater Than 1", "One of the Rows was left empty", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }


                if (_context != null)
                {
                    //Create a new thread to prevent lockup on the visual thread
                    //Replace this with a Threadpool in the future for better setup
                    //Perhaps change delegate type as well
                    Thread _ExcelThread = new Thread(new ParameterizedThreadStart(delegate(object obs)
                    {

                        int[] _toFromOutputArray = obs as int[];//[0] = url column to send from, [1] = desired url destination, [2] = output column
                        try
                        {

                            this._ExDoc = new ExcelDocument(_context.FileName);//Open new document for reading
                            UrlChecker.CheckURLS(_ExDoc, _toFromOutputArray[0], _toFromOutputArray[1], _toFromOutputArray[2]);
                            _ExDoc.SaveChange();
                            this._ExDoc.CleanExcelDocument();
                            MessageBox.Show("Finished", "Complete");

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Issue has occured", "Issue");
                            _ExDoc.CleanExcelDocument();
                        }
                    }));
                    _ExcelThread.Start(_fromToOutput);

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

        public ExcelDocument()
        {

        }

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

        public void CleanExcelDocument()
        {
            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
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
            using (ResponseWebClient _client = new ResponseWebClient())
            {
                _client.Headers["User-Agent"] =
        "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
        "(compatible; MSIE 6.0; Windows NT 5.1; " +
        ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                for (int i = 1; i <= excel.xlRange.Rows.Count; i++)
                {
                    //Confirm both cells contain values
                    if (excel.xlRange.Cells[i, row1] != null && excel.xlRange.Cells[i, row1].Value2 != null && excel.xlRange.Cells[i, row2].Value2 != null)
                    {
                        _url = excel.xlRange.Cells[i, row1].Value2.ToString();
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
    class ResponseWebClient : WebClient
    {
        Uri _responseUri;

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

        public static String url = @"\b(?:https?://|www\.)\S+\b";
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

        public string[] FromToOutputColumn
        {
            get { return _FromToOuputColumn; }
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
