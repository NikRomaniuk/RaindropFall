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
        private Microsoft.UI.Xaml.Window? _winWindow;
        private Microsoft.UI.Xaml.UIElement? _root;

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
            if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView
                is Microsoft.UI.Xaml.Window winWindow
                && winWindow.Content is Microsoft.UI.Xaml.UIElement root)
            {
                _winWindow = winWindow;
                _root = root;

                // Keyboard events
                root.KeyDown += OnKeyDown;
                root.KeyUp += OnKeyUp;

                // Make it focusable
                root.IsTabStop = true;

                // Pointer events: catch them even if children handle them
                root.AddHandler(Microsoft.UI.Xaml.UIElement.PointerPressedEvent,
                    new Microsoft.UI.Xaml.Input.PointerEventHandler(OnAnyPointer),
                    true);

                root.AddHandler(Microsoft.UI.Xaml.UIElement.PointerReleasedEvent,
                    new Microsoft.UI.Xaml.Input.PointerEventHandler(OnAnyPointer),
                    true);

                root.AddHandler(Microsoft.UI.Xaml.UIElement.PointerCanceledEvent,
                    new Microsoft.UI.Xaml.Input.PointerEventHandler(OnAnyPointer),
                    true);

                // Initial focus
                ForceFocus();

                System.Diagnostics.Debug.WriteLine("WindowsInput attached to Window.Content root");
            }

            // Disable touch zones on Windows completely
            DisableTouchZones();
        }

        public void Detach()
        {
            if (_root is not null)
            {
                _root.KeyDown -= OnKeyDown;
                _root.KeyUp -= OnKeyUp;

                _root.RemoveHandler(Microsoft.UI.Xaml.UIElement.PointerPressedEvent,
                    new Microsoft.UI.Xaml.Input.PointerEventHandler(OnAnyPointer));

                _root.RemoveHandler(Microsoft.UI.Xaml.UIElement.PointerReleasedEvent,
                    new Microsoft.UI.Xaml.Input.PointerEventHandler(OnAnyPointer));

                _root.RemoveHandler(Microsoft.UI.Xaml.UIElement.PointerCanceledEvent,
                    new Microsoft.UI.Xaml.Input.PointerEventHandler(OnAnyPointer));
            }
            _root = null;
            _winWindow = null;

            System.Diagnostics.Debug.WriteLine("WindowsInput dettached from Window.Content root");
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

        // --- Helper methods ---

        // Pointer to keep keyboard focus on page

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_nativeView is UIElement nativeView)
            {
                nativeView.Focus(FocusState.Programmatic);
                System.Diagnostics.Debug.WriteLine("Focus restored on pointer press.");
            }
        }

        private void OnAnyPointer(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            // Refocus AFTER the click finishes (works around focus changing on release)
            _root?.DispatcherQueue.TryEnqueue(() => ForceFocus());
        }

        private void ForceFocus()
        {
            if (_root is null) return;

            _root.IsTabStop = true;
            _root.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);

            System.Diagnostics.Debug.WriteLine($"ForceFocus() -> focused={_root.FocusState}");
        }
    }
}
#endif
