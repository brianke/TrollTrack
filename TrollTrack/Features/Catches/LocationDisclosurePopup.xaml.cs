namespace TrollTrack.Features.Catches;

public partial class LocationDisclosurePopup : ContentPage
{
    private readonly TaskCompletionSource<bool> _result =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private bool _closed;

    public LocationDisclosurePopup()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Awaits true if the user tapped Continue, false if Cancel or back.
    /// </summary>
    public Task<bool> WaitForResultAsync() => _result.Task;

    private async void OnContinueClicked(object? sender, EventArgs e)
    {
        await CloseAsync(accepted: true);
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await CloseAsync(accepted: false);
    }

    private async Task CloseAsync(bool accepted)
    {
        if (_closed)
        {
            return;
        }

        _closed = true;
        _result.TrySetResult(accepted);

        try
        {
            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PopModalAsync failed: {ex.Message}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync(false);
        return true;
    }
}
