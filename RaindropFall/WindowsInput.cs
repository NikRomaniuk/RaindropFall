#if WINDOWS
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace RaindropFall
{
    /// <summary>
    /// Windows-only keyboard input adapter (WinUI).
    /// Hooks key events on Window.Content root and restores focus on any pointer interaction.
    /// </summary>
    public sealed class WindowsInput
    {
        private UIElement? _root;

        private readonly GameManager _gameManager;
        private readonly View _leftInputArea;
        private readonly View _rightInputArea;

        private bool _leftArrowDown;
        private bool _rightArrowDown;

        public WindowsInput(
            GameManager gameManager,
            View leftInputArea,
            View rightInputArea)
        {
            _gameManager = gameManager;
            _leftInputArea = leftInputArea;
            _rightInputArea = rightInputArea;
        }

        public void Attach()
        {
            if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView
                is Microsoft.UI.Xaml.Window winWindow
                && winWindow.Content is Microsoft.UI.Xaml.UIElement root)
            {
                _root = root;

                // Keyboard
                root.KeyDown += OnKeyDown;
                root.KeyUp += OnKeyUp;

                // Focusability
                root.IsTabStop = true;

                // Pointer events (even if handled by children) -> restore focus after click ends
                root.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnAnyPointer), true);
                root.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnAnyPointer), true);
                root.AddHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(OnAnyPointer), true);

                ForceFocus();
                System.Diagnostics.Debug.WriteLine("WindowsInput attached to Window.Content root");
            }

            DisableTouchZones();
        }

        public void Detach()
        {
            if (_root is not null)
            {
                _root.KeyDown -= OnKeyDown;
                _root.KeyUp -= OnKeyUp;

                _root.RemoveHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnAnyPointer));
                _root.RemoveHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnAnyPointer));
                _root.RemoveHandler(UIElement.PointerCanceledEvent, new PointerEventHandler(OnAnyPointer));
            }

            _root = null;

            // reset local state so next level starts clean
            _leftArrowDown = false;
            _rightArrowDown = false;

            System.Diagnostics.Debug.WriteLine("WindowsInput detached from Window.Content root");
        }

        private void DisableTouchZones()
        {
            _leftInputArea.InputTransparent = true;
            _leftInputArea.IsEnabled = false;
            _leftInputArea.IsVisible = false;

            _rightInputArea.InputTransparent = true;
            _rightInputArea.IsEnabled = false;
            _rightInputArea.IsVisible = false;
        }

        // -------- Keyboard handling --------

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"KeyDown: {e.Key}");

            if (e.Key == VirtualKey.Left)
            {
                if (_leftArrowDown) return; // ignore repeats
                _leftArrowDown = true;

                _gameManager.SetPlayerDirection(Direction.Left);
                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Right)
            {
                if (_rightArrowDown) return; // ignore repeats
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

            // resolve direction after any key release
            if (_leftArrowDown && !_rightArrowDown)
                _gameManager.SetPlayerDirection(Direction.Left);
            else if (_rightArrowDown && !_leftArrowDown)
                _gameManager.SetPlayerDirection(Direction.Right);
            else
                _gameManager.StopPlayerMovement();
        }

        // -------- Focus handling --------

        private void OnAnyPointer(object sender, PointerRoutedEventArgs e)
        {
            // refocus AFTER the click finishes
            _root?.DispatcherQueue.TryEnqueue(ForceFocus);
        }

        private void ForceFocus()
        {
            if (_root is null) return;

            _root.IsTabStop = true;
            _root.Focus(FocusState.Programmatic);

            System.Diagnostics.Debug.WriteLine($"ForceFocus() -> focused={_root.FocusState}");
        }
    }
}
#endif
