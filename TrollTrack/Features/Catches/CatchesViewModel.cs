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

    #endregion

    #region Constructor

    public CatchesViewModel(ILocationService locationService, IDatabaseService databaseService)
        : base(locationService, databaseService)
    {
        Title = "Catches";
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsInitializing = true;
            FishOptions = FishData.GetAllFishNames();
            await LoadCatchesAsync();
            IsInitializing = false;
        }, "Initializing catches...");
    }

    #endregion

    #region Commands

    private async Task LoadCatchesAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsLoading = true;
            var allCatches = await _databaseService.GetCatchDataAsync();
            var todaysCatchList = await _databaseService.GetTodaysCatchesAsync();

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

        await ExecuteSafelyAsync(async () =>
        {
            var currentLocation = await _locationService.GetCurrentLocationAsync();
            var fishInfo = FishData.GetInfo(SelectedFishOption);

            var newCatch = new CatchDataEntity
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Location = currentLocation,
                FishInfo = fishInfo,
                FishInfoId = fishInfo.Id
            };

            await _databaseService.SaveCatchAsync(newCatch);

            Catches.Insert(0, newCatch);
            TotalCatches++;
            if (newCatch.Timestamp.Date == DateTime.Today)
            {
                TodaysCatches++;
            }

            Debug.WriteLine($"Added new catch: {SelectedFishOption} at {newCatch.Timestamp}");
        }, "Adding catch...");
    }

    #endregion
}