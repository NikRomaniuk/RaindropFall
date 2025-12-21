namespace RaindropFall.Pages;

public partial class MainMenuPage : ContentPage
{
    public MainMenuPage()
    {
        InitializeComponent();
    }

    private async void OnLevel1Clicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(Level1Page));

    private async void OnLevel2Clicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(Level2Page));

    private void OnExitClicked(object sender, EventArgs e)
    {
        try
        {
            Application.Current?.Quit();
        }
        catch
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#else
            System.Environment.Exit(0);
#endif
        }
    }
}
