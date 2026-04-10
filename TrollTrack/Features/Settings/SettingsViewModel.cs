using TrollTrack.Features.Shared;

namespace TrollTrack.Features.Settings;

public partial class SettingsViewModel : BaseViewModel
{
    #region Observable Properties

    [ObservableProperty]
    private bool isSettingsVisible = false;

    [ObservableProperty]
    private string databaseInfo = "Loading...";

    #endregion

    #region Constructor

    public SettingsViewModel(ILocationService locationService, IDatabaseService databaseService)
        : base(locationService, databaseService)
    {
        Title = "Settings";
        IsSettingsVisible = false;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        await LoadDatabaseInfoAsync();
        IsSettingsVisible = true;
    }

    [RelayCommand]
    private void CloseSettings()
    {
        IsSettingsVisible = false;
    }

    [RelayCommand]
    private async Task ClearAllDataAsync()
    {
        var confirmed = await ShowConfirmationAsync(
            "Clear All Data",
            "This will permanently delete all catches and data. This cannot be undone.",
            "Delete Everything",
            "Cancel"
        );

        if (confirmed)
        {
            try
            {
                await ExecuteSafelyAsync(async () =>
                {
                    await BaseDatabaseService.ClearAllTablesAsync();
                    await LoadDatabaseInfoAsync();
                    await ShowAlertAsync("Success", "All data has been cleared.");
                }, "Clearing data...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SettingsViewModel ClearAllDataAsync() failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private Task ImportDatabaseAsync() => DatabaseImportFlow.RunAsync(BaseDatabaseService);

    [RelayCommand]
    private async Task ExportDatabaseAsync()
    {
        try
        {
//#if ANDROID
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "trolltrack.db");

            if (!File.Exists(dbPath))
            {
                //await DisplayAlert("Error", "Database not found", "OK");
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
//#else
//        await DisplayAlert("Info", "Export only available on Android", "OK");
//#endif
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Error", ex.Message, "OK");
            Debug.WriteLine($"Export error: {ex}");
        }
    }


    [RelayCommand]
    private async Task ViewDatabasePathAsync()
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, "trolltrack.db");
        await ShowAlertAsync("Database Location", path);
    }

    private async Task LoadDatabaseInfoAsync()
    {
        var size = await BaseDatabaseService.GetDatabaseSizeAsync();
        var sizeKB = size / 1024.0;
        var stats = await BaseDatabaseService.GetCatchStatisticsAsync();

        DatabaseInfo = $"Database Size: {sizeKB:F2} KB\n" +
                      $"Total Catches: {stats.TotalCatches}\n" +
                      $"Today's Catches: {stats.TodaysCatches}";
    }

    #endregion
}