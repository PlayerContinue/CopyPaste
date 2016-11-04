using CodePaste.Base_Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodePaste.User_Controls
{
    public class CaptureClipboard : ModelBase
    {
        private IntPtr _ClipboardViewerNext;
        private IntPtr _Handle;//Stores the handle for the current window
        private HwndSource _HwndSource; //The source of the hwnd
        private Window _MainWindow;
        private int _MaxNumberSaved;
        private List<ClipboardDataContainer> _ClipBoardList; //Contains previous list of clipboard values

        public List<ClipboardDataContainer> ClipBoardList { get { return _ClipBoardList; } set { _ClipBoardList = value; } }

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool
               ChangeClipboardChain(IntPtr hWndRemove,
                                    IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg,
                                             IntPtr wParam,
                                             IntPtr lParam);

        public CaptureClipboard(Window window, int max_number_saved)
        {
            _MainWindow = window;
            _MaxNumberSaved = max_number_saved;
            RegisterHandle();
            RegisterClipboardViewer();
        }

        private void RegisterHandle()
        {
            _Handle = (new WindowInteropHelper(_MainWindow)).EnsureHandle();
            _HwndSource = HwndSource.FromHwnd((IntPtr)_Handle);
            _HwndSource.AddHook(new HwndSourceHook(WndProc));
            _ClipBoardList = new List<ClipboardDataContainer>();
        }

        /// <summary>
        /// Register this form as a Clipboard Viewer application
        /// </summary>
        private void RegisterClipboardViewer()
        {
            _ClipboardViewerNext = (IntPtr)SetClipboardViewer(_Handle);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;

            switch (msg)
            {
                case WM_CHANGECBCHAIN:
                    if (wParam == _ClipboardViewerNext)
                    {
                        // clipboard viewer chain changed, need to fix it.
                        _ClipboardViewerNext = lParam;
                    }
                    else if (_ClipboardViewerNext != IntPtr.Zero)
                    {
                        // pass the message to the next viewer.
                        SendMessage(_ClipboardViewerNext, msg, wParam, lParam);
                    }
                    break;

                case WM_DRAWCLIPBOARD:
                    // clipboard content changed
                    SaveClipboardData();
                    // pass the message to the next viewer.
                    SendMessage(_ClipboardViewerNext, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        //Dispose of the data
        private void Dispose(bool disposing)
        {
            ChangeClipboardChain(_Handle, _ClipboardViewerNext);
        }

        /// <summary>
        /// On Clipboard update, update the current list of items such that it contains the current list of clipboard values
        /// </summary>
        /// <param name="m"></param>
        private void SaveClipboardData()
        {
            if (!WindowInformation.ApplicationIsActivated())
            {
                if (Clipboard.ContainsText())
                {
                    // we have some text in the clipboard.
                    UpdateClipboardList(Clipboard.GetText());
                }
                else if (Clipboard.ContainsFileDropList())
                {
                    // we have a file drop list in the clipboard
                }
                else if (Clipboard.ContainsImage())
                {
                    // Because of a known issue in WPF,
                    // we have to use a workaround to get correct
                    // image that can be displayed.
                    // The image have to be saved to a stream and then
                    // read out to workaround the issue.
                    MemoryStream ms = new MemoryStream();
                    BmpBitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(Clipboard.GetImage()));
                    enc.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    BmpBitmapDecoder dec = new BmpBitmapDecoder(ms,
                        BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    UpdateClipboardList(dec.Frames[0]);
                }
                else
                {
                    Label lb = new Label();
                    lb.Content = "The type of the data in the clipboard is not supported by this sample.";
                }
            }
        }

        /// <summary>
        /// Update the list of previous clipboard values with new values
        /// </summary>
        private void UpdateClipboardListInner()
        {
            if (_ClipBoardList.Count >= this._MaxNumberSaved)//If the list is full, remove last object
            {
                //Change position of last to first in order to save memory

                ClipboardDataContainer _mem = _ClipBoardList.Last();
                _ClipBoardList.RemoveAt(this._MaxNumberSaved - 1);
                _ClipBoardList.Insert(0, _mem);
            }
            else
            {
                _ClipBoardList.Insert(0, new ClipboardDataContainer());
            }
        }

        private void UpdateClipboardList(ImageSource value)
        {
            if (_ClipBoardList.Count == 0 || !_ClipBoardList[0].IsEqual(value))
            {
                UpdateClipboardListInner();
                _ClipBoardList[0].UpdateValue(value);
                OnPropertyChanged("ClipBoardList");
            }
        }

        private void UpdateClipboardList(String value)
        {
            //Required in order to block more than single copy from showing up in the list
            if (_ClipBoardList.Count == 0 || !_ClipBoardList[0].IsEqual(value))
            {
                UpdateClipboardListInner();
                _ClipBoardList[0].UpdateValue(value);
                OnPropertyChanged("ClipBoardList");
            }
        }
    }

    public static class WindowInformation
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }
    }

    /// <summary>
    /// Interaction logic for ClipboardCapture.xaml
    /// </summary>
    public partial class ClipboardCapture : UserControl
    {
        public ClipboardCapture()
        {
            InitializeComponent();
        }
    }
}