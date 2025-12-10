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

        // Target Aspect Ratio (16/9 for portriat)
        // Landscape is a bit cursed, but it's not targeted orientation anyway
        public const double TARGET_ASPECT_RATIO = 16.0 / 9.0;

        public static void SetGameAreaDimensions()
        {
            double actualAspectRatio = Width / Height;

            if (actualAspectRatio < TARGET_ASPECT_RATIO)
            {
                // Screen is taller than target
                GameWidth = Width;
                GameHeight = Width * (1.0 / TARGET_ASPECT_RATIO);
            }
            else
            {
                // Screen is wider than target
                GameWidth = Height;
                GameHeight = Height * TARGET_ASPECT_RATIO;
            }
        }
    }
}
