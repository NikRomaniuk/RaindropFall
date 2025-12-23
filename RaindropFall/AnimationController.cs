using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaindropFall
{
    /// <summary>
    /// Manages continuous animations for game objects using an optimized Task-based loop
    /// This replaces the Unity-like Update loop with a more efficient approach
    /// </summary>
    public class AnimationController
    {
        private readonly List<IAnimatable> _animatables = new List<IAnimatable>();
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _gameLoopTask;
        private bool _isRunning;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _lastTime;

        // Target frame rate: 60 FPS
        private const int TargetFPS = 60;
        private const double TargetFrameTime = 1.0 / TargetFPS; // ~16.67ms

        // Singleton instance
        private static AnimationController? _instance;
        public static AnimationController Instance => _instance ??= new AnimationController();

        private AnimationController() { }

        /// <summary>
        /// Register an object for animation updates
        /// </summary>
        public void Register(IAnimatable animatable)
        {
            lock (_animatables)
            {
                if (!_animatables.Contains(animatable))
                {
                    _animatables.Add(animatable);
                }
            }
        }

        /// <summary>
        /// Unregister an object from animation updates
        /// </summary>
        public void Unregister(IAnimatable animatable)
        {
            lock (_animatables)
            {
                _animatables.Remove(animatable);
            }
        }

        /// <summary>
        /// Start the animation loop using an optimized Task-based approach
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _stopwatch.Restart();
            _lastTime = 0;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            // Start the game loop on a background thread
            _gameLoopTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested && _isRunning)
                {
                    var frameStart = _stopwatch.Elapsed.TotalSeconds;
                    
                    // Calculate delta time
                    double deltaTime = frameStart - _lastTime;
                    _lastTime = frameStart;

                    // Clamp deltaTime to prevent huge spikes (e.g., if app was paused/resumed)
                    deltaTime = Math.Min(deltaTime, 0.033); // Max 33ms (30 FPS minimum)

                    // Update all registered objects on the main thread
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        AnimationTick(deltaTime);
                    });

                    // Calculate sleep time to maintain target FPS
                    var frameTime = _stopwatch.Elapsed.TotalSeconds - frameStart;
                    var sleepTime = Math.Max(0, TargetFrameTime - frameTime);
                    
                    if (sleepTime > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(sleepTime), cancellationToken);
                    }
                }
            }, cancellationToken);

            Debug.WriteLine("AnimationController started");
        }

        /// <summary>
        /// Stop the animation loop
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            
            try
            {
                _gameLoopTask?.Wait(TimeSpan.FromSeconds(1)); // Wait up to 1 second for graceful shutdown
            }
            catch (AggregateException)
            {
                // Expected when cancellation token is triggered
            }

            _stopwatch.Stop();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _gameLoopTask = null;

            Debug.WriteLine("AnimationController stopped");
        }

        /// <summary>
        /// Called every animation frame to update all registered animatables
        /// </summary>
        private void AnimationTick(double deltaTime)
        {
            // Create a snapshot of the list to avoid issues if list is modified during iteration
            IAnimatable[] snapshot;
            lock (_animatables)
            {
                snapshot = _animatables.ToArray();
            }

            // Update all registered objects
            foreach (var animatable in snapshot)
            {
                try
                {
                    animatable.OnAnimate(deltaTime);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in OnAnimate: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Clear all registered animatables
        /// </summary>
        public void Clear()
        {
            lock (_animatables)
            {
                _animatables.Clear();
            }
        }
    }

    /// <summary>
    /// Interface for objects that can be animated by the AnimationController
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        /// Called every animation frame with delta time
        /// </summary>
        void OnAnimate(double deltaTime);
    }
}

