using System.Collections.ObjectModel;
using System.Diagnostics;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class CatchesViewModel : BaseViewModel
{
    #region Observable Properties

    [ObservableProperty]
    private List<string> _fishOptions = [];

    [ObservableProperty]
    private string _selectedFishOption = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CatchDataEntity> _catches = [];

    [ObservableProperty]
    private int _totalCatches;

    [ObservableProperty]
    private int _todaysCatches;

    [ObservableProperty]
    private TripDataEntity? _activeTrip;

    [ObservableProperty]
    private bool _hasActiveTrip;

    #endregion

    #region Constructor

    public CatchesViewModel(ILocationService locationService, IDatabaseService databaseService)
        : base(locationService, databaseService)
    {
        Title = "Catches";
        //_ = InitializeAsync();
    }

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsInitializing = true;
            FishOptions = FishData.GetAllFishNames();

            // Load active trip
            await LoadActiveTripAsync();

            // Load catches for active trip or all catches
            await LoadCatchesAsync();

            IsInitializing = false;
        }, "Initializing catches...");
    }

    #endregion

    #region Commands

    private async Task LoadActiveTripAsync()
    {
        var trip = await _databaseService.GetActiveTripAsync();
        ActiveTrip = trip;
        HasActiveTrip = trip != null;

        if (HasActiveTrip)
        {
            Title = $"Catches - {ActiveTrip?.TripName}";
        }
        else
        {
            Title = "Catches";
        }
    }

    private async Task LoadCatchesAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsLoading = true;

            List<CatchDataEntity> allCatches;

            if (HasActiveTrip && ActiveTrip != null)
            {
                // Load catches only for active trip
                allCatches = await _databaseService.GetCatchesForTripAsync(ActiveTrip.Id);
            }
            else
            {
                // Load all catches
                allCatches = await _databaseService.GetCatchDataAsync();
            }

            var todaysCatchList = allCatches
                .Where(c => c.Timestamp.Date == DateTime.Today)
                .ToList();

            Catches = new ObservableCollection<CatchDataEntity>(allCatches);
            TotalCatches = allCatches.Count;
            TodaysCatches = todaysCatchList.Count;

            Debug.WriteLine($"Loaded {allCatches.Count} catch records");
            IsLoading = false;
        }, "Loading catches...");
    }

    [RelayCommand]
    private async Task AddNewCatchAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFishOption))
        {
            await ShowAlertAsync("No Species Selected", "Please select a fish species before logging a catch.");
            return;
        }

        // Check if there's an active trip
        if (!HasActiveTrip)
        {
            await ShowAlertAsync("No Active Trip",
                "Please start a trip before logging catches. Go to the Trips tab to start a new trip.");
            return;
        }

        await ExecuteSafelyAsync(async () =>
        {
            var currentLocation = await _locationService.GetCurrentLocationAsync();
            var fishInfo = FishData.GetInfo(SelectedFishOption);

            var newCatch = new CatchDataEntity
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Latitude = currentLocation.Latitude,
                Longitude = currentLocation.Longitude,
                FishInfoId = fishInfo.Id,
                TripId = ActiveTrip!.Id  // Set the TripId
            };

            // Save catch
            await _databaseService.SaveCatchAsync(newCatch);

            // Add catch to active trip's collection for UI
            if (ActiveTrip.Catches == null)
            {
                ActiveTrip.Catches = new List<CatchDataEntity>();
            }
            ActiveTrip.Catches.Add(newCatch);

            Catches.Insert(0, newCatch);
            TotalCatches++;
            if (newCatch.Timestamp.Date == DateTime.Today)
            {
                TodaysCatches++;
            }

            Debug.WriteLine($"Added new catch: {SelectedFishOption} at {newCatch.Timestamp}");

            // Clear selection
            SelectedFishOption = string.Empty;
        }, "Adding catch...");
    }

    [RelayCommand]
    private async Task DeleteCatchAsync(CatchDataEntity catchToDelete)
    {
        if (catchToDelete == null)
            return;

        var confirm = await ShowConfirmationAsync(
            "Delete Catch?",
            $"Are you sure you want to delete this catch?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        await ExecuteSafelyAsync(async () =>
        {
            await _databaseService.DeleteCatchAsync(catchToDelete.Id);

            Catches.Remove(catchToDelete);
            TotalCatches--;

            if (catchToDelete.Timestamp.Date == DateTime.Today)
            {
                TodaysCatches--;
            }

            // Update active trip's catch collection if exists
            if (ActiveTrip?.Catches != null)
            {
                ActiveTrip.Catches.Remove(catchToDelete);
            }

            Debug.WriteLine($"Deleted catch: {catchToDelete.Id}");
        }, "Deleting catch...");
    }

    [RelayCommand]
    private async Task RefreshCatchesAsync()
    {
        await LoadActiveTripAsync();
        await LoadCatchesAsync();
    }

    #endregion
}