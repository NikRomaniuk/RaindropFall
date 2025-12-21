namespace RaindropFall.Pages;

public partial class Level2Page : ContentPage
{
    public Level2Page()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Game.Initial(LevelCatalog.Level2, this);
    }

    protected override void OnDisappearing()
    {
        Game.Stop();
        base.OnDisappearing();
    }
}
