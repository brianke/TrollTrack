using TrollTrack.Features.Shared.Models.Entities;
using TrollTrack.Services;

namespace TrollTrack.Features.Catches;

/// <summary>
/// Popup displaying detailed information for a single catch
/// </summary>
public partial class CatchDetailPopup : ContentPage
{
    private readonly CatchDataEntity _catch;
    private readonly IDatabaseService _databaseService;
    private readonly Func<CatchDataEntity, Task>? _afterDeletedAsync;

    public CatchDetailPopup(
        CatchDataEntity catchData,
        IDatabaseService databaseService,
        Func<CatchDataEntity, Task>? afterDeletedAsync = null)
    {
        _catch = catchData;
        _databaseService = databaseService;
        _afterDeletedAsync = afterDeletedAsync;
        InitializeComponent();
        BindingContext = catchData;
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Delete catch?",
            "Are you sure you want to delete this catch? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        try
        {
            await _databaseService.DeleteCatchAsync(_catch.Id);
            if (_afterDeletedAsync != null)
                await _afterDeletedAsync(_catch);
            await CloseAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete catch: {ex.Message}", "OK");
        }
    }

    private async Task CloseAsync()
    {
        if (Navigation.ModalStack.Count > 0)
        {
            await Navigation.PopModalAsync();
        }
    }
}
