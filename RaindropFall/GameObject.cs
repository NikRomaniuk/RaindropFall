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
        public double Size; // Virtual units (100 = GameWidth)

        // Track if layout flags have been set (they never change, so set only once)
        private bool _layoutFlagsSet = false;

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
            double sizePx = SceneProperties.PxFromVirtualUnits(Size);

            Visual.WidthRequest = sizePx;
            Visual.HeightRequest = sizePx;

            // Sets the object position
            // based on its constantly updating proportional coordinates and its fixed size
            AbsoluteLayout.SetLayoutBounds(Visual, new Rect(X, Y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
            
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
