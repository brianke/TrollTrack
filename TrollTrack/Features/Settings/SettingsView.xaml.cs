namespace TrollTrack.Features.Settings;

public partial class SettingsView : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = InitializeWhenAppearingAsync();
    }

    private async Task InitializeWhenAppearingAsync()
    {
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Settings ViewModel: {ex.Message}");
            try
            {
                await DisplayAlert("Error", "Failed to load settings data. Please try again.", "OK");
            }
            catch (Exception alertEx)
            {
                // Page may have been navigated away before alert could show
                System.Diagnostics.Debug.WriteLine($"Error showing alert: {alertEx.Message}");
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // No additional cleanup needed here
    }

    private async void OnExportDatabaseClicked(object sender, EventArgs e)
    {
        try
        {
#if ANDROID
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trolltrack.db");

            if (!File.Exists(dbPath))
            {
                await DisplayAlert("Error", "Database not found", "OK");
                return;
            }

            // Copy to a shareable location
            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"trolltrack_export.db");
            File.Copy(dbPath, tempPath, overwrite: true);

            // Share the file - this will let you save it anywhere, email it, etc.
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save TrollTrack Database",
                File = new ShareFile(tempPath)
            });
#else
        await DisplayAlert("Info", "Export only available on Android", "OK");
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            Debug.WriteLine($"Export error: {ex}");
        }
    }

}
