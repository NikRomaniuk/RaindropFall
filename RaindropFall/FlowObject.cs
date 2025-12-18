using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace RaindropFall
{
    public class FlowObject : GameObject
    {
        public const int OBJECT_ZINDEX = 20;   // Generic Object Layer

        public double Speed { get; set; } // Proportional units per second (100 - 100% Screen)

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
            // Set ZIndex
            Visual.ZIndex = OBJECT_ZINDEX;
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
        /// Called every frame to move the object
        /// Returns False if object has despawned
        /// </summary>
        public override bool Update(double deltaTime)
        {
            if (!IsActive) return false;

            // Calculate the distance to move this frame
            // Formula: ProportionalChange = ProportionalSpeed * deltaTime
            double changeY = (Speed / 100) * deltaTime;

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
