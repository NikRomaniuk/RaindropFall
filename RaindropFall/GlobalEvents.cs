using System.Diagnostics;

namespace RaindropFall
{
    /// <summary>
    /// Class that will invoke some global events like "Update"
    /// Uses optimized IDispatcherTimer with Stopwatch for accurate timing
    /// </summary>
    public static class GlobalEvents
    {
        // Define the fixed frame rate
        private const int TargetFPS = 60;
        // Calculate the interval in milliseconds
        private const double IntervalMs = 1000.0 / TargetFPS;

        // How much time passed since last frame (deltaTime)
        // Fixed value
        private const double FixedDeltaTime = 1.0 / TargetFPS;

        // Timer for game loop
        private static IDispatcherTimer? globalTimer;
        
        // Track the last tick time to calculate actual deltaTime (using Stopwatch for better performance)
        private static Stopwatch stopwatch;

        // Global Events that some interactive game objects will subscribe to
        // '?' declares it as a nullable event
        public static event Action<double>? Update;

        // Testing
        static int frames = 0;
        static double timeAccumulator = 0;

        /// <summary>
        /// Initializes and starts the game timer (Once per app load)
        /// Uses IDispatcherTimer optimized for Android performance
        /// </summary>
        public static void InitializeTimer()
        {
            if (globalTimer == null)
            {
                globalTimer = Dispatcher.GetForCurrentThread().CreateTimer();
                globalTimer.Interval = TimeSpan.FromMilliseconds(IntervalMs);
                globalTimer.Tick += OnTick;
                globalTimer.IsRepeating = true;
            }

            // Start the timer only if it's not already running
            if (!globalTimer.IsRunning)
            {
                stopwatch = Stopwatch.StartNew();
                globalTimer.Start();
                #if DEBUG && !ANDROID
                Debug.WriteLine("Global Timer Started");
                #endif
            }
        }

        /// <summary>
        /// Stops the game timer (To pause game in a future)
        /// </summary>
        public static void Stop()
        {
            globalTimer?.Stop();
            #if DEBUG && !ANDROID
            Debug.WriteLine("Global Timer Stopped");
            #endif
        }

        /// <summary>
        /// Currently invokes the global "Update" event
        /// Uses Stopwatch for accurate deltaTime calculation
        /// </summary>
        private static void OnTick(object? sender, EventArgs e)
        {
            // Calculate actual elapsed time since last tick using Stopwatch (more efficient on Android)
            double actualDeltaTime = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            // Clamp deltaTime to prevent huge spikes (e.g., if app was paused/resumed)
            // Maximum deltaTime is 2x the target frame time to handle occasional delays
            double maxDeltaTime = FixedDeltaTime * 2.0;
            double deltaTime = Math.Min(actualDeltaTime, maxDeltaTime);

            // Invoke the global Update event directly (we're already on UI thread)
            Update?.Invoke(deltaTime);

            // --- FPS Counter ---
            // Disabled on Android to avoid performance issues with Debug.WriteLine
            #if DEBUG && !ANDROID
            frames++;
            timeAccumulator += deltaTime;
            if (timeAccumulator >= 1.0)
            {
                Debug.WriteLine($"FPS: {frames}");
                frames = 0;
                timeAccumulator = 0;
            }
            #endif
        }
    }
}
