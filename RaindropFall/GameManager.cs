using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;

namespace RaindropFall
{
    public class GameManager
    {
        // --- Dependencies and Collections ---

        // The UI Element where all Interactive GameObjects will be drawn
        private readonly AbsoluteLayout _scene;
        private readonly Grid _root;

        // Game Objects
        public readonly Player _playerCharacter;
        private FlowGroup _testGroup;

        // Level
        private readonly LevelProperties _level;

        // Events
        public event Action<double>? PlayerHealthPercentChanged;
        public event Action? GameOver;

        // Misc
        private bool _isGameOver;
        private readonly Random _random = new Random();

        // --- Constructor ---

        /// <summary>
        /// Initializes the GameManager and all primary game entities
        /// </summary>
        public GameManager(Grid root, AbsoluteLayout scene, LevelProperties level)
        {
            _scene = scene;
            _root = root;
            _level = level;

            // --- Initialize Objects ---

            // Player
            var playerVisual = new BoxView
            {
                Color = Colors.Cyan,
                CornerRadius = 5,
                ZIndex = 50
            };
            _scene.Children.Add(playerVisual);

            _playerCharacter = new Player(0.5, 0.5, 10, playerVisual, level.PlayerSpeed, level.PlayerAcceleration);
            // Forward player hp updates to UI
            _playerCharacter.HealthPercentChanged += hp => PlayerHealthPercentChanged?.Invoke(hp);
            PlayerHealthPercentChanged?.Invoke(_playerCharacter.HealthPercent);

            // Test Group
            _testGroup = new FlowGroup(level.FallingSpeed);
            // Build the Group
            // With pre-built formation
            level.BuildFormation(_testGroup);

            // --- Visuals ---
            _root.BackgroundColor = level.BackgroundColor;

            _scene.Children.Add(_testGroup.Visual);     // The invisible anchor (FlowGroup itself)
            foreach (var member in _testGroup.Members)
            {
                _scene.Children.Add(member.ChildObject.Visual);
            }

            // --- Initial Spawn ---

            // Test Group
            SpawnFlowGroup(_testGroup, 0.5);
        }

        // --- Event Management ---

        /// <summary>
        /// Subscribes to the Update
        /// </summary>
        public void StartGameLoop()
        {
            GlobalEvents.Update += OnUpdate;
            Debug.WriteLine("GameManager subscribed to Update");
        }

        /// <summary>
        /// Unsubscribes from the Update
        /// </summary>
        public void StopGameLoop()
        {
            GlobalEvents.Update -= OnUpdate;
            Debug.WriteLine("GameManager unsubscribed from Update");
        }

        // --- Core Game Loop ---

        /// <summary>
        /// Invokes every Frame
        /// </summary>
        private void OnUpdate(double deltaTime)
        {
            if (_isGameOver) return;

            // Update the Group
            bool isStillActive = _testGroup.Update(deltaTime);

            if (!isStillActive)
            {
                // Respawn Group on its despawn
                SpawnFlowGroup(_testGroup, 0.5);
            }

            // Update the Player
            _playerCharacter.Update(deltaTime);

            CheckCollisionsAndApplyDamage();
        }

        // --- Player ---

        public void SetPlayerDirection(Direction direction)
        {
            _playerCharacter.SetDirection(direction);
        }

        public void StopPlayerMovement()
        {
            _playerCharacter.Stop();
        }

        // Collision

        /// <summary>
        /// Checks all possible Collisions and applies damage if needeed
        /// Currently only checks for TestGroup collisions
        /// </summary>
        private void CheckCollisionsAndApplyDamage()
        {
            foreach (var member in _testGroup.Members)
            {
                var obj = member.ChildObject;
                if (!obj.IsActive) continue;

                if (CheckForCollision(_playerCharacter.Visual, obj.Visual))
                {
                    // Disable collided object
                    obj.IsActive = false;
                    obj.Visual.IsVisible = false;

                    // Update Player stats
                    _playerCharacter.TakeDamage(_level.DamagePerHit);

                    // Invoke GameOver
                    if (_playerCharacter.Health <= 0)
                    {
                        _isGameOver = true;
                        GameOver?.Invoke();
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Checks if two VisualElements are colliding based on their Bounds
        /// </summary>
        private bool CheckForCollision(VisualElement element1, VisualElement element2)
        {
            // Get the absolute bounds of both elements
            Rect bounds1 = element1.Bounds;
            Rect bounds2 = element2.Bounds;

            // Check if the two rectangles intersect
            return bounds1.IntersectsWith(bounds2);
        }

        // --- Other ---

        /// <summary>
        /// Spawn FlowGroup at a specified horizontal position
        /// Ensures no intersections with player on spawn
        /// </summary>
        private void SpawnFlowGroup(FlowGroup group, double startX)
        {
            // Store old member UI elements before recreating (Spawn will clear Members)
            var oldVisuals = new List<BoxView>();
            foreach (var member in group.Members)
            {
                oldVisuals.Add(member.ChildObject.Visual);
            }

            // Remove old member UI elements from scene
            foreach (var visual in oldVisuals)
            {
                if (_scene.Children.Contains(visual))
                {
                    _scene.Children.Remove(visual);
                }
            }

            // Spawn the group (this will recreate all members as new FlowObjects)
            group.Spawn(startX);

            // Check for intersections with player and adjust spawn position if needed
            double safeX = FindSafeSpawnPosition(group, startX);
            if (safeX != startX)
            {
                group.X = safeX;
                group.UpdateUI();
            }

            // Add new member UI elements to scene
            foreach (var member in group.Members)
            {
                if (!_scene.Children.Contains(member.ChildObject.Visual))
                {
                    _scene.Children.Add(member.ChildObject.Visual);
                }
            }

            Debug.WriteLine("Flow Group Spawned!");
        }

        /// <summary>
        /// Finds a safe spawn position that doesn't intersect with the player
        /// </summary>
        private double FindSafeSpawnPosition(FlowGroup group, double preferredX)
        {
            // Store the original position to restore after checking
            double originalGroupX = group.X;
            group.X = preferredX;
            group.UpdateUI();

            // Check if any member would intersect with player at preferred position
            bool hasIntersection = false;
            foreach (var member in group.Members)
            {
                if (CheckForCollision(_playerCharacter.Visual, member.ChildObject.Visual))
                {
                    hasIntersection = true;
                    break;
                }
            }

            // If no intersection, restore and return preferred position
            if (!hasIntersection)
            {
                group.X = originalGroupX;
                return preferredX;
            }

            // Try alternative positions: left, right, then random
            double[] alternatives = { 0.2, 0.8, _random.NextDouble() };
            
            foreach (double altX in alternatives)
            {
                group.X = altX;
                group.UpdateUI();

                bool safe = true;
                foreach (var member in group.Members)
                {
                    if (CheckForCollision(_playerCharacter.Visual, member.ChildObject.Visual))
                    {
                        safe = false;
                        break;
                    }
                }

                if (safe)
                {
                    group.X = originalGroupX;
                    return altX;
                }
            }

            // Restore original position
            group.X = originalGroupX;
            // If all positions intersect, return preferred (shouldn't happen often)
            return preferredX;
        }
    }
}
