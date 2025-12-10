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

        // Target Aspect Ratio (16/9 for Portriat)
        // Landscape is a bit cursed, but it's not targeted orientation anyway
        public const double TARGET_W_UNITS = 9.0;
        public const double TARGET_H_UNITS = 16.0;

        public static void SetGameAreaDimensions()
        {
            // Calculate the largest possible scale factor
            // This ensures that GameArea fits entirely within the actual screen bounds
            double scaleFactor = Math.Min(
                Width / TARGET_W_UNITS,
                Height / TARGET_H_UNITS
            );

            // Apply the scale factor to the target units to get the final pixel dimensions
            GameWidth = TARGET_W_UNITS * scaleFactor;
            GameHeight = TARGET_H_UNITS * scaleFactor;
        }
    }
}
