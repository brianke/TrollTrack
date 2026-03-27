namespace TrollTrack.Features.About;

public partial class AboutView : ContentPage
{
    public AboutView()
    {
        InitializeComponent();
        VersionLabel.Text = $"Version {AppInfo.VersionString}";
    }

    private async void OnExportDatabaseClicked(object? sender, EventArgs e)
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

            var tempPath = Path.Combine(FileSystem.CacheDirectory, "trolltrack_export.db");
            File.Copy(dbPath, tempPath, overwrite: true);

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
            await DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
        }
    }
}
