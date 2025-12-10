using System;
using System.Diagnostics;

namespace RaindropFall
{
    public class GameManager
    {
        // --- Dependencies and Collections ---

        // The UI Element where all Interactive GameObjects will be drawn
        private readonly AbsoluteLayout _scene;

        // Game Objects
        private readonly Player _playerCharacter;
        private FlowGroup _testGroup;

        // Other
        private readonly Random _random = new Random();

        // --- Constructor ---

        /// <summary>
        /// Initializes the GameManager and all primary game entities
        /// </summary>
        public GameManager(AbsoluteLayout scene, BoxView playerVisual)
        {
            _scene = scene;

            // --- Initialize Objects ---

            // Player
            _playerCharacter = new Player(playerVisual, 0.5, 0.85, 30);

            // Test Group
            _testGroup = new FlowGroup(200);
            // Build the Group
            // With "V" formation forexample
            _testGroup.AddObstacle(0.0, 0.0, Colors.Red, 25);
            _testGroup.AddObstacle(-0.15, 0.1, Colors.DarkRed, 25);
            _testGroup.AddObstacle(0.15, 0.1, Colors.PaleVioletRed, 25);

            // --- Add Visuals ---

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
            // Update the Group
            bool isStillActive = _testGroup.Update(deltaTime);

            if (!isStillActive)
            {
                // Respawn Group on its despawn
                SpawnFlowGroup(_testGroup, 0.5);
            }

            // Update the Player
            _playerCharacter.Update(deltaTime);
        }

        // --- Input Handling ---
        public void SetPlayerDirection(Direction direction)
        {
            _playerCharacter.SetDirection(direction);
        }

        public void StopPlayerMovement()
        {
            _playerCharacter.Stop();
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
