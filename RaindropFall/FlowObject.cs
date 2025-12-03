using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace RaindropFall
{
    public class FlowObject
    {
        // UI
        public BoxView Visual { get; set; }

        // Position
        public double X { get; set; }
        public double Y { get; set; }

        // Properties
        public double Size { get; set; } // In Pixels
        public double Speed { get; set; } // Pixels per second
        public bool IsActive { get; set; } // To despawn it later on

        public FlowObject(Color color, double size, double speed)
        {
            Size = size;
            Speed = speed;
            IsActive = true;

            // Initialize the UI
            Visual = new BoxView
            {
                Color = color,
                WidthRequest = size,
                HeightRequest = size,
                CornerRadius = 3
            };

            // Initial Spawn Position
            // 1.2 is slightly below bottom boudary of the screen
            Y = 1.2;
        }

        /// <summary>
        /// Spawns the Object
        /// </summary>
        public void Spawn(double startX)
        {
            X = startX;
            Y = 1.2;
            IsActive = true;
            UpdateUI();
        }

        /// <summary>
        /// Called every frame to move the object. Returns False if object has despawned
        /// </summary>
        public bool Update(double deltaTime, double gameAreaHeight)
        {
            if (!IsActive) return false;

            // Calculate distance to move this frame
            double distance = Speed * deltaTime;

            // Convert to Proportional movement
            // Formula: ProportionalChange = Distance / (GameHeight - ObjectHeight)
            // AbsoluteLayout available space calculated as: ParentSize - ChildSize
            double effectiveHeight = gameAreaHeight - Size;

            // Avoid divide by zero errors
            if (effectiveHeight <= 0) return true;

            double proportionalChange = distance / effectiveHeight;

            // Move Upwards
            Y -= proportionalChange;

            // Update the UI and its Position on the screen
            UpdateUI();

            // Check Y position if above top boundary to despawn
            if (Y < -0.2)
            {
                IsActive = false;
                return false;
            }

            return true;
        }

        private void UpdateUI()
        {
            // Sets the object position
            // based on its constantly updating proportional coordinates and its fixed size
            AbsoluteLayout.SetLayoutBounds(Visual, new Rect(X, Y, Size, Size));
            // Tells the AbsoluteLayout container how to interpret object position
            AbsoluteLayout.SetLayoutFlags(Visual, AbsoluteLayoutFlags.PositionProportional);
        }
    }
}
