using System.Timers;

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
        private static IDispatcherTimer testTimer;

        // Global Events that some interactive game objects will subscribe to
        // '?' declares it as a nullable event
        public static event Action<double>? Update;

        static int seconds = 0;
        static int frames = 0;

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
                globalTimer.Start();
                System.Diagnostics.Debug.WriteLine("Global Timer Started");
            }

            if (testTimer == null)
            {
                testTimer = Dispatcher.GetForCurrentThread().CreateTimer();
                testTimer.Interval = TimeSpan.FromSeconds(1);
                testTimer.Tick += Test;
                testTimer.IsRepeating = true;
            }

            // Start the timer only if it's not already running
            if (!testTimer.IsRunning)
            {
                testTimer.Start();
                System.Diagnostics.Debug.WriteLine("test Timer Started");
            }
        }

        /// <summary>
        /// Stops the game timer (To pause game in a future)
        /// </summary>
        public static void Stop()
        {
            globalTimer?.Stop();
            System.Diagnostics.Debug.WriteLine("Global Timer Stopped");
        }

        /// <summary>
        /// Currently invokes the global "Update" event
        /// </summary>
        private static void OnTick(object sender, EventArgs e)
        {
            // Calculate DeltaTime
            double deltaTime = 1.0 / TargetFPS;

            // Invoke the global Update event
            Update?.Invoke(deltaTime);
            frames++;
        }

        private static void Test(object sender, EventArgs e)
        {
            seconds++;

            System.Diagnostics.Debug.WriteLine("Frames: " + frames);
            System.Diagnostics.Debug.WriteLine("Seconds: " + seconds);

            frames = 0;
        }
    }
}
