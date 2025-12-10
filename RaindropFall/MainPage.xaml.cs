// File: MainPage.xaml.cs

using Microsoft.Maui.Layouts;
using System;

namespace RaindropFall
{
    public partial class MainPage : ContentPage
    {
        // Manager holds all game logic and objects
        private GameManager _gameManager;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoaded;

            // Initialize the GameManager
            // Passing the UI elements it needs
            _gameManager = new GameManager(Scene, DropCharacter);
        }

        // --- Event Subscription Management ---

        protected override void OnDisappearing()
        {
            _gameManager.StopGameLoop();
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("Page disappearing");
        }

        private void OnPageLoaded(object sender, EventArgs e)
        {
            // --- Setup Scene Properties  ---
            SceneProperties.Height = Scene.Height;
            SceneProperties.Width = Scene.Width;
            SceneProperties.SetGameAreaDimensions();
            System.Diagnostics.Debug.WriteLine("Scene Properties set");

            Scene.WidthRequest = SceneProperties.GameWidth;
            Scene.HeightRequest = SceneProperties.GameHeight;
            System.Diagnostics.Debug.WriteLine("Scene Properties applied to the Scene");

            // Start the Game Loop
            _gameManager.StartGameLoop();

            System.Diagnostics.Debug.WriteLine("Page loaded");
        }

        // --- Input Handling ---

        /// <summary>
        /// Sets the Player's direction state to Left when pressed
        /// </summary>
        private void OnLeftAreaPressed(object sender, PointerEventArgs e)
        {
            _gameManager.SetPlayerDirection(Direction.Left);
        }

        /// <summary>
        /// Sets the Player's direction state to Right when pressed
        /// </summary>
        private void OnRightAreaPressed(object sender, PointerEventArgs e)
        {
            _gameManager.SetPlayerDirection(Direction.Right);
        }

        /// <summary>
        /// Stops the Player's movement when released or the pointer exits
        /// </summary>
        private void OnInputAreaReleased(object sender, PointerEventArgs e)
        {
            _gameManager.StopPlayerMovement();
        }
    }
}