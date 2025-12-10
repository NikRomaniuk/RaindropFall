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
        protected GameObject(double initialX, double initialY, double size)
        {
            X = initialX;
            Y = initialY;
            Size = size;
        }

        protected virtual void UpdateUI()
        {
            AbsoluteLayout.SetLayoutBounds(Visual, new Rect(X, Y, Size, Size));
            AbsoluteLayout.SetLayoutFlags(Visual, AbsoluteLayoutFlags.PositionProportional);
        }

        /// <summary>
        /// Update method
        /// </summary>
        public abstract bool Update(double deltaTime);
    }
}
