using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
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
            if (SceneProperties.GameWidth <= 0 || SceneProperties.GameHeight <= 0) return;

            var playerBounds = GetBoundsProportional(
                _playerCharacter.X,
                _playerCharacter.Y,
                SceneProperties.PxFromWidthPercent(_playerCharacter.Size));

            foreach (var member in _testGroup.Members)
            {
                var obj = member.ChildObject;
                if (!obj.IsActive) continue;

                var objBounds = GetBoundsProportional(
                    obj.X, 
                    obj.Y,
                    SceneProperties.PxFromWidthPercent(obj.Size));

                if (Intersects(playerBounds, objBounds))
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

        private (double Left, double Top, double Right, double Bottom) GetBoundsProportional(double x, double y, double sizePx)
        {
            // Convert pixel size to proportional coordinates
            // With PositionProportional flag, X and Y represent the top-left corner (0.0 to 1.0)
            // sizePx is the object's size in pixels (square object)
            
            // Convert pixel dimensions to proportional units
            // Since Size is defined as % of width, we convert width using GameWidth
            double w = sizePx / SceneProperties.GameWidth;  // Width in proportional units (0.0 to 1.0)
            // For height, we need to account for the aspect ratio
            // A square object in pixels becomes a rectangle in proportional space if GameWidth != GameHeight
            double h = sizePx / SceneProperties.GameHeight; // Height in proportional units (0.0 to 1.0)
            
            // X, Y is top-left corner, so bounds extend from (x, y) to (x + w, y + h)
            return (x, y, x + w, y + h);
        }

        private bool Intersects(
            (double Left, double Top, double Right, double Bottom) a,
            (double Left, double Top, double Right, double Bottom) b)
        {
            return a.Left < b.Right &&
                   a.Right > b.Left &&
                   a.Top < b.Bottom &&
                   a.Bottom > b.Top;
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
            if (SceneProperties.GameWidth <= 0 || SceneProperties.GameHeight <= 0) return preferredX;

            var playerBounds = GetBoundsProportional(
                _playerCharacter.X,
                _playerCharacter.Y,
                SceneProperties.PxFromWidthPercent(_playerCharacter.Size));

            // Check if any member would intersect with player at preferred position
            bool hasIntersection = false;
            foreach (var member in group.Members)
            {
                double objX = preferredX + member.OffsetX;
                double objY = group.Y + member.OffsetY; // group.Y should be 1.2 (spawn position)

                var objBounds = GetBoundsProportional(
                    objX,
                    objY,
                    SceneProperties.PxFromWidthPercent(member.ChildObject.Size));

                if (Intersects(playerBounds, objBounds))
                {
                    hasIntersection = true;
                    break;
                }
            }

            // If no intersection, use preferred position
            if (!hasIntersection) return preferredX;

            // Try alternative positions: left, right, then random
            double[] alternatives = { 0.2, 0.8, _random.NextDouble() };
            
            foreach (double altX in alternatives)
            {
                bool safe = true;
                foreach (var member in group.Members)
                {
                    double objX = altX + member.OffsetX;
                    double objY = group.Y + member.OffsetY;

                    var objBounds = GetBoundsProportional(
                        objX,
                        objY,
                        SceneProperties.PxFromWidthPercent(member.ChildObject.Size));

                    if (Intersects(playerBounds, objBounds))
                    {
                        safe = false;
                        break;
                    }
                }

                if (safe) return altX;
            }

            // If all positions intersect, return preferred (shouldn't happen often)
            return preferredX;
        }
    }
}
