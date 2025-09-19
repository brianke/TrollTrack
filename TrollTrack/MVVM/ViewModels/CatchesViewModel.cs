using TrollTrack.Configuration;
using TrollTrack.MVVM.Models;

namespace TrollTrack.MVVM.ViewModels
{
    public partial class CatchesViewModel : BaseViewModel
    {

        #region Observable Properties

        [ObservableProperty]
        private List<FishCommonName> fishOptions;

        [ObservableProperty]
        private FishCommonName selectedFishOption;

        private ObservableCollection<CatchData> catchDataCollection = new ObservableCollection<CatchData>();

        partial void OnSelectedFishOptionChanged(FishCommonName value)
        {
            // Use the enum value
            Debug.WriteLine($"Selected fish: {value}");
        }
    
        
        #endregion

        #region Constructor

        public CatchesViewModel(ILocationService locationService) : base(locationService)
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
            await ExecuteSafelyAsync(async () =>
            {
                Debug.WriteLine("Starting catches initialization...");
                IsInitializing = true;

                // Load fish names for ItemPicker
                FishOptions = Enum.GetValues<FishCommonName>().ToList();

                // Get current location - ADD THIS
                await UpdateLocationAsync();

                // Update Title
                Title = "Catches";
            }, "Initializing dashboard...", showErrorAlert: false);
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void AddCatch()
        {
            // Handle button press
            catchDataCollection.Add(new CatchData
            {
                Id = new Guid(),
                Timestamp = DateTime.Now,
                CatchProgram = null,
                Location = CurrentLocation,
                FishCaught = SelectedFishOption
            });

            // TODO: testing only, updating location
            CurrentLocation = new Location(CurrentLocation.Latitude + 0.001, CurrentLocation.Longitude + 0.001);
        }

        #endregion

    }
}