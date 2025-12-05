using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace RaindropFall
{
    // Set up enum variable in RaindropFall so every class can access it
    public enum Direction { None, Left, Right }     // Movement State

    public class Player : GameObject
    {
        // Player Properties 
        public Direction CurrentDirection { get; set; } = Direction.None;

        public double Speed { get; set; } = 100;    // Pixels per second
        
        // Constructor
        public Player(BoxView visual, double initialX, double initialY, double size)
            : base(initialX, initialY, size)
        {
            Visual = visual;
            UpdateUI(); // Set initial position
        }

        /// <summary>
        /// Called every frame to move the object. Returns False if object has despawned
        /// </summary>
        public override bool Update(double deltaTime)
        {
            if (!IsActive) return false;

            // Calculate distance to move this frame
            double distance = Speed * deltaTime;
            double effectiveWidth = SceneProperties.Width - Size;
            double changeX = distance / effectiveWidth;

            if (CurrentDirection == Direction.Left) { changeX *= -1; }      // Move Left
            else if (CurrentDirection == Direction.Right) { }               // Move Right
            else { return true; }                                           // Skip Moving

            // Apply movement
            X += changeX;
            // Clamp X position to screen bounds
            X = Math.Clamp(X, 0.0, 1.0);

            UpdateUI();

            return true;
        }

        // --- Input Handling ---

        /// <summary>
        /// Sets the direction for continuous movement.
        /// </summary>
        public void SetDirection(Direction direction)
        {
            CurrentDirection = direction;
        }

        /// <summary>
        /// Stops horizontal movement.
        /// </summary>
        public void Stop()
        {
            CurrentDirection = Direction.None;
        }
    }
}