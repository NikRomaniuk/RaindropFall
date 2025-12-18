using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace RaindropFall
{
    // Set up enum variable in RaindropFall so every class can access it
    public enum MoveState { Idle, Moving }      // Movement State
    public enum Direction { None, Left, Right}  // Movement Direction

    public class Player : GameObject
    {
        // Player Properties 
        public MoveState MovementState { get; set; } = MoveState.Idle;
        public Direction CurrentDirection { get; private set; } = Direction.None;

        public double Speed { get; set; }           // % of Screen per second
        
        // Constructor
        public Player(double initialX, double initialY, double size, BoxView visual, double speed)
            : base(initialX, initialY, size)
        {
            Visual = visual;
            Speed = speed;
            UpdateUI(); // Set initial position
        } 

        /// <summary>
        /// Called every frame to move the object. Returns False if object has despawned
        /// </summary>
        public override bool Update(double deltaTime)
        {
            if (!IsActive) return false;

            if (CurrentDirection == Direction.None)
            {
                MovementState = MoveState.Idle;
                return true;
            }

            MovementState = MoveState.Moving;

            // Calculate the distance to move this frame
            // Formula: ProportionalChange = ProportionalSpeed * deltaTime
            double changeX = (Speed / 100) * deltaTime;

            if (CurrentDirection == Direction.Left) { changeX *= -1; }      // Move Left
            else if (CurrentDirection == Direction.Right) { }               // Move Right

            // Apply movement
            X += changeX;
            // Clamp X position to screen bounds
            X = Math.Clamp(X, 0.0, 1.0);

            UpdateUI();

            return true;
        }

        // --- Input Handling ---

        /// <summary>
        /// Sets the movement direction
        /// </summary>
        public void SetDirection(Direction direction)
        {
            CurrentDirection = direction;
        }

        public void Stop()
        {
            CurrentDirection = Direction.None;
            MovementState = MoveState.Idle;
        }
    }
}