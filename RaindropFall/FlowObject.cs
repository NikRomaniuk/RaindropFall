using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;

namespace RaindropFall
{
    public class FlowObject : GameObject, IAnimatable, ICollidable
    {
        public const int OBJECT_ZINDEX = 20;   // Generic Object Layer

        public double Speed { get; set; } // Proportional units per second (100 - 100% Screen)

        // --- ICollidable Implementation ---
        public bool IsCollidable => IsActive;
        public CollisionLayer CollisionLayer => CollisionLayer.Obstacle;

        public FlowObject(Color color, double size, double speed) : base(size)
        {
            // Size is counted as a % of the Scene (area with interactive objects)
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
            // Set ZIndex
            Visual.ZIndex = OBJECT_ZINDEX;

            // Register with animation controller
            AnimationController.Instance.Register(this);
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
        /// Called every animation frame to move the object (IAnimatable interface)
        /// </summary>
        public void OnAnimate(double deltaTime)
        {
            if (!IsActive) return;

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
                Visual.IsVisible = false;
            }
        }

        /// <summary>
        /// Legacy Update method for compatibility - calls OnAnimate
        /// </summary>
        public override bool Update(double deltaTime)
        {
            OnAnimate(deltaTime);
            return IsActive;
        }

        // --- ICollidable Implementation ---
        
        public CollisionBounds GetBounds()
        {
            // Size is a percentage of screen width
            double proportionalSize = Size / 100.0;
            double halfSizeX = proportionalSize / 2.0;
            
            // Account for aspect ratio for Y
            double aspectRatio = SceneProperties.GameHeight / SceneProperties.GameWidth;
            double halfSizeY = (proportionalSize / aspectRatio) / 2.0;

            return new CollisionBounds(X, Y, halfSizeX, halfSizeY);
        }

        public void OnCollisionEnter(ICollidable other)
        {
            // Collision handling is done by GameManager
        }

        public void OnCollisionExit(ICollidable other)
        {
            // Not needed for obstacles
        }

        /// <summary>
        /// Cleanup when object is destroyed
        /// </summary>
        public void Dispose()
        {
            AnimationController.Instance.Unregister(this);
        }
    }
}
