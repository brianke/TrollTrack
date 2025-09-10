using TrollTrack.Configuration;

namespace TrollTrack.MVVM.ViewModels
{
    public partial class CatchesViewModel : BaseViewModel
    {
        #region Constructor

        public CatchesViewModel(ILocationService locationService) : base(locationService)
        {
            Title = "Catches";

            // Load data when ViewModel is created
            _ = InitializeAsync();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the data needed for the dashboard (Location, Weather, etc.)
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                Debug.WriteLine("Starting dashboard initialization...");
                IsInitializing = true;

                // Set default location first
                CurrentLatitude = AppConfig.Constants.DefaultLatitude;
                CurrentLongitude = AppConfig.Constants.DefaultLongitude;

                // Try to get actual location
                await UpdateLocationAsync();

                // Update Title
                Title = "Dashboard";
            }, "Initializing dashboard...", showErrorAlert: false);
        }

        #endregion

    }
}