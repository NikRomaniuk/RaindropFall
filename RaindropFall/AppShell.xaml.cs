using RaindropFall.Pages;

namespace RaindropFall;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Level1Page), typeof(Level1Page));
        Routing.RegisterRoute(nameof(Level2Page), typeof(Level2Page));
    }
}
