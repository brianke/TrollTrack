using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using TrollTrack.Features.Shared.Models.Entities;
using TrollTrack.Services;
using Map = Microsoft.Maui.Controls.Maps.Map;
using Pin = Microsoft.Maui.Controls.Maps.Pin;
using PinType = Microsoft.Maui.Controls.Maps.PinType;

namespace TrollTrack.Features.Catches;

/// <summary>
/// Popup displaying all catches for a selected trip
/// </summary>
public partial class TripCatchesPopup : ContentPage
{
    private readonly IDatabaseService _databaseService;
    private readonly Func<Guid, Task>? _onTripDeletedAsync;

    public TripCatchesPopup(TripCatchesDisplay display, IDatabaseService databaseService, Func<Guid, Task>? onTripDeletedAsync = null)
    {
        _databaseService = databaseService;
        _onTripDeletedAsync = onTripDeletedAsync;
        InitializeComponent();
        BindingContext = display;
        Loaded += OnLoaded;
    }

    private async Task RefreshMapRegionAfterLayoutAsync()
    {
        await Task.Delay(250);
        await MainThread.InvokeOnMainThreadAsync(ApplyMapRegionFromBindingContext);
    }

    /// <summary>
    /// Re-centers/zooms the map without clearing pins (same logic as the end of PopulateMapWithCatches).
    /// </summary>
    private void ApplyMapRegionFromBindingContext()
    {
        if (BindingContext is not TripCatchesDisplay display)
            return;

        var catchesWithLocation = display.Catches
            .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
            .ToList();

        if (catchesWithLocation.Count > 0)
        {
            var locations = catchesWithLocation
                .Select(c => new Location(c.Latitude!.Value, c.Longitude!.Value))
                .ToList();
            MoveMapToFitPins(locations);
        }
        else
        {
            var defaultLocation = new Location(45.0, -95.0);
            CatchesMap.MoveToRegion(MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(500)));
        }

        UpdateMapDebugLabel();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;
        PopulateMapWithCatches();
    }

    private void UpdateMapDebugLabel()
    {
        var platform = DeviceInfo.Platform.ToString();
        var pinCount = CatchesMap.Pins.Count;
        MapDebugLabel.Text = $"Map loaded | {platform} | {pinCount} pin(s)";
    }

    private void PopulateMapWithCatches()
    {
        if (BindingContext is not TripCatchesDisplay display)
            return;

        var catchesWithLocation = display.Catches
            .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
            .ToList();

        CatchesMap.Pins.Clear();

        foreach (var catchData in catchesWithLocation)
        {
            var pin = new Pin
            {
                Location = new Location(catchData.Latitude!.Value, catchData.Longitude!.Value),
                Label = catchData.FishName,
                Address = catchData.Timestamp.ToString("g"),
                Type = PinType.Place
            };
            CatchesMap.Pins.Add(pin);
        }

        UpdateMapDebugLabel();

        // Fit map to show all pins (or use default region if none)
        if (catchesWithLocation.Count > 0)
        {
            var locations = catchesWithLocation
                .Select(c => new Location(c.Latitude!.Value, c.Longitude!.Value))
                .ToList();
            MoveMapToFitPins(locations);
        }
        else
        {
            // Default: center of North America with reasonable zoom
            var defaultLocation = new Location(45.0, -95.0);
            CatchesMap.MoveToRegion(MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(500)));
        }

        // Second pass after native map has laid out (tiles + camera often need this on Android).
        _ = RefreshMapRegionAfterLayoutAsync();
    }

    private void MoveMapToFitPins(IReadOnlyList<Location> locations)
    {
        if (locations.Count == 0) return;

        var centerLat = locations.Average(l => l.Latitude);
        var centerLon = locations.Average(l => l.Longitude);
        var center = new Location(centerLat, centerLon);

        var maxDistanceMeters = locations
            .Select(l => center.CalculateDistance(l, DistanceUnits.Kilometers) * 1000)
            .DefaultIfEmpty(500)
            .Max();

        var radiusMeters = Math.Max(maxDistanceMeters * 1.2, 500);
        CatchesMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(radiusMeters)));
    }

    private async void OnCatchTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not CatchDataEntity catchData)
            return;

        try
        {
            var detailPopup = new CatchDetailPopup(catchData, _databaseService, OnCatchDeletedFromDetailAsync);
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

    private async void OnDeleteTripClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not TripCatchesDisplay display || !display.CanDeleteTrip)
            return;

        var confirm = await DisplayAlert(
            "Delete trip?",
            $"Are you sure you want to delete '{display.TripName}' and all of its catches? This cannot be undone.",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        try
        {
            await _databaseService.DeleteTripAsync(display.TripId);
            if (_onTripDeletedAsync != null)
                await _onTripDeletedAsync(display.TripId);
            if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnDeleteTripClicked failed: {ex.Message}");
            await DisplayAlert("Error", $"Could not delete trip: {ex.Message}", "OK");
        }
    }

    private async Task OnCatchDeletedFromDetailAsync(CatchDataEntity deleted)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (BindingContext is TripCatchesDisplay display)
            {
                var item = display.Catches.FirstOrDefault(c => c.Id == deleted.Id);
                if (item != null)
                    display.Catches.Remove(item);
            }

            PopulateMapWithCatches();
        });
    }
}
