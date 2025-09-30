using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches
{
    public partial class CatchesViewModel : BaseViewModel
    {
        #region Observable Properties  

        [ObservableProperty]
        private List<string> fishOptions = new();

        [ObservableProperty]
        private string selectedFishOption = string.Empty; // Initialize to avoid CS8618  

        partial void OnSelectedFishOptionChanged(string value)
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
            Debug.WriteLine("Starting catches initialization...");
            IsInitializing = true;

            // Load fish names for ItemPicker  
            FishOptions = FishData.GetAllFishNames();

            // Get current location  
            await UpdateLocationAsync();

            // Load catches when ViewModel is created  
            await LoadCatchesAsync();

            // Update Title  
            Title = "Catches";
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