using TrollTrack.Configuration;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models;

namespace TrollTrack.Features.Dashboard
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IWeatherService _weatherService;

        #region Observable Properties

        [ObservableProperty]
        private bool isInitializing = true;


        #region WeatherProperties

        [ObservableProperty]
        private WeatherData weatherData = new();

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

            // Load data when ViewModel is created
            _ = InitializeAsync();
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
            //await ExecuteSafelyAsync(async () =>
            //{
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
            //}, "Initializing dashboard...", showErrorAlert: false);
        }

        #endregion

        #region Location Commands

        [RelayCommand]
        private async Task RefreshDashboard()
        {
            await ExecuteSafelyAsync(async () =>
            {
                RefreshStatus = "Refreshing dashboard...";

                // Don't call UpdateLocationAsync directly since it manages its own busy state
                // Instead, call the core logic without the busy state management
                if (!HasLocationPermission)
                {
                    await RequestLocationPermissionAsync();
                    if (!HasLocationPermission)
                    {
                        RefreshStatus = "Location permission required";
                        return;
                    }
                }

                var location = await _locationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    CurrentLatitude = Math.Round(location.Latitude, 6);
                    CurrentLongitude = Math.Round(location.Longitude, 6);

                    // Load weather with new location
                    if (IsWeatherApiConfigured && (CurrentLatitude != 0 && CurrentLongitude != 0))
                    {
                        await LoadWeatherDataAsync();
                    }

                    RefreshStatus = "Dashboard updated";
                    UpdateLastUpdatedTime();
                }
                else
                {
                    RefreshStatus = "Unable to get location";
                }
            }, "Refreshing dashboard...", showErrorAlert: true);
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
                        WeatherData.WeatherEntity = weather;
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

        #region AI Recommendations
        /*
                [RelayCommand]
                private async Task RefreshRecommendationsAsync()
                {
                    try
                    {
                        Debug.WriteLine("Refreshing AI recommendations...");

                        // Use default values if weather data is not available
                        var weather = WeatherData ?? new WeatherData
                        {
                            Temperature = 70,
                            WindSpeed = 5,
                            WindDirection = "N"
                        };

                        var recommendations = await _aiService.GetRecommendationsAsync(weather, 20, 2.5);

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            AIRecommendations.Clear();
                            foreach (var recommendation in recommendations)
                            {
                                AIRecommendations.Add(recommendation);
                            }
                            Debug.WriteLine($"Added {recommendations.Count} recommendations");
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"AI recommendations error: {ex.Message}");
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            AIRecommendations.Clear();
                            AIRecommendations.Add("Unable to load recommendations");
                        });
                    }
                }
        */
        #endregion

        #region Dashboard Data
        /*
                private async Task LoadDashboardDataAsync()
                {
                    try
                    {
                        Debug.WriteLine("Loading dashboard data...");

                        var today = DateTime.Today;
                        var catches = await _databaseService.GetCatchRecordsAsync();
                        var todayCatches = catches.Where(c => c.CatchDateTime.Date == today).ToList();
                        var recentCatches = catches.OrderByDescending(c => c.CatchDateTime).Take(5).ToList();

                        // Get active trolling method
                        var trollingMethods = await _databaseService.GetTrollingMethodsAsync();
                        var activeMethod = trollingMethods.FirstOrDefault(t => t.IsActive);

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            TodaysCatches = todayCatches.Count;

                            // Update recent catches
                            RecentCatches.Clear();
                            foreach (var catchRecord in recentCatches)
                            {
                                RecentCatches.Add(catchRecord);
                            }

                            // Calculate best catch and fishing time
                            if (todayCatches.Any())
                            {
                                var best = todayCatches.OrderByDescending(c => c.FishWeight).First();
                                BestCatch = $"{best.FishSpecies} - {best.FishWeight:F1} lbs";

                                var firstCatch = todayCatches.OrderBy(c => c.CatchDateTime).First().CatchDateTime;
                                var lastCatch = todayCatches.OrderByDescending(c => c.CatchDateTime).First().CatchDateTime;
                                var timeSpan = lastCatch - firstCatch;
                                TotalFishingTime = $"{timeSpan.Hours}h {timeSpan.Minutes}m";
                            }
                            else
                            {
                                BestCatch = "No catches today";
                                TotalFishingTime = "0h 0m";
                            }

                            // Update current trolling method
                            CurrentTrollingMethod = activeMethod?.Name ?? "None active";

                            Debug.WriteLine($"Dashboard data loaded: {TodaysCatches} catches today");
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Dashboard data loading error: {ex.Message}");
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            TodaysCatches = 0;
                            BestCatch = "Error loading data";
                            TotalFishingTime = "N/A";
                            CurrentTrollingMethod = "Error";
                        });
                    }
                }

                [RelayCommand]
                private async Task RefreshDashboardAsync()
                {
                    if (IsBusy) return;

                    try
                    {
                        IsBusy = true;
                        await LoadDashboardDataAsync();
                        await UpdateLocationAndWeatherAsync();
                        await RefreshRecommendationsAsync();

                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            UpdateLastUpdatedTime();
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Dashboard refresh error: {ex.Message}");
                        await ShowAlertAsync("Error", "Failed to refresh dashboard data.");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
        */
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


        #region Public Methods for External Updates
       
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}