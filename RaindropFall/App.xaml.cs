namespace RaindropFall
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Animation system is now started when game starts, not globally
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}