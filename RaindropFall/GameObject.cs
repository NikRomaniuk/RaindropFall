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

            Visual.WidthRequest = sizePx;
            Visual.HeightRequest = sizePx;

            // Sets the object position
            // based on its constantly updating proportional coordinates and its fixed size
            AbsoluteLayout.SetLayoutBounds(Visual, new Rect(X, Y, sizePx, sizePx));
            // Tells the AbsoluteLayout container how to interpret object position
            AbsoluteLayout.SetLayoutFlags(Visual, AbsoluteLayoutFlags.PositionProportional);
        }

        /// <summary>
        /// Update method
        /// </summary>
        public abstract bool Update(double deltaTime);
    }
}
