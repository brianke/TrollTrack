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
            await ExecuteSafelyAsync(async () =>
            {
                await _databaseService.ClearAllTablesAsync();
                await LoadDatabaseInfoAsync();
                await ShowAlertAsync("Success", "All data has been cleared.");
            }, "Clearing data...");
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
        var size = await _databaseService.GetDatabaseSizeAsync();
        var sizeKB = size / 1024.0;
        var stats = await _databaseService.GetCatchStatisticsAsync();

        DatabaseInfo = $"Database Size: {sizeKB:F2} KB\n" +
                      $"Total Catches: {stats.TotalCatches}\n" +
                      $"Today's Catches: {stats.TodaysCatches}";
    }

    #endregion
}