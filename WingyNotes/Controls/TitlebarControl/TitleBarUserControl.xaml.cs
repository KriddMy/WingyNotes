using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WingyNotes.TitlebarControl;

namespace WingyNotes
{
    /// <summary>
    /// Логика взаимодействия для TitleBarButton.xaml
    /// </summary>
    public partial class CustomTitleBar : UserControl
    {
        private Window _parentWindow;
        private IntPtr _hwnd;
        private TitlebarHookProvider _hookProvider;
        Cursor _userCursor;

        private Brush MouseOverOverlayBackgroundBrush = Brushes.Ivory;
        private Brush PressedOverlayBackgroundBrush = Brushes.Beige;
        private Brush MouseOverWindowCloseButtonBackgroundBrush = Brushes.IndianRed;
        private Brush MouseOverWindowCloseButtonForegroundBrush = Brushes.Black;
        private Brush PressedWindowCloseButtonBackgroundBrush = Brushes.PaleVioletRed;

        #region Properties

        public const int TitlebarButtonWidth = 46;
        public const int TitlebarButtonHeight = 32;

        #endregion

        public CustomTitleBar()
        {
            InitializeComponent();
            _userCursor = Mouse.OverrideCursor;
        }

        private void UpdateMaximizeRestore()
        {
            if (_parentWindow.WindowState == WindowState.Maximized)
            {
                maximizeButton.Visibility = Visibility.Collapsed;
                restoreButton.Visibility = Visibility.Visible;
            }
            else
            {
                maximizeButton.Visibility = Visibility.Visible;
                restoreButton.Visibility = Visibility.Collapsed;
            }

            NativeWinApi.SetWindowPos(_hwnd, 0, 0, 0, 0, 0,
                NativeWinApi.SWP_NOMOVE | NativeWinApi.SWP_NOSIZE | NativeWinApi.SWP_NOZORDER | NativeWinApi.SWP_FRAMECHANGED);
        }

        private void CustomTitleBar_Loaded(object sender, EventArgs e)
        {
            _parentWindow = Window.GetWindow(this);
            if(_parentWindow == null)
            {
                throw new InvalidOperationException();
            }

            _hookProvider = new TitlebarHookProvider(_parentWindow, this);

            _hwnd = new WindowInteropHelper(_parentWindow).Handle;
            ((HwndSource)PresentationSource.FromVisual(_parentWindow)).AddHook(_hookProvider.HookProc);

            _parentWindow.StateChanged += _parentWindow_StateChanged;
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            UpdateMaximizeRestore();
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(SystemParameters.WorkArea))
            {
                this.Dispatcher.Invoke(() =>
               {
                   if(_parentWindow.WindowState == WindowState.Maximized)
                   {
                       _parentWindow.WindowState = WindowState.Normal;
                       _parentWindow.WindowState = WindowState.Maximized;
                   }
                   
               });
            }
        }

        private void _parentWindow_StateChanged(object sender, EventArgs e)
        {
            UpdateMaximizeRestore();
        }

        private void maximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if(_parentWindow.WindowState == WindowState.Maximized)
            {
                _parentWindow.WindowState = WindowState.Normal;
            }
            else if (_parentWindow.WindowState == WindowState.Normal)
            {
                _parentWindow.WindowState = WindowState.Maximized;
            }
        }

        private void minimizwButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.WindowState = WindowState.Minimized;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.Close();
        }

        private void TitlebarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                NativeWinApi.SendMessage(_hwnd, NativeWinApi.WM_NCLBUTTONDBLCLK, (IntPtr)NativeWinApi.HTCAPTION, IntPtr.Zero);
                return;
            }
            NativeWinApi.SendMessage(_hwnd, NativeWinApi.WM_NCLBUTTONDOWN, (IntPtr)NativeWinApi.HTCAPTION, IntPtr.Zero);
        }

        private void topResizeAreaGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeNS;
        }

        private void topResizeAreaGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = _userCursor;
        }

        private void topResizeAreaGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NativeWinApi.SendMessage(_hwnd, NativeWinApi.WM_NCLBUTTONDOWN, (IntPtr)NativeWinApi.HTTOP, IntPtr.Zero);
        }
    }
}
