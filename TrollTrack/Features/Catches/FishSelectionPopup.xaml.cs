namespace TrollTrack.Features.Catches;

public partial class FishSelectionPopup : ContentPage
{
    private readonly TaskCompletionSource<string?> _tcs = new();

    public Task<string?> ResultTask => _tcs.Task;

    public FishSelectionPopup(List<string> fishOptions)
    {
        InitializeComponent();
        FishList.ItemsSource = fishOptions;
    }

    private async void OnFishSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is string selected)
        {
            _tcs.TrySetResult(selected);
            await Navigation.PopModalAsync();
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        await Navigation.PopModalAsync();
    }
}
