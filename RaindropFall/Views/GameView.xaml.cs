using CommunityToolkit.Maui.Core;
using Microsoft.Maui.ApplicationModel;

namespace RaindropFall.Views
{
    public partial class GameView : ContentView
    {
        // Manager holds all game logic and objects
        private GameManager? _gameManager;
        private ContentPage? _hostPage;
        // Current level
        private LevelProperties? _level;
        private bool _pendingStart;
        private bool _sceneInitialized;

        // --- Input Handling ---
        private bool _leftHeld;
        private bool _rightHeld;

        private enum LastPressedSide { None, Left, Right }
        private LastPressedSide _lastPressed = LastPressedSide.None;

        #if WINDOWS
        private WindowsInput? _windowsInput;
        #endif

        public GameView()
        {
            InitializeComponent();
            Scene.SizeChanged += OnSceneSizeChanged;
        }

        public void Initial(LevelProperties level, ContentPage hostPage)
        {
            _hostPage = hostPage;
            _level = level;
            _pendingStart = true;

            if (_sceneInitialized)
                Start();
        }

        public void Start()
        {
            if (_gameManager != null || _level == null) return;

            // Setup neccesery components
            _gameManager = new GameManager(Root, Scene, _level);

            // Subscribe on Events
            _gameManager.PlayerHealthPercentChanged += OnPlayerHealthChanged;
            _gameManager.GameOver += OnGameOver;

            // Attach key controls
            #if WINDOWS
            _windowsInput = new WindowsInput(_gameManager, LeftInputArea, RightInputArea);
            _windowsInput.Attach();
            #endif

            // Apply background colors per level
            Root.BackgroundColor = _level.BackgroundColor;          // top/bottom center area
            CenterArea.BackgroundColor = _level.BackgroundColor;    // center column background
            SceneBackground.Color = _level.BackgroundColor;         // inside scene
            LeftGutter.Color = Colors.Black.WithAlpha(0.25f);
            RightGutter.Color = Colors.Black.WithAlpha(0.25f);

            _gameManager.StartGameLoop();
            _pendingStart = false;
        }

        public void Stop()
        {
            // Unsubscribe from Events
            if (_gameManager != null)
            {
                _gameManager.PlayerHealthPercentChanged -= OnPlayerHealthChanged;
                _gameManager.GameOver -= OnGameOver;
                _gameManager.StopGameLoop();
            }

            // Detach key controls
            #if WINDOWS
            _windowsInput?.Detach();
            #endif

            // Get rid of components
            _gameManager = null;
            _hostPage = null;
        }

        // --- Events ---

        private void OnPlayerHealthChanged(double hpPercent)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                hpPercent = Math.Clamp(hpPercent, 0.0, 1.0);

                // Get original icon size
                var iconSize = HpIconContainer.HeightRequest;

                // Fill height scales with HP
                HpIconFill.HeightRequest = iconSize * hpPercent;

                // Hide at 0%
                HpIconContainer.IsVisible = hpPercent > 0;
            });
        }

        private void OnGameOver()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Hide HP Bar
                HpIconContainer.IsVisible = false;

                // Stop Game loop and disable inputs
                _gameManager?.StopGameLoop();           // Stop Game loop
                LeftInputArea.InputTransparent = true;
                RightInputArea.InputTransparent = true;

                // Alert
                if (_hostPage != null)
                    await _hostPage.DisplayAlert("Game Over", "HP reached 0", "OK");
            });
        }

        private void OnSceneSizeChanged(object? sender, EventArgs e)
        {
            if (_sceneInitialized) return;
            if (Scene.Width <= 0 || Scene.Height <= 0) return;

            // --- Setup Scene Properties ---

            SceneProperties.Height = Root.Height;
            SceneProperties.Width = Root.Width;
            SceneProperties.SetGameAreaDimensions();

            // Setup size for Scene Container / Central Row
            CenterArea.WidthRequest = SceneProperties.GameWidth;
            CenterArea.HeightRequest = SceneProperties.GameHeight;

            //Scene.WidthRequest = SceneProperties.GameWidth;
            //Scene.HeightRequest = SceneProperties.GameHeight;

            _sceneInitialized = true;
            if (_pendingStart)
                Start();
            System.Diagnostics.Debug.WriteLine($"Scene initialized: {Scene.Width} x {Scene.Height}");
        }

        // --- Input Handling ---

        // ===================================
        // TOUCH/POINTER INPUT (iOS & Android)
        // ===================================

        private void OnLeftTouchStatusChanged(object sender, TouchStatusChangedEventArgs e)
        {
            #if WINDOWS
            return; // Windows uses keyboard (WindowsInput)
            #endif

            if (e.Status == TouchStatus.Started)
            {
                _leftHeld = true;
                _lastPressed = LastPressedSide.Left;
            }
            else if (e.Status == TouchStatus.Completed || e.Status == TouchStatus.Canceled)
            {
                _leftHeld = false;
            }

            ApplyCombinedTouchInput();
        }

        private void OnRightTouchStatusChanged(object sender, TouchStatusChangedEventArgs e)
        {
            #if WINDOWS
            return;
            #endif

            if (e.Status == TouchStatus.Started)
            {
                _rightHeld = true;
                _lastPressed = LastPressedSide.Right;
            }
            else if (e.Status == TouchStatus.Completed || e.Status == TouchStatus.Canceled)
            {
                _rightHeld = false;
            }

            ApplyCombinedTouchInput();
        }

        private void ApplyCombinedTouchInput()
        {
            // Resolve direction after any touch zone release
            if (_leftHeld && _rightHeld)
            {
                if (_lastPressed == LastPressedSide.Left)
                    _gameManager.SetPlayerDirection(Direction.Left);
                else if (_lastPressed == LastPressedSide.Right)
                    _gameManager.SetPlayerDirection(Direction.Right);
                else
                    _gameManager.StopPlayerMovement();

                return;
            }

            if (_leftHeld)
            {
                _gameManager.SetPlayerDirection(Direction.Left);
                return;
            }

            if (_rightHeld)
            {
                _gameManager.SetPlayerDirection(Direction.Right);
                return;
            }

            _gameManager.StopPlayerMovement();
        }
    }
}