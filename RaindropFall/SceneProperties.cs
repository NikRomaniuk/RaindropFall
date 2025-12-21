namespace RaindropFall
{
    public static class SceneProperties
    {
        // Actual screen dimensions
        public static double Height;
        public static double Width;

        // Virtual Game Area dimensions
        public static double GameHeight;
        public static double GameWidth;

        // Target Aspect Ratio (9/16 for Portriat)
        // Changed to 10/16 because 9/16 looks terrible on PC
        public const double TARGET_W_UNITS = 10.0;
        public const double TARGET_H_UNITS = 16.0;
        // Scale factor for different Screen resolutions
        public static double ScaleFactor { get; private set; }

        public static void SetGameAreaDimensions()
        {
            // Calculate the largest possible scale factor
            // This ensures that GameArea fits entirely within the actual screen bounds
            double ScaleFactor = Math.Min(
                Width / TARGET_W_UNITS,
                Height / TARGET_H_UNITS
            );

            // Apply the scale factor to the target units to get the final pixel dimensions
            GameWidth = TARGET_W_UNITS * ScaleFactor;
            GameHeight = TARGET_H_UNITS * ScaleFactor;
        }

        // --- Helpers ---
        /// <summary>
        /// Size => Pixels
        /// </summary>
        public static double PxFromWidthPercent(double percent)
            => GameWidth * (percent / 100.0);

        /// <summary>
        /// % of the overall Scene size
        /// </summary>
        public static double ProportionalFromWidthPercent(double percent)
            => (PxFromWidthPercent(percent) / GameWidth);
    }
}
