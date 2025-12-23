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
        // --- Constants ---
        public const int OBJECT_ZINDEX = 50;   // Player Layer

        // --- Player Properties ---
        // Movement
        public MoveState MovementState { get; set; } = MoveState.Idle;
        public Direction CurrentDirection { get; private set; } = Direction.None;
        public double MaxSpeed { get; set; }                        // Virtual units per second (100 = GameWidth per second)
        public double CurrentSpeed { get; private set; } = 0.0;     // Current speed (StartingSpeedPercent * Speed => Speed)
        public double Acceleration { get; set; }                    // Virtual units per second^2
        public double StartingSpeedPercent { get; set; } = 0.5;     // Starting speed as % of Max Speed (0.5 = 50%)
        // HP
        public int Health { get; private set; } = 100;      // HP
        // Misc
        public double HealthPercent => Math.Clamp(Health / 100.0, 0.0, 1.0); // % of HP

        // --- Events ---
        public event Action<double>? HealthPercentChanged;

        // Constructor
        public Player(double initialX, double initialY, double size, BoxView visual, double speed, double acceleration)
            : base(size)
        {
            Visual = visual;
            MaxSpeed = speed;
            Acceleration = acceleration;

            X = initialX;
            Y = initialY;

            UpdateUI(); // Set initial position

            // Set ZIndex
            Visual.ZIndex = OBJECT_ZINDEX;
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
                CurrentSpeed = 0.0; // Reset speed when idle
                return true;
            }

            MovementState = MoveState.Moving;

            // Ensure we're at least at starting speed when moving
            double startingSpeed = MaxSpeed * StartingSpeedPercent;
            if (CurrentSpeed < startingSpeed)
            {
                CurrentSpeed = startingSpeed;
            }

            // Accelerate towards max speed with non-linear acceleration
            // Calculate acceleration effectiveness based on current speed
            double accelerationMultiplier = GetAccelerationMultiplier(CurrentSpeed / MaxSpeed);
            CurrentSpeed += Acceleration * deltaTime * accelerationMultiplier;
            
            // Clamp to max speed
            CurrentSpeed = Math.Min(CurrentSpeed, MaxSpeed);

            // Calculate the distance to move this frame
            // Formula: ProportionalChange = ProportionalSpeed * deltaTime
            double changeX = (CurrentSpeed / 100) * deltaTime;

            if (CurrentDirection == Direction.Left) { changeX *= -1; }      // Move Left
            else if (CurrentDirection == Direction.Right) { }               // Move Right

            // Apply movement
            X += changeX;
            // Clamp X position to screen bounds
            X = Math.Clamp(X, 0.0, 1.0);

            UpdateUI();

            return true;
        }

        /// <summary>
        /// Calculates acceleration effectiveness based on speed percentage
        /// At 70% Speed => 100% Effectiveness
        /// At 80% Speed => 75% Effectiveness
        /// At 90% Speed => 50% Effectiveness (Capped)
        /// </summary>
        private double GetAccelerationMultiplier(double speedPercent)
        {
            if (speedPercent < 0.7)
            {
                // Below 70%: full acceleration
                return 1.0;
            }
            else if (speedPercent < 0.8)
            {
                // 70% to 80%: interpolate from 1.0 to 0.75
                double t = (speedPercent - 0.7) / 0.1; // 0 to 1
                return 1.0 - (t * 0.25);
            }
            else if (speedPercent < 0.9)
            {
                // 80% to 90%: interpolate from 0.75 to 0.5
                double t = (speedPercent - 0.8) / 0.1; // 0 to 1
                return 0.75 - (t * 0.25);
            }
            else
            {
                // 90% and above: capped at 50%
                return 0.5;
            }
        }

        // --- Input Handling ---

        /// <summary>
        /// Sets the movement direction
        /// </summary>
        public void SetDirection(Direction direction)
        {
            // If changing direction => reset Speed
            if (CurrentDirection != Direction.None &&
                direction != Direction.None &&
                CurrentDirection != direction)
            {
                CurrentSpeed = MaxSpeed * StartingSpeedPercent;
            }

            CurrentDirection = direction;
        }

        public void Stop()
        {
            // Stop all movement
            CurrentDirection = Direction.None;
            MovementState = MoveState.Idle;
            CurrentSpeed = 0.0;
        }

        /// <summary>
        /// Substracts from HP
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (amount <= 0) return;

            Health = Math.Clamp(Health - amount, 0, 100);
            HealthPercentChanged?.Invoke(HealthPercent);
        }
    }
}