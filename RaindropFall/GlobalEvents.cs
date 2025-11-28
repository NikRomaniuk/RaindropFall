using System.Timers;

namespace RaindropFall
{
    /// <summary>
    /// Class that will invoke some global events like "Update"
    /// </summary>
    public static class GlobalEvents
    {
        // Define the fixed frame rate: 60 FPS
        private const int TargetFPS = 60;
        // Calculate the interval in milliseconds
        private const double IntervalMs = 1000.0 / TargetFPS;

        // '!' is a null-forgiving operator
        private static System.Timers.Timer globalTimer = null!;

        // Global Events that some interactive game objects will subscribe to
        // '?' declares it as a nullable event
        public static event Action<double>? Update;

        /// <summary>
        /// Initializes and starts the game timer (Once per app load)
        /// </summary>
        public static void InitializeTimer()
        {
            if (globalTimer == null)
            {
                globalTimer = new System.Timers.Timer(IntervalMs);
                globalTimer.Elapsed += OnTick;
                globalTimer.AutoReset = true;
            }

            // Start the timer only if it's not already running
            if (!globalTimer.Enabled)
            {
                globalTimer.Start();
                System.Diagnostics.Debug.WriteLine("Global Timer Started");
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
        private static void OnTick(object sender, ElapsedEventArgs e)
        {
            // IMPORTANT:
            // The Dispatcher ensures that the frame update logic
            // which modifies the position of your BoxView elements
            // executes legally on the thread that controls the screen

            Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
            {
                // Calculate DeltaTime
                double deltaTime = IntervalMs / 1000.0;

                // Invoke the global Update event
                Update?.Invoke(deltaTime);
            });
        }
    }
}
