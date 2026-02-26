using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

/// <summary>
/// Popup displaying detailed information for a single catch
/// </summary>
public partial class CatchDetailPopup : ContentPage
{
    public CatchDetailPopup(CatchDataEntity catchData)
    {
        InitializeComponent();
        BindingContext = catchData;
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
}
