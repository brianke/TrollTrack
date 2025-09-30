using TrollTrack.Configuration;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models;

namespace TrollTrack.Features.Dashboard
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IWeatherService _weatherService;
        private readonly ISettingsService _settingsService;

        #region Observable Properties

        [ObservableProperty]
        private WeatherData weatherData = new();

        [ObservableProperty]
        private string weatherSummary = "Loading weather...";

        [ObservableProperty]
        private string fishingConditions = "Loading fishing conditions...";

        [ObservableProperty]
        private bool isWeatherApiConfigured;

        [ObservableProperty]
        private string weatherApiStatusMessage = "";

        #endregion

        #region Constructor

        public DashboardViewModel(ILocationService locationService, IDatabaseService databaseService, IWeatherService weatherService, ISettingsService settingsService) : base(locationService, databaseService)
        {
            _weatherService = weatherService;
            _settingsService = settingsService;

            CheckApiConfiguration();

            Title = "Dashboard";
            _ = InitializeAsync();
        }

        private void CheckApiConfiguration()
        {
            IsWeatherApiConfigured = _settingsService.IsWeatherApiConfigured();
            var (isValid, message) = _settingsService.GetWeatherApiKeyStatus();
            WeatherApiStatusMessage = message;
        }

        #endregion

        #region Initialization

        public async Task InitializeAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                Debug.WriteLine("Starting dashboard initialization...");
                IsInitializing = true;

                CurrentLatitude = AppConfig.Constants.DefaultLatitude;
                CurrentLongitude = AppConfig.Constants.DefaultLongitude;
                LocationName = "Default Location (Great Lakes)";

                await GetAndSetLocationAsync(showAlerts: false);
                await LoadWeatherDataAsyncCore();

            }, "Initializing dashboard...", showErrorAlert: false);
            IsInitializing = false;
        }

        #endregion

        #region Data Loading and Refreshing

        [RelayCommand]
        private async Task RefreshDashboard()
        {
            await ExecuteSafelyAsync(async () =>
            {
                if (await GetAndSetLocationAsync(showAlerts: true))
                {
                    await LoadWeatherDataAsyncCore();
                    RefreshStatus = "Dashboard updated";
                }
            }, "Refreshing dashboard...", showErrorAlert: true);
        }

        [RelayCommand]
        private async Task RefreshWeatherAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                if (CurrentLatitude == 0 || CurrentLongitude == 0)
                {
                    await ShowAlertAsync("Location Required", "Please update your location first.");
                    return;
                }
                await LoadWeatherDataAsyncCore();
            }, "Refreshing weather...", showErrorAlert: true);
        }

        private async Task LoadWeatherDataAsyncCore()
        {
            WeatherSummary = "Loading weather...";

            if (!IsWeatherApiConfigured)
            {
                WeatherSummary = "API not configured.";
                return;
            }

            if (CurrentLatitude == 0 && CurrentLongitude == 0)
            {
                WeatherSummary = "Current location not available.";
                return;
            }

            var weather = await _weatherService.GetCurrentWeatherAsync(CurrentLatitude, CurrentLongitude);

            if (weather != null)
            {
                WeatherData.WeatherEntity = weather;
                LocationName = weather.LocationName ?? "Location Unavailable";
                WeatherSummary = "Weather updated";
                Debug.WriteLine($"Weather loaded: {weather.Temperature}°F, {weather.WeatherCondition}");
            }
            else
            {
                WeatherSummary = "Weather data unavailable";
                LocationName = "Unknown location";
            }
        }

        #endregion

        #region Navigation Commands

        private async Task NavigateToAsync(string route)
        {
            await ExecuteSafelyAsync(() => Shell.Current.GoToAsync(route), "Navigating...");
        }

        [RelayCommand]
        private async Task LogCatchAsync() => await NavigateToAsync(RouteConstants.Catches);

        [RelayCommand]
        private async Task ChangeTrollingMethodAsync() => await NavigateToAsync(RouteConstants.Programs);

        [RelayCommand]
        private async Task ViewCatchHistoryAsync() => await NavigateToAsync(RouteConstants.Catches);


        [RelayCommand]
        private async Task ManageLuresAsync() => await NavigateToAsync(RouteConstants.Lures);

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}