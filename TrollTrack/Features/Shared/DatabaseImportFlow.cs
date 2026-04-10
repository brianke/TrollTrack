using Microsoft.Maui.Storage;
using TrollTrack.Services;

namespace TrollTrack.Features.Shared;

/// <summary>Shared pick-copy-validate-replace flow for importing <c>trolltrack.db</c> from the file system.</summary>
public static class DatabaseImportFlow
{
    public static readonly FilePickerFileType SqliteDbPickerTypes = new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        { DevicePlatform.iOS, new[] { "public.data", "public.item" } },
        { DevicePlatform.Android, new[] { "application/octet-stream", "application/x-sqlite3", "*/*" } },
        { DevicePlatform.WinUI, new[] { ".db" } },
        { DevicePlatform.Tizen, new[] { "*/*" } },
        { DevicePlatform.MacCatalyst, new[] { "db", "sqlite" } }
    });

    /// <summary>Uses <see cref="Shell.Current"/>'s page for dialogs and picker.</summary>
    public static Task RunAsync(IDatabaseService databaseService)
    {
        var page = Shell.Current?.CurrentPage;
        if (page is null)
        {
            System.Diagnostics.Debug.WriteLine("DatabaseImportFlow: Shell.Current.CurrentPage is null.");
            return Task.CompletedTask;
        }

        return RunAsync(page, databaseService);
    }

    public static async Task RunAsync(Page page, IDatabaseService databaseService)
    {
        var confirm = await page.DisplayAlert(
            "Import database",
            "Importing will replace your current TrollTrack data with the selected file. Trips, catches, and settings stored in this app will be overwritten. This cannot be undone.\n\nContinue?",
            "Import",
            "Cancel");
        if (!confirm)
            return;

        try
        {
            var pick = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select TrollTrack database (.db)",
                FileTypes = SqliteDbPickerTypes
            });
            if (pick is null)
                return;

            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"trolltrack_import_{Guid.NewGuid():N}.db");
            await using (var src = await pick.OpenReadAsync())
            await using (var dest = File.Create(tempPath))
                await src.CopyToAsync(dest);

            if (!LooksLikeSqliteFile(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* ignore */ }
                await page.DisplayAlert("Import failed", "The selected file does not look like a SQLite database.", "OK");
                return;
            }

            await databaseService.ImportDatabaseFromFileAsync(tempPath);
            try { File.Delete(tempPath); } catch { /* ignore */ }

            await page.DisplayAlert(
                "Import complete",
                "Your database was replaced. Fully close and reopen the app so every screen loads the imported data.",
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Import error: {ex}");
            await page.DisplayAlert("Import failed", ex.Message, "OK");
        }
    }

    private static bool LooksLikeSqliteFile(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            Span<byte> buf = stackalloc byte[16];
            if (fs.Read(buf) < 16)
                return false;
            return System.Text.Encoding.ASCII.GetString(buf[..15]) == "SQLite format 3";
        }
        catch
        {
            return false;
        }
    }
}
