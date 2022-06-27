using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WingyNotes.TitlebarControl
{
    enum WingyNotesControlButtons : uint
    {
        none = 0,
        close = 1,
        maximize = 2,
        restore = 3,
        minimize = 4
    };

    class CustomTitlebarButtonHittester
    {
        public CustomTitlebarButtonHittester(Window window, CustomTitleBar customTitleBar)
        {
            _customTitleControl = customTitleBar;
            _sourceWindow = window;
            _controlButtons = new List<Button>
            {
                _customTitleControl.closeButton,
                _customTitleControl.maximizeButton,
                _customTitleControl.restoreButton,
                _customTitleControl.minimizwButton
            };
            loadButtonColorsFromResource();
            _customTitleControl.closeButton.LostMouseCapture += controlButton_LostMouseCapture;
            _customTitleControl.maximizeButton.LostMouseCapture += controlButton_LostMouseCapture;
            _customTitleControl.restoreButton.LostMouseCapture += controlButton_LostMouseCapture;
            _customTitleControl.minimizwButton.LostMouseCapture += controlButton_LostMouseCapture;

            _customTitleControl.closeButton.MouseMove += CloseButton_MouseMove;

        }

        private void CloseButton_MouseMove(object sender, MouseEventArgs e)
        {
            if(Mouse.Captured != null)
            {
                return;
            }
            var button = sender as Button;
            var testRect = new Rect(button.PointToScreen(
                new Point()),
                new Size(button.ActualWidth, button.ActualHeight));
            var mousepose = _sourceWindow.PointFromScreen(Mouse.GetPosition(_sourceWindow));
            if (testRect.Contains(mousepose))
            {
                setLMouseDownColors(button);
            }
            else
            {
                ResetButtonColor(button);
            }
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetButtonColor(_controlButtons[(int)_hittestedButton - 1]);
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            setLMouseDownColors(_controlButtons[(int)_hittestedButton - 1]);
        }

        public WingyNotesControlButtons HittestButton(int x, int y)
        {
            WingyNotesControlButtons result = WingyNotesControlButtons.none;

            updateButtonRects();

            if (_maximizeRect.Contains(new Point(x, y)))
            {
                result = WingyNotesControlButtons.maximize;
            }
            else if (_RestoreRect.Contains(new Point(x, y)))
            {
                result = WingyNotesControlButtons.restore;
            }
            else if (_minimizeRect.Contains(new Point(x, y)))
            {
                result = WingyNotesControlButtons.minimize;
            }
            else if (_closeRect.Contains(new Point(x, y)))
            {
                result = WingyNotesControlButtons.close;
            }

            if (result != _hittestedButton
                && Mouse.Captured == null)
            {
                _prevHittestedButton = _hittestedButton;
                _hittestedButton = result;

                if (_prevHittestedButton != WingyNotesControlButtons.none)
                    ResetButtonColor(_controlButtons[(int)_prevHittestedButton - 1]);
                if (_hittestedButton != WingyNotesControlButtons.none)
                    setMouseOverColors(_controlButtons[(int)_hittestedButton - 1]);
            }


            return result;
        }

        public void OnHittestedButtonLMouseDown()
        {
            if (_hittestedButton == WingyNotesControlButtons.none)
            {
                return;
            }
            Button prossesButton = _controlButtons[(int)_hittestedButton - 1];
            MouseButtonEventArgs arg = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
            arg.RoutedEvent = Button.MouseLeftButtonDownEvent;
            prossesButton.CaptureMouse();
            prossesButton.RaiseEvent(arg);

            setLMouseDownColors(prossesButton);
        }

        public void OnHittestedButtonMouseMove()
        {
            /*if (_hittestedButton == WingyNotesControlButtons.none)
            {
                return;
            }
            Button prossesButton = _controlButtons[(int)_hittestedButton - 1];
            MouseEventArgs arg = new MouseEventArgs(Mouse.PrimaryDevice, 0);
            arg.RoutedEvent = Button.MouseMoveEvent;
            prossesButton.RaiseEvent(arg);*/

            /*setLMouseDownColors(prossesButton);

            if (_prevHittestedButton != WingyNotesControlButtons.none)
                ResetButtonColor(_controlButtons[(int)_prevHittestedButton - 1]);*/

        }

        public void OnHittestedMouseLeave()
        {
            if (_hittestedButton != WingyNotesControlButtons.none
                && Mouse.Captured == null)
            {
                ResetButtonColor(_controlButtons[(int)_hittestedButton - 1]);

            }
            _prevHittestedButton = _hittestedButton;
            _hittestedButton = WingyNotesControlButtons.none;
        }

        private void loadButtonColorsFromResource()
        {
            _mouseOverOverlayBackgroundBrush = System.Windows.Application.Current.Resources["MouseOverOverlayBackgroundBrush"] as SolidColorBrush;
            _pressedOverlayBackgroundBrush = System.Windows.Application.Current.Resources["PressedOverlayBackgroundBrush"] as SolidColorBrush;
            _mouseOverWindowCloseButtonBackgroundBrush = System.Windows.Application.Current.Resources["MouseOverWindowCloseButtonBackgroundBrush"] as SolidColorBrush;
            _mouseOverWindowCloseButtonForegroundBrush = System.Windows.Application.Current.Resources["MouseOverWindowCloseButtonForegroundBrush"] as SolidColorBrush;
            _pressedWindowCloseButtonBackgroundBrush = System.Windows.Application.Current.Resources["PressedWindowCloseButtonBackgroundBrush"] as SolidColorBrush;
        }

        private void controlButton_LostMouseCapture(object sender, MouseEventArgs e)
        {
            var button = e.Source as Button;
            ResetButtonColor(button);
        }

        private void setMouseOverColors(Button button)
        {
            if (_hittestedButton == WingyNotesControlButtons.close)
            {
                ColorAnimation coloAnimation;
                coloAnimation = new ColorAnimation();
                coloAnimation.From = Brushes.Transparent.Color;
                coloAnimation.To = _mouseOverWindowCloseButtonBackgroundBrush.Color;
                coloAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
                button.Background = new SolidColorBrush();
                button.Background.BeginAnimation(SolidColorBrush.ColorProperty, coloAnimation);
                button.Foreground = _mouseOverWindowCloseButtonForegroundBrush;
            }
            else
            {
                ColorAnimation colorAnimation;
                colorAnimation = new ColorAnimation();
                colorAnimation.From = Brushes.Transparent.Color;
                colorAnimation.To = _mouseOverOverlayBackgroundBrush.Color;
                colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
                button.Background = new SolidColorBrush();
                button.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                button.Background = _mouseOverOverlayBackgroundBrush;
            }
        }

        private void setLMouseDownColors(Button button)
        {
            if (_hittestedButton == WingyNotesControlButtons.close)
            {
                ColorAnimation colorAnimation;
                colorAnimation = new ColorAnimation();
                colorAnimation.From = _mouseOverWindowCloseButtonBackgroundBrush.Color;
                colorAnimation.To = _pressedWindowCloseButtonBackgroundBrush.Color;
                colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
                button.Background = new SolidColorBrush();
                button.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                button.Foreground = _mouseOverWindowCloseButtonForegroundBrush;
            }
            else
            {
                ColorAnimation colorAnimation;
                colorAnimation = new ColorAnimation();
                colorAnimation.From = _mouseOverOverlayBackgroundBrush.Color;
                colorAnimation.To = _pressedOverlayBackgroundBrush.Color;
                colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
                button.Background = new SolidColorBrush();
                button.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private void ResetButtonColor(Button button)
        {
            ColorAnimation colorAnimation;
            colorAnimation = new ColorAnimation();
            colorAnimation.From = Brushes.Transparent.Color;
            colorAnimation.To = Brushes.Transparent.Color;
            colorAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
            button.Background = new SolidColorBrush();
            button.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }

        private void updateButtonRects()
        {
            if (_sourceWindow.WindowState == WindowState.Normal)
            {
                _maximizeRect = new Rect(_customTitleControl.maximizeButton.PointToScreen(
                new Point()),
                new Size(_customTitleControl.maximizeButton.ActualWidth, _customTitleControl.maximizeButton.ActualHeight));
            }
            else
            {
                _RestoreRect = new Rect(_customTitleControl.restoreButton.PointToScreen(
                new Point()),
                new Size(_customTitleControl.restoreButton.ActualWidth, _customTitleControl.restoreButton.ActualHeight));
            }
            _minimizeRect = new Rect(_customTitleControl.minimizwButton.PointToScreen(
                new Point()),
                new Size(_customTitleControl.minimizwButton.ActualWidth, _customTitleControl.minimizwButton.ActualHeight));
            _closeRect = new Rect(_customTitleControl.closeButton.PointToScreen(
                new Point()),
                new Size(_customTitleControl.closeButton.ActualWidth, _customTitleControl.closeButton.ActualHeight));
        }

        private CustomTitleBar _customTitleControl;
        private Window _sourceWindow;

        private Rect _maximizeRect;
        private Rect _RestoreRect;
        private Rect _minimizeRect;
        private Rect _closeRect;
        private List<Button> _controlButtons;

        private WingyNotesControlButtons _hittestedButton = WingyNotesControlButtons.none;
        private WingyNotesControlButtons _prevHittestedButton = WingyNotesControlButtons.none;

        private SolidColorBrush _mouseOverOverlayBackgroundBrush;
        private SolidColorBrush _pressedOverlayBackgroundBrush;
        private SolidColorBrush _mouseOverWindowCloseButtonBackgroundBrush;
        private SolidColorBrush _mouseOverWindowCloseButtonForegroundBrush;
        private SolidColorBrush _pressedWindowCloseButtonBackgroundBrush;
    }
}
