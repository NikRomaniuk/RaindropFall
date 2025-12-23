using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<FlowGroup> _flowGroups = new List<FlowGroup>();

        // Level
        private readonly LevelProperties _level;

        // Events
        public event Action<double>? PlayerHealthPercentChanged;
        public event Action? GameOver;

        // Misc
        private bool _isGameOver;
        private readonly Random _random = new Random();
        
        // FlowGroup Management
        /// <summary>
        /// Distance in virtual units past the center where FlowGroups should spawn and despawn
        /// </summary>
        public double RenderDistance { get; set; } = 200;       // Virtual units
        
        /// <summary>
        /// Distance in virtual units between FlowGroups when spawning
        /// </summary>
        public double FlowGroupDistance { get; set; } = 100;    // Virtual units
        
        /// <summary>
        /// Y position of the most recently spawned group (to prevent infinite spawning)
        /// </summary>
        private double _lastSpawnY = double.MaxValue;

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

            // --- Visuals ---
            _root.BackgroundColor = level.BackgroundColor;

            // --- Initial Spawn ---
            // Spawn the first FlowGroup
            SpawnNewFlowGroup(0.5);
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

            // Update all FlowGroups and remove despawned ones
            for (int i = _flowGroups.Count - 1; i >= 0; i--)
            {
                bool isStillActive = _flowGroups[i].Update(deltaTime);
                if (!isStillActive)
                {
                    // Remove despawned group
                    _flowGroups.RemoveAt(i);
                }
            }

            // Check if we need to spawn a new group
            // Spawn a new group if there are no groups, or if the furthest group has moved enough
            if (_flowGroups.Count == 0)
            {
                // No groups exist, spawn one at center
                SpawnNewFlowGroup(0.5);
            }
            else
            {
                // Find the group with the highest Y position (closest to spawn point)
                // Groups move upward (Y decreases), so highest Y = closest to spawn
                double highestY = _flowGroups.Max(g => GetGroupHighestY(g));
                
                // Calculate how far the closest group has moved from the last spawn position
                double distanceFromLastSpawn = _lastSpawnY - highestY;
                
                // Convert FlowGroupDistance to proportional coordinates for comparison
                double flowGroupDistanceProportional = SceneProperties.ProportionalFromVirtualUnitsY(FlowGroupDistance);
                
                // If the closest group has moved FlowGroupDistance units from last spawn, spawn a new one
                if (distanceFromLastSpawn >= flowGroupDistanceProportional)
                {
                    // Spawn new group at the same X position (or randomize if desired)
                    SpawnNewFlowGroup(0.5);
                }
            }

            // Update the Player
            _playerCharacter.Update(deltaTime);

            CheckCollisionsAndApplyDamage();
        }
        
        /// <summary>
        /// Gets the highest Y position (closest to spawn point) of any member in a FlowGroup
        /// </summary>
        private double GetGroupHighestY(FlowGroup group)
        {
            if (group.Members.Count == 0) return double.MinValue;
            return group.Members.Where(m => m.ChildObject.IsActive)
                               .Select(m => m.ChildObject.Y)
                               .DefaultIfEmpty(double.MinValue)
                               .Max();
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
        /// Checks collisions with all FlowGroups
        /// </summary>
        private void CheckCollisionsAndApplyDamage()
        {
            foreach (var group in _flowGroups)
            {
                foreach (var member in group.Members)
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
        }

        /// <summary>
        /// Checks if two GameObjects are colliding using their visual Bounds
        /// Uses AABB (Axis-Aligned Bounding Box) collision detection
        /// </summary>
        private bool CheckForCollision(GameObject obj1, GameObject obj2)
        {
            // Ensure visuals are updated and have valid bounds
            if (obj1.Visual == null || obj2.Visual == null) return false;
            
            // Get the actual rendered bounds of both visuals
            var bounds1 = obj1.Visual.Bounds;
            var bounds2 = obj2.Visual.Bounds;
            
            // Check if rectangles overlap using AABB collision detection
            bool overlapX = bounds1.Left < bounds2.Right && bounds1.Right > bounds2.Left;
            bool overlapY = bounds1.Top < bounds2.Bottom && bounds1.Bottom > bounds2.Top;
            
            return overlapX && overlapY;
        }

        // --- Other ---

        /// <summary>
        /// Creates and spawns a new FlowGroup at a specified horizontal position
        /// </summary>
        private void SpawnNewFlowGroup(double startX)
        {
            // Calculate spawn Y position and update last spawn tracking
            double spawnYProportional = 0.5 + SceneProperties.ProportionalFromVirtualUnitsY(RenderDistance);
            _lastSpawnY = spawnYProportional;

            // Create a new FlowGroup
            var newGroup = new FlowGroup(_level.FallingSpeed, RenderDistance);
            
            // Build the formation using the level's formation builder
            _level.BuildFormation(newGroup);
            
            // Add to the list of active groups
            _flowGroups.Add(newGroup);
            
            // Spawn the group at the specified X position
            newGroup.Spawn(startX);
            
            // Ensure all visuals are in the scene
            foreach (var member in newGroup.Members)
            {
                if (!_scene.Children.Contains(member.ChildObject.Visual))
                {
                    _scene.Children.Add(member.ChildObject.Visual);
                }
            }

            #if DEBUG && !ANDROID
            Debug.WriteLine($"Flow Group Spawned! Total groups: {_flowGroups.Count}");
            #endif
        }
    }
}
