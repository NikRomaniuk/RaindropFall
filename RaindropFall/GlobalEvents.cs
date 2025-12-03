using System.Diagnostics;

namespace RaindropFall
{
    /// <summary>
    /// Class that will invoke some global events like "Update"
    /// </summary>
    public static class GlobalEvents
    {
        // Define the fixed frame rate
        private const int TargetFPS = 60;
        // Calculate the interval in milliseconds
        private const double IntervalMs = 1000.0 / TargetFPS;

        // '!' is a null-forgiving operator
        // private static System.Timers.Timer globalTimer = null!;
        private static IDispatcherTimer globalTimer;

        // Stopwatch to measure real time passed
        private static Stopwatch gameStopwatch = new Stopwatch();
        private static double lastFrameTime = 0;

        // Global Events that some interactive game objects will subscribe to
        // '?' declares it as a nullable event
        public static event Action<double>? Update;

        static int frames = 0;
        static double timeAccumulator = 0;

        /// <summary>
        /// Initializes and starts the game timer (Once per app load)
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
                gameStopwatch.Start();
                lastFrameTime = gameStopwatch.Elapsed.TotalSeconds;

                globalTimer.Start();
                Debug.WriteLine("Global Timer & Stopwatch Started");
            }
        }

        /// <summary>
        /// Stops the game timer (To pause game in a future)
        /// </summary>
        public static void Stop()
        {
            globalTimer?.Stop();
            gameStopwatch?.Stop();
            Debug.WriteLine("Global Timer Stopped");
        }

        /// <summary>
        /// Currently invokes the global "Update" event
        /// </summary>
        private static void OnTick(object sender, EventArgs e)
        {
            // --- Main Body ---
            // Get current time
            double currentTime = gameStopwatch.Elapsed.TotalSeconds;

            // Calculate how much time passed since last frame (Delta)
            double deltaTime = currentTime - lastFrameTime;
            if (deltaTime > 0.1) deltaTime = 0.1; // Max is 100ms

            // Update lastFrameTime for the next tick
            lastFrameTime = currentTime;

            

            // Invoke the global Update event
            Update?.Invoke(deltaTime);

            // --- FPS Counter ---
            frames++;
            timeAccumulator += deltaTime;
            if (timeAccumulator >= 1.0)
            {
                Debug.WriteLine($"FPS: {frames}");
                frames = 0;
                timeAccumulator = 0;
            }
        }
    }
}
