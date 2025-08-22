
namespace TrollTrack.MVVM.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly LocationService _locationService;
        //private readonly DatabaseService _databaseService;
        //private readonly WeatherService _weatherService;
        //private readonly AIRecommendationService _aiService;

        #region Observable Properties

        [ObservableProperty]
        private double currentLatitude;

        [ObservableProperty]
        private double currentLongitude;

        [ObservableProperty]
        private string formattedLatitude = "0° 0' 0\" N";

        [ObservableProperty]
        private string formattedLongitude = "0° 0' 0\" W";

        [ObservableProperty]
        private string locationStatus = "Getting location...";

        //[ObservableProperty]
        //private WeatherData weatherData;

        [ObservableProperty]
        private bool isWeatherLoading;

        [ObservableProperty]
        private string weatherStatus = "Loading weather...";

        [ObservableProperty]
        private int todaysCatches;

        [ObservableProperty]
        private string totalFishingTime = "0h 0m";

        [ObservableProperty]
        private string bestCatch = "No catches today";

        [ObservableProperty]
        private string currentTrollingMethod = "None active";

        [ObservableProperty]
        private bool hasLocationPermission;

        [ObservableProperty]
        private DateTime lastUpdated;

        [ObservableProperty]
        private string formattedLastUpdated;

        //[ObservableProperty]
        //private bool isAutoRefreshEnabled = true;

        //[ObservableProperty]
        //private int autoRefreshCountdown = 30;

        [ObservableProperty]
        private bool isInitializing = true;

        #endregion

        #region Collections

        //public ObservableCollection<string> AIRecommendations { get; } = new();
        //public ObservableCollection<CatchRecord> RecentCatches { get; } = new();

        #endregion

        #region Timers
/*
        private IDispatcherTimer _locationRefreshTimer;
        private IDispatcherTimer _countdownTimer;
*/
        #endregion

        #region Constructor

        public DashboardViewModel(
            LocationService locationService
            //DatabaseService databaseService,
            //WeatherService weatherService,
            //AIRecommendationService aiService
        )
        {
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            //_databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            //_weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            //_aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));

            Title = "Dashboard";

            // Initialize weather data to prevent null reference
            //WeatherData = new WeatherData
            //{
            //    Temperature = 0,
            //    WindSpeed = 0,
            //    WindDirection = "N/A",
            //    Conditions = "Loading...",
            //    Sunrise = DateTime.Now,
            //    Sunset = DateTime.Now,
            //    Pressure = 0,
            //    Humidity = 0
            //};

            // Initialize auto-refresh timers
            //InitializeTimers();

            // Start initialization in background
            _ = Task.Run(async () => await InitializeAsync());
        }

        #endregion

        #region Initialization

        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("Starting dashboard initialization...");

                // Check and request location permission
                await RequestLocationPermissionAsync();

                // Get location and weather
                await UpdateLocationAndWeatherAsync();

/*
                // Initialize database first
                await _databaseService.InitializeAsync();
                Debug.WriteLine("Database initialized");

                // Load initial data
                await LoadDashboardDataAsync();


                // Load AI recommendations
                await RefreshRecommendationsAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsInitializing = false;
                    UpdateLastUpdatedTime();
                    StartAutoRefresh(); // Start auto-refresh after initialization
                });
*/
                Debug.WriteLine("Dashboard initialization completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dashboard initialization error: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsInitializing = false;
                    LocationStatus = "Initialization failed";
                    WeatherStatus = "Weather unavailable";
                });
            }
        }

        private async Task RequestLocationPermissionAsync()
        {
            try
            {
                var hasPermission = await _locationService.RequestLocationPermissionAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HasLocationPermission = hasPermission;
                    if (!hasPermission)
                    {
                        LocationStatus = "Location permission denied";
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Permission request error: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HasLocationPermission = false;
                    LocationStatus = "Permission error";
                });
            }
        }

        #endregion

        #region Auto Refresh Timer Management

        /* Not doing the refresh as it will drain battery, but keeping the code should I want to use it in the future
        private void InitializeTimers()
        {
            // Location refresh timer (30 seconds)
            _locationRefreshTimer = Application.Current.Dispatcher.CreateTimer();
            _locationRefreshTimer.Interval = TimeSpan.FromSeconds(30);
            _locationRefreshTimer.Tick += async (sender, e) => await OnLocationRefreshTimerElapsed();
            _locationRefreshTimer.IsRepeating = true;

            // Countdown timer (1 second intervals)
            _countdownTimer = Application.Current.Dispatcher.CreateTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += OnCountdownTimerElapsed;
            _countdownTimer.IsRepeating = true;

            Debug.WriteLine("🔧 Timers initialized");
        }

        private async Task OnLocationRefreshTimerElapsed()
        {
            try
            {
                if (IsAutoRefreshEnabled && !IsBusy && HasLocationPermission)
                {
                    Debug.WriteLine("🔄 Auto-refreshing location...");
                    await UpdateLocationAndWeatherAsync();

                    // Reset countdown
                    AutoRefreshCountdown = 30;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Auto-refresh error: {ex.Message}");
            }
        }

        private void OnCountdownTimerElapsed(object sender, EventArgs e)
        {
            if (IsAutoRefreshEnabled && AutoRefreshCountdown > 0)
            {
                AutoRefreshCountdown--;
                if (AutoRefreshCountdown <= 0)
                {
                    AutoRefreshCountdown = 30; // Reset for next cycle
                }
                Debug.WriteLine($"⏰ Countdown: {AutoRefreshCountdown}");
            }
        }

        [RelayCommand]
        private void ToggleAutoRefresh()
        {
            IsAutoRefreshEnabled = !IsAutoRefreshEnabled;

            Debug.WriteLine($"🔄 Auto-refresh toggled: {IsAutoRefreshEnabled}");

            if (IsAutoRefreshEnabled)
            {
                StartAutoRefresh();
                Debug.WriteLine("✅ Auto-refresh enabled");
            }
            else
            {
                StopAutoRefresh();
                Debug.WriteLine("⏹️ Auto-refresh disabled");
            }
        }

        [RelayCommand]
        private async Task TestTimerAsync()
        {
            Debug.WriteLine("🧪 Testing timer functionality...");
            await ShowAlertAsync("Timer Test", $"Auto-refresh enabled: {IsAutoRefreshEnabled}\nCountdown: {AutoRefreshCountdown}\nLocation timer running: {_locationRefreshTimer?.IsRunning}\nCountdown timer running: {_countdownTimer?.IsRunning}");
        }

        private void StartAutoRefresh()
        {
            if (HasLocationPermission)
            {
                AutoRefreshCountdown = 30;
                _locationRefreshTimer?.Start();
                _countdownTimer?.Start();
                Debug.WriteLine("🔄 Auto-refresh started (30 second intervals)");
                Debug.WriteLine($"🔧 Location timer running: {_locationRefreshTimer?.IsRunning}");
                Debug.WriteLine($"🔧 Countdown timer running: {_countdownTimer?.IsRunning}");
            }
            else
            {
                Debug.WriteLine("⚠️ Cannot start auto-refresh: No location permission");
            }
        }

        private void StopAutoRefresh()
        {
            _locationRefreshTimer?.Stop();
            _countdownTimer?.Stop();
            AutoRefreshCountdown = 30;
            Debug.WriteLine("⏹️ Auto-refresh stopped");
            Debug.WriteLine($"🔧 Location timer running: {_locationRefreshTimer?.IsRunning}");
            Debug.WriteLine($"🔧 Countdown timer running: {_countdownTimer?.IsRunning}");
        }
        */
        #endregion

        #region Location Commands

        [RelayCommand]
        private async Task UpdateLocationAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                LocationStatus = "Getting location...";

                if (!HasLocationPermission)
                {
                    await RequestLocationPermissionAsync();
                    if (!HasLocationPermission)
                    {
                        await ShowAlertAsync("Permission Required",
                            "Location permission is required for this app to work properly.");
                        return;
                    }
                }

                var location = await _locationService.GetCurrentLocationAsync();
                if (location != null)
                {
                    CurrentLatitude = Math.Round(location.Latitude, 6);
                    CurrentLongitude = Math.Round(location.Longitude, 6);
                    LocationStatus = "Location updated";

                    Debug.WriteLine($"Location updated: {CurrentLatitude}, {CurrentLongitude}");

                    //// Reset auto-refresh countdown
                    //if (IsAutoRefreshEnabled)
                    //{
                    //    AutoRefreshCountdown = 30;
                    //}

                    //// Update weather with new location
                    //_ = Task.Run(async () => await LoadWeatherDataAsync());
                }
                else
                {
                    LocationStatus = "Unable to get location";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Location update error: {ex.Message}");
                LocationStatus = "Location error";
                await ShowAlertAsync("Error", $"Failed to get location: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateLocationAndWeatherAsync()
        {
            await UpdateLocationAsync();

            //if (CurrentLatitude != 0 && CurrentLongitude != 0)
            //{
            //    await LoadWeatherDataAsync();
            //}
        }

        #endregion

        #region Weather Commands
/*
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
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsWeatherLoading = true;
                    WeatherStatus = "Loading weather...";
                });

                if (CurrentLatitude != 0 && CurrentLongitude != 0)
                {
                    var weather = await _weatherService.GetWeatherDataAsync(CurrentLatitude, CurrentLongitude);

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (weather != null)
                        {
                            WeatherData = weather;
                            WeatherStatus = "Weather updated";
                            Debug.WriteLine($"Weather loaded: {weather.Temperature}°F, {weather.Conditions}");
                        }
                        else
                        {
                            WeatherStatus = "Weather unavailable";
                        }
                        IsWeatherLoading = false;
                        UpdateLastUpdatedTime();
                    });

                    // Refresh recommendations with new weather data
                    if (weather != null)
                    {
                        _ = Task.Run(async () => await RefreshRecommendationsAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Weather loading error: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsWeatherLoading = false;
                    WeatherStatus = "Weather error";
                });
            }
        }
*/
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
                await Shell.Current.GoToAsync("//catch");
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
                await Shell.Current.GoToAsync("//trolling");
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
                await Shell.Current.GoToAsync("//history");
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
                await Shell.Current.GoToAsync("//lures");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await ShowAlertAsync("Error", "Unable to navigate to lure management.");
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateLastUpdatedTime()
        {
            LastUpdated = DateTime.Now;
            FormattedLastUpdated = $"Last updated: {LastUpdated:HH:mm:ss}";
        }

        private static async Task ShowAlertAsync(string title, string message)
        {
            try
            {
                var page = GetCurrentPage();
                if (page != null)
                {
                    await page.DisplayAlert(title, message, "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Alert display error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current page using the modern .NET MAUI approach
        /// </summary>
        /// <returns>Current page or null if not available</returns>
        private static Page GetCurrentPage()
        {
            try
            {
                // Try to get the current page from Shell first
                if (Shell.Current?.CurrentPage != null)
                {
                    return Shell.Current.CurrentPage;
                }

                // Fall back to the main window's page
                var mainWindow = Application.Current?.Windows?.FirstOrDefault();
                if (mainWindow?.Page != null)
                {
                    return mainWindow.Page;
                }

                // Last resort: try to find any available window with a page
                var windowWithPage = Application.Current?.Windows?.FirstOrDefault(w => w.Page != null);
                return windowWithPage?.Page;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get current page: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Property Change Handlers

        partial void OnCurrentLatitudeChanged(double value)
        {
            if (value != 0)
            {
                FormattedLatitude = ConvertToDegreesMinutesSeconds(value, true);
                LocationStatus = "Location updated";
            }
        }

        partial void OnCurrentLongitudeChanged(double value)
        {
            if (value != 0)
            {
                FormattedLongitude = ConvertToDegreesMinutesSeconds(value, false);
            }
        }

        /// <summary>
        /// Converts decimal degrees to degrees, minutes, seconds format
        /// </summary>
        /// <param name="coordinate">The decimal degree coordinate</param>
        /// <param name="isLatitude">True for latitude (N/S), false for longitude (E/W)</param>
        /// <returns>Formatted coordinate string</returns>
        private static string ConvertToDegreesMinutesSeconds(double coordinate, bool isLatitude)
        {
            if (coordinate == 0) return isLatitude ? "0° 0' 0\" N" : "0° 0' 0\" W";

            // Determine direction
            string direction;
            if (isLatitude)
            {
                direction = coordinate >= 0 ? "N" : "S";
            }
            else
            {
                direction = coordinate >= 0 ? "E" : "W";
            }

            // Work with absolute value
            coordinate = Math.Abs(coordinate);

            // Extract degrees (whole number part)
            int degrees = (int)coordinate;

            // Extract minutes (whole number part of remainder * 60)
            double remainderAfterDegrees = coordinate - degrees;
            int minutes = (int)(remainderAfterDegrees * 60);

            // Extract seconds (remainder after minutes * 60)
            double remainderAfterMinutes = (remainderAfterDegrees * 60) - minutes;
            double seconds = remainderAfterMinutes * 60;

            // Format and return
            return $"{degrees}° {minutes}' {seconds:F1}\" {direction}";
        }
/*
        partial void OnWeatherDataChanged(WeatherData value)
        {
            if (value != null)
            {
                UpdateLastUpdatedTime();
            }
        }
*/
        #endregion

        #region Public Methods for External Updates
/*
        public async Task OnCatchAddedAsync()
        {
            // Called when a new catch is added from another view
            await LoadDashboardDataAsync();
            await RefreshRecommendationsAsync();
        }

        public async Task OnTrollingMethodChangedAsync()
        {
            // Called when trolling method is changed from another view
            await LoadDashboardDataAsync();
        }

        public void PauseAutoRefresh()
        {
            // Called when app goes to background or user navigates away
            StopAutoRefresh();
        }

        public void ResumeAutoRefresh()
        {
            // Called when app comes to foreground or user returns to dashboard
            if (IsAutoRefreshEnabled && HasLocationPermission)
            {
                StartAutoRefresh();
            }
        }
*/
        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //StopAutoRefresh();
                //_locationRefreshTimer = null;
                //_countdownTimer = null;
                //Debug.WriteLine("🗑️ Timers disposed");
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}