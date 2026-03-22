namespace TrollTrack.Features.About;

public partial class AboutView : ContentPage
{
    public AboutView()
    {
        InitializeComponent();
        VersionLabel.Text = $"Version {AppInfo.VersionString}";
    }
}
