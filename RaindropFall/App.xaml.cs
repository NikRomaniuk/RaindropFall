namespace RaindropFall
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            GlobalEvents.InitializeTimer();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}