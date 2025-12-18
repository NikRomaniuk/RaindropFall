using CommunityToolkit.Maui.Core;

namespace RaindropFall
{
    public partial class MainPage : ContentPage
    {
        // Manager holds all game logic and objects
        private GameManager _gameManager;

        private bool _sceneInitialized = false;

        // Input Handling
        private bool _leftHeld;
        private bool _rightHeld;

        private enum LastPressedSide { None, Left, Right }
        private LastPressedSide _lastPressed = LastPressedSide.None;

        #if WINDOWS
        private WindowsInput? _windowsInput;
        #endif

        public MainPage()
        {
            InitializeComponent();

            // Subscribe to the Loaded event
            this.Loaded += OnPageLoaded;
            Scene.SizeChanged += OnSceneSizeChanged;

            // Initialize the GameManager
            _gameManager = new GameManager(Scene, DropCharacter);
        }

        protected override void OnDisappearing()
        {
            _gameManager.StopGameLoop();

            // Unsubscribe from Windows events
            #if WINDOWS
            _windowsInput?.Detach();
            #endif

            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("Page disappearing");
        }

        private void OnSceneSizeChanged(object? sender, EventArgs e)
        {
            if (_sceneInitialized) return;
            if (Scene.Width <= 0 || Scene.Height <= 0) return;

            // --- Setup Scene Properties ---

            SceneProperties.Height = Scene.Height;
            SceneProperties.Width = Scene.Width;
            SceneProperties.SetGameAreaDimensions();

            Scene.WidthRequest = SceneProperties.GameWidth;
            Scene.HeightRequest = SceneProperties.GameHeight;

            _sceneInitialized = true;
            System.Diagnostics.Debug.WriteLine($"Scene initialized: {Scene.Width} x {Scene.Height}");
        }

        private void OnPageLoaded(object sender, EventArgs e)
        {
            // --- Controls setup ---
            // Set up controls for Desktop
            #if WINDOWS
            // Windows-only: keyboard input + disable touch zones
            _windowsInput = new WindowsInput(this, _gameManager, Scene, LeftInputArea, RightInputArea);
            _windowsInput.Attach();
            #endif

            // --- Game ---
            // Start the Game Loop
            _gameManager.StartGameLoop();
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