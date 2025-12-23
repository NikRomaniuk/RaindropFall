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

            // Add visuals for all group members
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

                if (CheckForCollision(_playerCharacter, obj))
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
        /// Checks if two GameObjects are colliding based on their position and size (proportional coordinates)
        /// Uses math-based collision detection to avoid expensive Bounds property access
        /// </summary>
        private bool CheckForCollision(GameObject obj1, GameObject obj2)
        {
            // Size is a percentage of screen width
            // Convert to proportional coordinates: Size% / 100 = proportional size
            double proportionalSize1 = obj1.Size / 100.0;
            double proportionalSize2 = obj2.Size / 100.0;
            
            // Calculate half-sizes in proportional coordinates
            double halfSize1X = proportionalSize1 / 2.0;
            double halfSize2X = proportionalSize2 / 2.0;
            
            // For Y, we need to account for aspect ratio since Size is width-based
            // Objects are squares in pixels, but proportional Y size depends on GameHeight
            double aspectRatio = SceneProperties.GameHeight / SceneProperties.GameWidth;
            double halfSize1Y = (proportionalSize1 / aspectRatio) / 2.0;
            double halfSize2Y = (proportionalSize2 / aspectRatio) / 2.0;

            // Check if rectangles overlap (using AABB collision detection)
            bool overlapX = Math.Abs(obj1.X - obj2.X) < (halfSize1X + halfSize2X);
            bool overlapY = Math.Abs(obj1.Y - obj2.Y) < (halfSize1Y + halfSize2Y);

            return overlapX && overlapY;
        }

        // --- Other ---

        /// <summary>
        /// Spawn FlowGroup at a specified horizontal position
        /// Spawns at constant Y position 1.2
        /// Optimized to avoid removing/adding UI elements (which causes expensive layout passes on Android)
        /// </summary>
        private void SpawnFlowGroup(FlowGroup group, double startX)
        {
            // Since RecreateMembers() now reuses existing objects, we don't need to remove/add visuals
            // The same BoxView objects stay in the scene, we just update their properties
            
            // Spawn the group at the specified X position (Y will be set to 1.2 in Spawn)
            group.Spawn(startX);

            // Ensure all visuals are in the scene (only add if missing - should only happen on first spawn)
            foreach (var member in group.Members)
            {
                if (!_scene.Children.Contains(member.ChildObject.Visual))
                {
                    _scene.Children.Add(member.ChildObject.Visual);
                }
            }

            #if DEBUG && !ANDROID
            Debug.WriteLine("Flow Group Spawned!");
            #endif
        }
    }
}
