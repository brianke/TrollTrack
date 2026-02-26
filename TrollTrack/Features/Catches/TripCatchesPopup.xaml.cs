using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

/// <summary>
/// Popup displaying all catches for a selected trip
/// </summary>
public partial class TripCatchesPopup : ContentPage
{
    public TripCatchesPopup(TripCatchesDisplay display)
    {
        InitializeComponent();
        BindingContext = display;
    }

    private async void OnCatchTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not CatchDataEntity catchData)
            return;

        try
        {
            var detailPopup = new CatchDetailPopup(catchData);
            await Navigation.PushModalAsync(detailPopup);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnCatchTapped failed: {ex.Message}");
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
}
