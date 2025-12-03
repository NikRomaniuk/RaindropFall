using Microsoft.Maui.Layouts;
using System;

namespace RaindropFall
{
    public partial class MainPage : ContentPage
    {
        // Player (Cyan Square)
        private double playerX = 0.5;
        private double playerY = 0.5;
        private const double playerSize = 30; // Pixel size

        // TestDrop (Red (But not obviously red) Square)
        private FlowObject testObstacle;

        // Other
        private readonly Random random = new Random(); // Random class object

        public MainPage()
        {
            InitializeComponent();

            // But still red in this case
            testObstacle = new FlowObject(Colors.Red, 20, 100);
            Scene.Children.Add(testObstacle.Visual);
            SpawnFlowObject(testObstacle);
        }

        // --- Event Subscription Management ---

        // Override default method
        protected override void OnAppearing()
        {
            base.OnAppearing();
            GlobalEvents.Update += OnUpdate;
            System.Diagnostics.Debug.WriteLine("Scene subscribed to Update");
        }
        // Override default method
        protected override void OnDisappearing()
        {
            GlobalEvents.Update -= OnUpdate;
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("Scene unsubscribed from Update");
        }

        // --- Game Logic ---

        /// <summary>
        /// Invokes every Frame
        /// </summary>
        private void OnUpdate(double deltaTime)
        {
            // Update Obstacle
            bool isStillActive = testObstacle.Update(deltaTime, Scene.Height);

            if (!isStillActive)
            {
                // Respawn object on its despawn
                SpawnFlowObject(testObstacle);
            }

            // --- Update the UI ---

            // Main Character
            AbsoluteLayout.SetLayoutBounds(DropCharacter, new Rect(playerX, playerY, playerSize, playerSize));
            AbsoluteLayout.SetLayoutFlags(DropCharacter, AbsoluteLayoutFlags.PositionProportional);
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
    }
}
