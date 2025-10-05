namespace TrollTrack.Features.Catches;

public partial class CatchesView : ContentPage
{
    private readonly CatchesViewModel _viewModel;

    public CatchesView(CatchesViewModel viewModel)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
        _viewModel = viewModel;
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
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Initialize the ViewModel when the page appears
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Dashboard ViewModel: {ex.Message}");
            // Optionally show error message to user
            await DisplayAlert("Error", "Failed to load dashboard data. Please try again.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // The ViewModel will handle its own cleanup through BaseViewModel's Dispose
        // No additional cleanup needed here
    }
}