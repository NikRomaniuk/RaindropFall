using Microsoft.Maui.Layouts;

namespace RaindropFall
{
    public abstract class GameObject
    {
        // UI
        public BoxView Visual { get; set; }
        public bool IsActive { get; set; } = true;

        // Position
        public double X { get; set; }
        public double Y { get; set; }

        // Properties
        public double Size;

        // Cached values to avoid unnecessary UI updates (critical for Android performance)
        private double _lastSizePx = -1;
        private double _lastX = double.NaN;
        private double _lastY = double.NaN;
        private bool _layoutFlagsSet = false;
        private const double PositionEpsilon = 0.0001; // Small threshold for position changes
        private const double SizeEpsilon = 0.1; // Threshold for size changes in pixels

        // Constructor
        protected GameObject(double size)
        {
            Size = size;

            // Initialize the UI
            Visual = new BoxView
            {
                Color = Colors.White
            };
        }

        public virtual void UpdateUI()
        {
            double sizePx = SceneProperties.PxFromWidthPercent(Size);

            // Only update size if it changed (avoids triggering layout recalculation)
            if (Math.Abs(_lastSizePx - sizePx) > SizeEpsilon)
            {
                Visual.WidthRequest = sizePx;
                Visual.HeightRequest = sizePx;
                _lastSizePx = sizePx;
            }

            // Only update position if it changed (avoids triggering expensive layout recalculation)
            // Check for NaN to handle initial setup
            bool positionChanged = double.IsNaN(_lastX) || double.IsNaN(_lastY) ||
                                   Math.Abs(_lastX - X) > PositionEpsilon || 
                                   Math.Abs(_lastY - Y) > PositionEpsilon;
            
            if (positionChanged)
            {
                // Sets the object position
                // based on its constantly updating proportional coordinates and its fixed size
                AbsoluteLayout.SetLayoutBounds(Visual, new Rect(X, Y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                _lastX = X;
                _lastY = Y;
            }

            // Set layout flags only once (they never change)
            if (!_layoutFlagsSet)
            {
                AbsoluteLayout.SetLayoutFlags(Visual, AbsoluteLayoutFlags.PositionProportional);
                _layoutFlagsSet = true;
            }
        }

        /// <summary>
        /// Update method
        /// </summary>
        public abstract bool Update(double deltaTime);
    }
}
