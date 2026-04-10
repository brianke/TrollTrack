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

        var allLocations = new List<Location>();

        allLocations.AddRange(display.RoutePoints
            .Select(rp => new Location(rp.Latitude, rp.Longitude)));

        allLocations.AddRange(display.Catches
            .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
            .Select(c => new Location(c.Latitude!.Value, c.Longitude!.Value)));

        if (allLocations.Count > 0)
        {
            MoveMapToFitPins(allLocations);
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
        CatchesMap.MapElements.Clear();

        // Draw route as gradient segments: green → blue → red
        if (display.RoutePoints.Count >= 2)
        {
            var points = display.RoutePoints;
            int segmentCount = points.Count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                double progress = segmentCount == 1 ? 0.0 : (double)i / (segmentCount - 1);
                var segment = new Microsoft.Maui.Controls.Maps.Polyline
                {
                    StrokeColor = InterpolateRouteColor(progress),
                    StrokeWidth = 4
                };
                segment.Geopath.Add(new Location(points[i].Latitude, points[i].Longitude));
                segment.Geopath.Add(new Location(points[i + 1].Latitude, points[i + 1].Longitude));
                CatchesMap.MapElements.Add(segment);
            }
        }

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

        // Collect all points (route + catches) to fit the map bounds
        var allLocations = new List<Location>();

        allLocations.AddRange(display.RoutePoints
            .Select(rp => new Location(rp.Latitude, rp.Longitude)));

        allLocations.AddRange(catchesWithLocation
            .Select(c => new Location(c.Latitude!.Value, c.Longitude!.Value)));

        if (allLocations.Count > 0)
        {
            MoveMapToFitPins(allLocations);
        }
        else
        {
            var defaultLocation = new Location(45.0, -95.0);
            CatchesMap.MoveToRegion(MapSpan.FromCenterAndRadius(defaultLocation, Distance.FromKilometers(500)));
        }

        _ = RefreshMapRegionAfterLayoutAsync();
    }

    /// <summary>
    /// Green (0.0) → Blue (0.5) → Red (1.0)
    /// </summary>
    private static Color InterpolateRouteColor(double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);

        double r, g, b;
        if (t < 0.5)
        {
            double local = t / 0.5;
            r = 0;
            g = 1.0 - local;
            b = local;
        }
        else
        {
            double local = (t - 0.5) / 0.5;
            r = local;
            g = 0;
            b = 1.0 - local;
        }

        return Color.FromRgb(r, g, b);
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
            var detailPopup = new CatchDetailPopup(catchData, _databaseService, OnCatchDeletedFromDetailAsync, OnCatchUpdatedFromDetailAsync);
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

    private async Task OnCatchUpdatedFromDetailAsync(CatchDataEntity updated)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (BindingContext is TripCatchesDisplay display)
            {
                var existing = display.Catches.FirstOrDefault(c => c.Id == updated.Id);
                if (existing != null)
                {
                    var index = display.Catches.IndexOf(existing);
                    display.Catches[index] = updated;
                }
            }
        });
    }
}
