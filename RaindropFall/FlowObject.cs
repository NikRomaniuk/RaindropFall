using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace RaindropFall
{
    public class FlowObject : GameObject
    {
        public double Speed { get; set; } // Pixels per second

        public FlowObject(Color color, double size, double speed) : base(0, 1.2, size)
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
        public virtual void Spawn(double startX)
        {
            X = startX;
            Y = 1.2;
            IsActive = true;
            UpdateUI();
        }

        /// <summary>
        /// Called every frame to move the object. Returns False if object has despawned
        /// </summary>
        public override bool Update(double deltaTime)
        {
            if (!IsActive) return false;

            // Calculate distance to move this frame
            double distance = Speed * deltaTime;

            // Convert to Proportional movement
            // Formula: ProportionalChange = Distance / (GameHeight - ObjectHeight)
            // AbsoluteLayout available space calculated as: ParentSize - ChildSize
            double effectiveHeight = SceneProperties.Height - Size;

            // Avoid divide by zero errors
            if (effectiveHeight <= 0) return true;

            double changeY = distance / effectiveHeight;

            // Move Upwards
            Y -= changeY;

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

        public virtual void UpdateUI()
        {
            // Sets the object position
            // based on its constantly updating proportional coordinates and its fixed size
            AbsoluteLayout.SetLayoutBounds(Visual, new Rect(X, Y, Size, Size));
            // Tells the AbsoluteLayout container how to interpret object position
            AbsoluteLayout.SetLayoutFlags(Visual, AbsoluteLayoutFlags.PositionProportional);
        }
    }
}
