namespace RaindropFall.Pages;

public partial class Level1Page : ContentPage
{
    public Level1Page()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Game.Initial(LevelCatalog.Level1, this);
    }

    protected override void OnDisappearing()
    {
        Game.Stop();
        base.OnDisappearing();
    }
}
