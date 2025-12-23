using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace RaindropFall
{
    public class FlowObject : GameObject
    {
        public const int OBJECT_ZINDEX = 20;   // Generic Object Layer

        public double Speed { get; set; } // Virtual units per second (100 = GameWidth per second)
        public double RenderDistance { get; set; } // Virtual units past center for spawn/despawn

        public FlowObject(Color color, double size, double speed, double renderDistance = 20.0) : base(size)
        {
            // Size is in virtual units (100 = GameWidth)
            Size = size;
            Speed = speed;
            RenderDistance = renderDistance;
            IsActive = true;

            // Initialize the UI
            Visual = new BoxView
            {
                Color = color,
                WidthRequest = size,
                HeightRequest = size,
                CornerRadius = 3
            };
            // Set ZIndex
            Visual.ZIndex = OBJECT_ZINDEX;
        }

        /// <summary>
        /// Spawns the Object
        /// </summary>
        public virtual void Spawn(double startX)
        {
            X = startX;
            // Spawn at center (0.5) + RenderDistance converted to proportional coordinates
            // Using Y conversion to account for aspect ratio
            double spawnYProportional = 0.5 + SceneProperties.ProportionalFromVirtualUnitsY(RenderDistance);
            Y = spawnYProportional;
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

            // Check Y position if past despawn boundary (center - RenderDistance)
            double despawnYProportional = 0.5 - SceneProperties.ProportionalFromVirtualUnitsY(RenderDistance);
            if (Y < despawnYProportional)
            {
                IsActive = false;
                return false;
            }

            return true;
        }
    }
}
