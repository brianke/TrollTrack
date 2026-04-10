using Microsoft.Maui.Storage;
using TrollTrack.Configuration;
using TrollTrack.Features.Shared;
using TrollTrack.Services;

namespace TrollTrack.Features.About;

public partial class AboutView : ContentPage
{
    public AboutView()
    {
        InitializeComponent();
        VersionLabel.Text = $"Version {AppInfo.VersionString}";
        CopyrightLabel.Text = $"\u00a9 {DateTime.Now.Year} TrollTrack. All rights reserved.";
    }

    private async void OnImportDatabaseClicked(object? sender, EventArgs e)
    {
        var db = Handler?.MauiContext?.Services.GetService<IDatabaseService>();
        if (db is null)
        {
            await DisplayAlert("Import failed", "Database service is not available. Try again after the app has fully loaded.", "OK");
            return;
        }

        await DatabaseImportFlow.RunAsync(this, db);
    }

    private async void OnExportDatabaseClicked(object? sender, EventArgs e)
    {
        try
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, AppConfig.Constants.DatabaseName);

            if (!File.Exists(dbPath))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    DisplayAlert("Export", $"Database file not found at:\n{dbPath}", "OK"));
                return;
            }

            var fileInfo = new FileInfo(dbPath);
            System.Diagnostics.Debug.WriteLine($"Exporting database: {dbPath} ({fileInfo.Length / 1024.0:F1} KB)");

            var tempPath = Path.Combine(FileSystem.CacheDirectory, "trolltrack_export.db");
            File.Copy(dbPath, tempPath, overwrite: true);

#if ANDROID
            await TrollTrack.Platforms.Android.DatabaseShare.ShareFileAsync(
                tempPath,
                "Save TrollTrack Database",
                "application/octet-stream");
#else
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save TrollTrack Database",
                File = new ShareFile(tempPath)
            });
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export error: {ex}");
            await MainThread.InvokeOnMainThreadAsync(() =>
                DisplayAlert("Export failed", ex.Message, "OK"));
        }
    }
}
