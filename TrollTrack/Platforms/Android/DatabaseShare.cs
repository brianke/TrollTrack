using Android.Content;
using Microsoft.Maui.ApplicationModel;
using FileProvider = AndroidX.Core.Content.FileProvider;

namespace TrollTrack.Platforms.Android;

/// <summary>
/// Shares a file via ACTION_SEND using a FileProvider content URI (required on API 24+).
/// MAUI Share with a raw cache path often fails silently on newer Android.
/// </summary>
public static class DatabaseShare
{
    public static Task ShareFileAsync(string filePath, string title, string mimeType)
    {
        var activity = Platform.CurrentActivity;
        if (activity == null)
            throw new InvalidOperationException("No active Android activity.");

        var file = new Java.IO.File(filePath);
        if (!file.Exists())
            throw new FileNotFoundException("File not found for sharing.", filePath);

        var authority = $"{activity.PackageName}.fileprovider";
        var uri = FileProvider.GetUriForFile(activity, authority, file);

        var intent = new Intent(Intent.ActionSend);
        intent.SetType(mimeType);
        intent.PutExtra(Intent.ExtraStream, uri);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);

        var chooser = Intent.CreateChooser(intent, title);
        activity.StartActivity(chooser);
        return Task.CompletedTask;
    }
}
