namespace TrollTrack;

/// <summary>
/// Displays a loading indicator while initial database seeding completes.
/// </summary>
public class SeedLoadingPage : ContentPage
{
    public SeedLoadingPage()
    {
        BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Color.FromArgb("#1C1C1C") : Color.FromArgb("#F8F8F8");
        Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 20,
            Children =
            {
                new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Color.FromArgb("#512BD4"),
                    WidthRequest = 60,
                    HeightRequest = 60
                },
                new Label
                {
                    Text = "Loading...",
                    FontSize = 18,
                    TextColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (MauiProgram.PendingSeedTask != null)
            {
                await MauiProgram.PendingSeedTask;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Seed loading error: {ex.Message}");
        }
        finally
        {
            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = new AppShell();
            }
        }
    }
}