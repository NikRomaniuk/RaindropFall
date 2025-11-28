
using Microsoft.Maui.Layouts;

namespace RaindropFall
{
    public partial class MainPage : ContentPage
    {
        // Player (Cyan Square)
        private double playerX = 0.5;
        private double playerY = 0.5;
        private const double playerSize = 30; // Pixel size

        // TestDrop (Red Square)
        private double testDropX = 0.8; // Initial X position (80% from left)
        private double testDropY = 0.1; // Initial Y position (10% from top)
        private double testDropVelocityX, testDropVelocityY;
        private const double testDropSpeed = 0.2; // 0.5 means 50% of the screen width in 1 second
        private const double testDropSize = 25; // Pixel size

        // Direction Change Timer
        private double directionTimer = 0;
        private const double directionChangeInterval = 2.0; // In seconds
        
        // Other
        private readonly Random random = new Random(); // Random class object

        public MainPage()
        {
            InitializeComponent();

            // Set initial velocity
            ChangeDirection();
        }

        // --- Event Subscription Management ---

        // Override default method
        protected override void OnAppearing()
        {
            base.OnAppearing();
            GlobalEvents.Update += OnUpdate;
            System.Diagnostics.Debug.WriteLine("GameView subscribed to Update");
        }
        // Override default method
        protected override void OnDisappearing()
        {
            GlobalEvents.Update -= OnUpdate;
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("GameView unsubscribed from Update");
        }

        /// <summary>
        /// Invokes every Frame
        /// </summary>
        private void OnUpdate(double deltaTime)
        {
            // --- Get position ---

            // Update the direction timer
            directionTimer += deltaTime;

            if (directionTimer >= directionChangeInterval)
            {
                ChangeDirection();
                directionTimer = 0; // Reset the timer
            }

            // Apply movement (using the stored velocity)
            testDropX += testDropVelocityX * deltaTime;
            testDropY += testDropVelocityY * deltaTime;

            // Calculate proportional size based on screen dimensions
            // for accurate boundary checks
            double sceneWidth = Scene.Width;
            double sceneHeight = Scene.Height;

            double testDropWidthRatio = testDropSize / sceneWidth;
            double testDropHeightRatio = testDropSize / sceneHeight;

            // If testDrop cords beyond screen edge
            // Right edge
            if (testDropX + testDropWidthRatio >= 1.0)
            {
                testDropVelocityX = -Math.Abs(testDropVelocityX);
                testDropX = 1.0 - testDropWidthRatio;
            }
            // Left edge
            else if (testDropX <= 0.0)
            {
                testDropVelocityX = Math.Abs(testDropVelocityX);
                testDropX = 0.0;
            }
            // Bottom edge
            if (testDropY + testDropHeightRatio >= 1.0)
            {
                testDropVelocityY = -Math.Abs(testDropVelocityY);
                testDropY = 1.0 - testDropHeightRatio;
            }
            // Top edge
            else if (testDropY <= 0.0)
            {
                testDropVelocityY = Math.Abs(testDropVelocityY);
                testDropY = 0.0;
            }

            // --- Update the UI ---

            // Test Drop
            // Sets the object position
            // based on its constantly updating proportional coordinates and its fixed size
            AbsoluteLayout.SetLayoutBounds(TestMove, new Rect(testDropX, testDropY, testDropSize, testDropSize));
            // Tells the AbsoluteLayout container how to interpret object position
            AbsoluteLayout.SetLayoutFlags(TestMove, AbsoluteLayoutFlags.PositionProportional);

            // Main Character
            AbsoluteLayout.SetLayoutBounds(DropCharacter, new Rect(playerX, playerY, playerSize, playerSize));
            AbsoluteLayout.SetLayoutFlags(DropCharacter, AbsoluteLayoutFlags.PositionProportional);
        }

        /// <summary>
        /// Gets random horizontal velocity for the test drop
        /// </summary>
        private void ChangeDirection()
        {
            // Generate a random angle
            double angle = random.NextDouble() * 2 * Math.PI;

            // Use trigonometry to calculate the X and Y components of the velocity vector
            testDropVelocityX = testDropSpeed * Math.Cos(angle);
            testDropVelocityY = testDropSpeed * Math.Sin(angle);
        }
    }
}
