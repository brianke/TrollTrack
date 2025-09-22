
using TrollTrack.Configuration;
using TrollTrack.MVVM.Models;

namespace TrollTrack.MVVM.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IWeatherService _weatherService;

        #region Observable Properties

        [ObservableProperty]
        private bool isInitializing = true;


        #region WeatherProperties

        [ObservableProperty]
        private WeatherData? weatherData;

        [ObservableProperty]
        private bool isWeatherLoading;

        [ObservableProperty]
        private string weatherSummary = "Loading weather...";

        [ObservableProperty]
        private string fishingConditions = "Loading fishing conditions...";

        [ObservableProperty]
        private bool isWeatherApiConfigured;

        [ObservableProperty]
        private string weatherApiStatusMessage = "";

        #endregion

        #endregion

        #region Constructor

        public DashboardViewModel(ILocationService locationService, IDatabaseService databaseService, IWeatherService weatherService) : base(locationService, databaseService)
        {
            _weatherService = weatherService;

            // Check API configuration status
            CheckApiConfiguration();
        }

        private void CheckApiConfiguration()
        {
            IsWeatherApiConfigured = ConfigurationService.IsWeatherApiConfigured();
            var (isValid, message) = ConfigurationService.GetWeatherApiKeyStatus();
            WeatherApiStatusMessage = message;
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
                LocationName = "Default Location (Great Lakes)";

                // Try to get actual location
                await UpdateLocationAsync();
                await LoadWeatherDataAsync();

                // Update Title
                Title = "Dashboard";
            }, "Initializing dashboard...", showErrorAlert: false);
        }

        #endregion

        #region Weather Commands

        /// <summary>
        /// Refresh weather command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task RefreshWeatherAsync()
        {
            if (CurrentLatitude == 0 || CurrentLongitude == 0)
            {
                await ShowAlertAsync("Location Required", "Please update your location first.");
                return;
            }

            await LoadWeatherDataAsync();
        }

        private async Task LoadWeatherDataAsync()
        {
            try
            {
                IsWeatherLoading = true;
                WeatherSummary = "Loading weather...";

                if (CurrentLatitude != 0 && CurrentLongitude != 0)
                {
                    var weather = await _weatherService.GetCurrentWeatherAsync(CurrentLatitude, CurrentLongitude);

                    if (weather != null)
                    {
                        WeatherData = weather;
                        LocationName = weather.LocationName ?? "Location Unavailable";
                        WeatherSummary = "Weather updated";
                        Debug.WriteLine($"Weather loaded: {weather.Temperature}°F, {weather.WeatherCondition}");
                    }
                    else
                    {
                        WeatherSummary = "Weather unavailable";
                        LocationName = "Unknown location";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Weather loading error: {ex.Message}");
                WeatherSummary = "Weather error";
                LocationName = "Unknown location";
            }
            finally
            {
                IsWeatherLoading = false;
            }
        }

        #endregion

        #region Navigation Commands

        [RelayCommand]
        private async Task LogCatchAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(RouteConstants.Catch);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await ShowAlertAsync("Error", "Unable to navigate to catch logging.");
            }
        }

        [RelayCommand]
        private async Task ChangeTrollingMethodAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(RouteConstants.Trolling);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await ShowAlertAsync("Error", "Unable to navigate to trolling methods.");
            }
        }

        [RelayCommand]
        private async Task ViewCatchHistoryAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(RouteConstants.History);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await ShowAlertAsync("Error", "Unable to navigate to catch history.");
            }
        }

        [RelayCommand]
        private async Task ManageLuresAsync()
        {
            try
            {
                await Shell.Current.GoToAsync(RouteConstants.Lures);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await ShowAlertAsync("Error", "Unable to navigate to lure management.");
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}