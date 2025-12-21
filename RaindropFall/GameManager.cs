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

            // --- Initialize Objects ---

            // Player
            var playerVisual = new BoxView
            {
                Color = Colors.Cyan,
                CornerRadius = 5,
                ZIndex = 50
            };
            _scene.Children.Add(playerVisual);

            _playerCharacter = new Player(0.5, 0.5, 10, playerVisual, level.PlayerSpeed);
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
                    _playerCharacter.TakeDamage(40);

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
            double w = sizePx / SceneProperties.GameWidth;
            double h = sizePx / SceneProperties.GameHeight;
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
        /// </summary>
        private void SpawnFlowGroup(FlowObject obj, double startX)
        {
            obj.Spawn(startX);
            Debug.WriteLine("Flow Group Spawned!");
        }
    }
}
