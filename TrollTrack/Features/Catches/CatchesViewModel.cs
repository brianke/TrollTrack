using TrollTrack.Features.Shared.Models;
using TrollTrack.Features.Shared.Models.Entities;
using TrollTrack.Fetures.Shared;

namespace TrollTrack.Features.Catches
{
    public partial class CatchesViewModel : BaseViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        private List<FishCommonName> fishOptions = new();

        [ObservableProperty]
        private FishCommonName selectedFishOption;

        partial void OnSelectedFishOptionChanged(FishCommonName value)
        {
            // Use the enum value
            Debug.WriteLine($"Selected fish: {value}");
        }

        [ObservableProperty]
        private CatchDataEntity? selectedCatch;

        [ObservableProperty]
        private ObservableCollection<CatchDataEntity> catches = new();

        [ObservableProperty]
        private int totalCatches;

        [ObservableProperty]
        private int todaysCatches;

        #endregion

        #region Constructor

        public CatchesViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            // Load data when ViewModel is created
            _ = InitializeAsync();

        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the data needed for the catches 
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            //await ExecuteSafelyAsync(async () =>
            //{
                Debug.WriteLine("Starting catches initialization...");
                IsInitializing = true;

                // Load fish names for ItemPicker
                FishOptions = Enum.GetValues<FishCommonName>().ToList();

                // Get current location
                await UpdateLocationAsync();

                // Load catches when ViewModel is created
                await LoadCatchesAsync();

                // Update Title
                Title = "Catches";
            //}, "Initializing dashboard...", showErrorAlert: false);
        }

        #endregion

        #region Commands

        //[RelayCommand]
        private async Task LoadCatchesAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                IsLoading = true;

                var allCatches = await _databaseService.GetCatchDataAsync();
                var todaysCatchList = await _databaseService.GetTodaysCatchesAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Catches.Clear();
                    foreach (var catchData in allCatches)
                    {
                        Catches.Add(catchData);
                    }

                    TotalCatches = allCatches.Count;
                    TodaysCatches = todaysCatchList.Count;
                });

                System.Diagnostics.Debug.WriteLine($"Loaded {allCatches.Count} catch records");
            }, "Loading catches...", showErrorAlert: false);
        }
        
        //[RelayCommand]
        //private void AddCatch()
        //{
        //    // Handle button press
        //    catches.Add(new CatchData
        //    {
        //        Id = new Guid(),
        //        Timestamp = DateTime.Now,
        //        CatchProgram = null,
        //        Location = CurrentLocation,
        //        FishCaught = SelectedFishOption
        //    });

        //    // TODO: testing only, updating location
        //    CurrentLocation = new Location(CurrentLocation.Latitude + 0.001, CurrentLocation.Longitude + 0.001);
        //}

        [RelayCommand]
        private async Task AddNewCatchAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                // Get current location
                var currentLocation = await _locationService.GetCurrentLocationAsync();

                var newCatch = new CatchDataEntity
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Location = currentLocation
                };

                // Save to database
                await _databaseService.SaveCatchAsync(newCatch);

                // Add to collection
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Catches.Insert(0, newCatch);
                    TotalCatches++;
                    if (newCatch.Timestamp.Date == DateTime.Today)
                        TodaysCatches++;
                });

                System.Diagnostics.Debug.WriteLine($"Added new catch at {newCatch.Timestamp}");
            }, "Adding catch...");
        }

        #endregion

    }
}