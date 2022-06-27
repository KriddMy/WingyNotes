using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WingyNotes.TitlebarControl
{
    class TitlebarHookProvider
    {
        public TitlebarHookProvider(Window sourceWindow, CustomTitleBar customTitleBar)
        {
            _rootWindow = sourceWindow;
            _customTitlebarControl = customTitleBar;
            _hwnd = new WindowInteropHelper(_rootWindow).EnsureHandle();
            _buttonHittester = new CustomTitlebarButtonHittester(_rootWindow, _customTitlebarControl);
            CalcWindMetrics();
            
        }

        private void CalcWindMetrics()
        {
            NativeWinApi.RECT clientArea, windowRect;
            // Random value to see the differance
            // windowSize - clientSize = NCsize
            clientArea = new NativeWinApi.RECT 
            {
                Bottom = 450,
                Right = 450,
                Left = 0,
                Top = 0
            };
            windowRect = clientArea;

            
            int winStyle = NativeWinApi.GetWindowLong(_hwnd, NativeWinApi.GWL_STYLE);
            int winExStile = NativeWinApi.GetWindowLong(_hwnd, NativeWinApi.GWL_EXSTYLE);
            NativeWinApi.AdjustWindowRectEx(ref windowRect, (uint)winStyle, false, (uint)winExStile);

            // calculations of outer resize borders sizes
            _topResizeBarHeight = clientArea.Top - windowRect.Top/* - (int)SystemParameters.BorderWidth*/;
            _leftResizeBarWidth = clientArea.Left - windowRect.Left - (int)SystemParameters.BorderWidth;
            _bottomResizeBarHeight = windowRect.Bottom - clientArea.Bottom - (int)SystemParameters.BorderWidth;
            _rightResizeBarWidth = windowRect.Right - clientArea.Right - (int)SystemParameters.BorderWidth;
        }

        private void getMinMaxInfoProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            NativeWinApi.MINMAXINFO mmi = (NativeWinApi.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(NativeWinApi.MINMAXINFO));
            IntPtr monitor = NativeWinApi.MonitorFromWindow(hwnd, NativeWinApi.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                NativeWinApi.MONITORINFO monitorInfo = new NativeWinApi.MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(NativeWinApi.MONITORINFO));
                NativeWinApi.GetMonitorInfo(monitor, ref monitorInfo);
                NativeWinApi.RECT rcWorkArea = monitorInfo.rcWork;
                NativeWinApi.RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.X = rcWorkArea.Left - rcMonitorArea.Left - _leftResizeBarWidth - (int)SystemParameters.BorderWidth;
                mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top) - (int)SystemParameters.BorderWidth;
                mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left) + _leftResizeBarWidth + _rightResizeBarWidth + 2 * (int)SystemParameters.BorderWidth;
                mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top) + _bottomResizeBarHeight + 2 * (int)SystemParameters.BorderWidth;
                mmi.ptMinTrackSize = new NativeWinApi.POINT((int)_rootWindow.MinWidth, (int)_rootWindow.MinHeight);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
            handled = true;
        }

        private void ncActivateProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            NativeWinApi.SendMessage(hwnd, NativeWinApi.WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
            NativeWinApi.DefWindowProc(hwnd, msg, wParam, lParam);
            _customTitlebarControl.TitlebarGrid.InvalidateVisual();
            NativeWinApi.SendMessage(hwnd, NativeWinApi.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            handled = true;
        }

        private void ncCalcSizeProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (wParam == (IntPtr)1)
            {
                var csp = (NativeWinApi.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NativeWinApi.NCCALCSIZE_PARAMS));
                csp.rgrc[0].Top -= this._topResizeBarHeight;
                _rootWindow.Dispatcher.Invoke(() =>
                {
                    var userControl = _rootWindow.FindName("TextInputField") as RedactorControl;
                    var richtext = userControl.RichTextbox1;
                    if(richtext == null)
                    {
                        return;
                    }
                    richtext.Document.Blocks.Clear();
                    richtext.Document.Blocks.Add(new Paragraph(new Run(_topResizeBarHeight.ToString())));
                });
                if (_rootWindow.WindowState == WindowState.Maximized)
                    csp.rgrc[0].Top += 8;
                Marshal.StructureToPtr(csp, lParam, false);
            }
        }

        private IntPtr ncHitTestProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            int x = lParam.ToInt32() & 0xffff;
            int y = lParam.ToInt32() >> 16;
            WingyNotesControlButtons result = _buttonHittester.HittestButton(x, y);
            if(result == WingyNotesControlButtons.maximize ||
                result == WingyNotesControlButtons.restore)
            {
                handled = true;
                return (IntPtr)NativeWinApi.HTMAXBUTTON;
            }
            else if(result == WingyNotesControlButtons.close)
            {
                handled = true;
                return (IntPtr)NativeWinApi.HTCLOSE;
            }
            else if (result == WingyNotesControlButtons.minimize)
            {
                handled = true;
                return (IntPtr)NativeWinApi.HTMINBUTTON;
            }

            return (IntPtr)NativeWinApi.HTCLIENT;
        }

        private void ncLButtonDownProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if(wParam == (IntPtr)NativeWinApi.HTMAXBUTTON ||
                wParam == (IntPtr)NativeWinApi.HTMINBUTTON ||
                wParam == (IntPtr)NativeWinApi.HTCLOSE)
            {
                handled = true;
                _buttonHittester.OnHittestedButtonLMouseDown();
            }
            
        }

        private void ncMouseMoveProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _buttonHittester.OnHittestedButtonMouseMove();
        }

        public IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeWinApi.WM_NCCALCSIZE:
                    ncCalcSizeProc(hwnd, msg, wParam, lParam, ref handled);
                    break;

                case NativeWinApi.WM_NCHITTEST:
                    return ncHitTestProc(hwnd, msg, wParam, lParam, ref handled);

                case NativeWinApi.WM_NCLBUTTONDOWN:
                    ncLButtonDownProc(hwnd, msg, wParam, lParam, ref handled);
                    break;

                case NativeWinApi.WM_NCMOUSEMOVE:
                    ncMouseMoveProc(hwnd, msg, wParam, lParam, ref handled);
                    break;

                case NativeWinApi.WM_NCMOUSELEAVE:
                    ncMouseLeaveProc(hwnd, msg, wParam, lParam, ref handled);
                    break;

                case NativeWinApi.WM_GETMINMAXINFO:
                    getMinMaxInfoProc(hwnd, msg, wParam, lParam, ref handled);
                    break;

                case NativeWinApi.WM_NCACTIVATE:
                    ncActivateProc(hwnd, msg, wParam, lParam, ref handled);
                    break;

                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void ncMouseLeaveProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            _buttonHittester.OnHittestedMouseLeave();
        }

        private int _topResizeBarHeight;
        private int _leftResizeBarWidth;
        private int _bottomResizeBarHeight;
        private int _rightResizeBarWidth;

        private Window _rootWindow;
        private IntPtr _hwnd;

        private CustomTitleBar _customTitlebarControl;
        private CustomTitlebarButtonHittester _buttonHittester;

    }
}
