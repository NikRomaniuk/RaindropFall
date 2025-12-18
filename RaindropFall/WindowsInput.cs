#if WINDOWS
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace RaindropFall
{
    public class WindowsInput
    {
        private readonly MainPage _page;
        private readonly GameManager _gameManager;
        private readonly AbsoluteLayout _scene;
        private readonly View _leftInputArea;
        private readonly View _rightInputArea;

        private bool _leftArrowDown;
        private bool _rightArrowDown;

        private UIElement? _nativeView;

        public WindowsInput(
            MainPage page,
            GameManager gameManager,
            AbsoluteLayout scene,
            View leftInputArea,
            View rightInputArea)
        {
            _page = page;
            _gameManager = gameManager;
            _scene = scene;
            _leftInputArea = leftInputArea;
            _rightInputArea = rightInputArea;
        }

        public void Attach()
        {
            // Get the native WinUI view for this page
            if (_page.Handler?.PlatformView is UIElement nativeView)
            {
                _nativeView = nativeView;

                // Keyboard
                nativeView.IsTabStop = true;
                nativeView.KeyDown += OnKeyDown;
                nativeView.KeyUp += OnKeyUp;

                // Pointer: any click in the window re-focuses this view
                nativeView.PointerPressed += OnPointerPressed;

                nativeView.Focus(FocusState.Programmatic);

                System.Diagnostics.Debug.WriteLine("WindowsInputAdapter attached.");
            }

            // Disable touch zones on Windows completely
            DisableTouchZones();
        }

        public void Detach()
        {
            if (_nativeView is UIElement nativeView)
            {
                nativeView.KeyDown -= OnKeyDown;
                nativeView.KeyUp -= OnKeyUp;
                nativeView.PointerPressed -= OnPointerPressed;
            }

            System.Diagnostics.Debug.WriteLine("WindowsInputAdapter detached.");
        }

        private void DisableTouchZones()
        {
            _leftInputArea.InputTransparent = true;
            _leftInputArea.IsEnabled = false;

            _rightInputArea.InputTransparent = true;
            _rightInputArea.IsEnabled = false;
        }

        // -------- Keyboard handling --------

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"KeyDown: {e.Key}");

            if (e.Key == VirtualKey.Left)
            {   
                if (_leftArrowDown) return;   // ignore repeat
                _leftArrowDown = true;

                _gameManager.SetPlayerDirection(Direction.Left);

                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Right)
            {
                if (_rightArrowDown) return;  // ignore repeat
                _rightArrowDown = true;

                _gameManager.SetPlayerDirection(Direction.Right);

                e.Handled = true;
            }
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"KeyUp: {e.Key}");

            if (e.Key == VirtualKey.Left)
            {
                _leftArrowDown = false;
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Right)
            {
                _rightArrowDown = false;
                e.Handled = true;
            }

            // Resolve direction after any key release
            if (_leftArrowDown && !_rightArrowDown)
                _gameManager.SetPlayerDirection(Direction.Left);
            else if (_rightArrowDown && !_leftArrowDown)
                _gameManager.SetPlayerDirection(Direction.Right);
            else
                _gameManager.StopPlayerMovement();
        }

        // Pointer to keep keyboard focus on page

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_nativeView is UIElement nativeView)
            {
                nativeView.Focus(FocusState.Programmatic);
                System.Diagnostics.Debug.WriteLine("Focus restored on pointer press.");
            }
        }
    }
}
#endif
