using Microsoft.Maui.Layouts;
using System;

namespace RaindropFall
{
    public partial class MainPage : ContentPage
    {
        // Player (Cyan Square)
        private Player playerCharacter;

        // Group for testing (contains extremely red (But not obviously red) squares)
        private FlowGroup testGroup;

        // Other
        private readonly Random random = new Random(); // Random class object

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += OnPageLoaded;
            // --- Setup Scene Properties ---

            // --- Player ---
            // Initialize
            playerCharacter = new Player(DropCharacter, 0.5, 0.85, 30);

            // Create a new Group (object formation)
            testGroup = new FlowGroup(200);

            // Build the Group
            // With "V" formation forexample

            testGroup.AddObstacle(0.0, 0.0, Colors.Red, 25);
            testGroup.AddObstacle(-0.15, 0.1, Colors.DarkRed, 25);
            testGroup.AddObstacle(0.15, 0.1, Colors.PaleVioletRed, 25);

            // Add all visual elements to the Scene
            Scene.Children.Add(testGroup.Visual);
            for (int i = 0; i < testGroup.Members.Count; i++)
            {
                Scene.Children.Add(testGroup.Members[i].ChildObject.Visual);
            }
            SpawnFlowGroup(testGroup);
        }

        // --- Event Subscription Management ---

        // Override default method
        protected override void OnDisappearing()
        {
            GlobalEvents.Update -= OnUpdate;
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("Scene unsubscribed from Update");
        }
        // On Page Loaded
        private void OnPageLoaded(object sender, EventArgs e)
        {
            SceneProperties.Height = Scene.Height;
            SceneProperties.Width = Scene.Width;
            System.Diagnostics.Debug.WriteLine("Scene Properties set");

            GlobalEvents.Update += OnUpdate;
            System.Diagnostics.Debug.WriteLine("Scene subscribed to Update");

            System.Diagnostics.Debug.WriteLine("Page loaded");
        }

        // --- Input Handling ---

        /// <summary>
        /// Sets the Player's direction state to Left when pressed
        /// </summary>
        private void OnLeftAreaPressed(object sender, PointerEventArgs e)
        {
            playerCharacter.SetDirection(Direction.Left);
        }

        /// <summary>
        /// Sets the Player's direction state to Right when pressed
        /// </summary>
        private void OnRightAreaPressed(object sender, PointerEventArgs e)
        {
            playerCharacter.SetDirection(Direction.Right);
        }

        /// <summary>
        /// Stops the Player's movement when released or the pointer exits
        /// </summary>
        private void OnInputAreaReleased(object sender, PointerEventArgs e)
        {
            playerCharacter.Stop();
        }

        // --- Game Logic ---

        /// <summary>
        /// Invokes every Frame
        /// </summary>
        private void OnUpdate(double deltaTime)
        {
            // Update the Group
            bool isStillActive = testGroup.Update(deltaTime);

            if (!isStillActive)
            {
                // Respawn Group on its despawn
                SpawnFlowGroup(testGroup);
            }

            // --- Update the UI ---

            // Main Character
            playerCharacter.Update(deltaTime);
        }

        /// <summary>
        /// Spawn FlowObject with random horizontal position
        /// </summary>
        private void SpawnFlowObject(FlowObject obj)
        {
            // Get random X Position
            double randomX = 0.1 + (random.NextDouble() * 0.8);
            obj.Spawn(randomX);

            Console.WriteLine("Flow Object Spawned!");
        }

        /// <summary>
        /// Spawn FlowObject with random horizontal position
        /// </summary>
        private void SpawnFlowGroup(FlowObject obj)
        {
            // Get fixed X Position (Center)
            double X = 0.5;
            obj.Spawn(X);

            Console.WriteLine("Flow Group Spawned!");
        }
    }
}
