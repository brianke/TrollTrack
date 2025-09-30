using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Shared
{
    /// <summary>
    /// Base class for all ViewModels providing common functionality
    /// </summary>
    public partial class BaseViewModel : ObservableValidator
    {
        #region Private Fields - Services injected via constructor
        protected readonly ILocationService _locationService;
        protected readonly IDatabaseService _databaseService;
        #endregion

        #region Protected Properties - Access services through these
        protected ILocationService LocationService => _locationService;
        protected IDatabaseService DatabaseService => _databaseService;
        #endregion

        #region Properties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string refreshStatus = "Refreshing data...";

        partial void OnIsBusyChanged(bool value)
        {
            IsRefreshing = value;
        }

        /// <summary>
        /// Inverse of IsBusy for binding to UI elements that should be enabled when not busy
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string subtitle = string.Empty;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isInitializing;

        [ObservableProperty]
        private bool isLoading;




        #region Location Properties

        [ObservableProperty]
        private LocationDataEntity currentLocation;

        [ObservableProperty]
        private double currentLatitude;

        [ObservableProperty]
        private double currentLongitude;

        [ObservableProperty]
        private string formattedLatitude = "0° 0' 0\" N";

        [ObservableProperty]
        private string formattedLongitude = "0° 0' 0\" W";

        [ObservableProperty]
        private string locationName = "Unknown Location";

        [ObservableProperty]
        private bool hasLocationPermission;

        [ObservableProperty]
        private DateTime locationLastUpdated;

        [ObservableProperty]
        private string locationLastUpdatedFormatted = "Never Updated";

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Event raised when an error occurs in the ViewModel
        /// </summary>
        public event EventHandler<string> ErrorOccurred;

        /// <summary>
        /// Event raised when the ViewModel starts or stops being busy
        /// </summary>
        public event EventHandler<bool> BusyStateChanged;

        #endregion

        #region Constructor

        public BaseViewModel(ILocationService locationService, IDatabaseService databaseService)
        {
            _locationService = locationService;
            _databaseService = databaseService;

            // Subscribe to location updates
            _locationService.LocationUpdated += async (sender, location) => await OnLocationServiceUpdated(sender, location);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _locationService.LocationUpdated -= async (sender, location) => await OnLocationServiceUpdated(sender, location);
            }
        }

        #endregion

        #region Location Commands

        [RelayCommand]
        public async Task UpdateLocationAsync()
        {
            await ExecuteSafelyAsync(() => GetAndSetLocationAsync(showAlerts: true), "Getting location...");
        }

        protected virtual async Task OnLocationUpdatedAsync(LocationDataEntity location)
        {
            await Task.CompletedTask;
        }

        // Internal method that doesn't set busy state (used by RefreshDashboard)
        protected async Task UpdateLocationInternalAsync()
        {
            try
            {
                await GetAndSetLocationAsync(showAlerts: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Internal location update error: {ex.Message}");
            }
        }

        protected async Task<bool> GetAndSetLocationAsync(bool showAlerts)
        {
            if (!HasLocationPermission)
            {
                await RequestLocationPermissionAsync();
                if (!HasLocationPermission)
                {
                    RefreshStatus = "Location permission denied";
                    if (showAlerts)
                    {
                        await ShowAlertAsync("Permission Required", "Location permission is required for this app to work properly.");
                    }
                    return false;
                }
            }

            Debug.WriteLine("Requesting location from GPS...");
            var location = await _locationService.GetCurrentLocationAsync();

            if (location != null)
            {
                Debug.WriteLine($"Location received: {location.Latitude}, {location.Longitude}");

                // Update location properties on main thread
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    CurrentLocation = location;
                    CurrentLatitude = Math.Round(location.Latitude, 6);
                    CurrentLongitude = Math.Round(location.Longitude, 6);
                    UpdateLastUpdatedTime(); // This also updates formatted time
                    RefreshStatus = $"Location updated at {DateTime.Now:HH:mm:ss}";
                });
                return true;
            }
            else
            {
                Debug.WriteLine("Failed to get location");
                RefreshStatus = "Unable to get location";
                if (showAlerts)
                {
                    await ShowAlertAsync("Location Error", "Unable to get your current location. Please check that location services are enabled.");
                }
                return false;
            }
        }

        public async Task RequestLocationPermissionAsync()
        {
            try
            {
                var hasPermission = await _locationService.RequestLocationPermissionAsync();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HasLocationPermission = hasPermission;
                    if (!hasPermission)
                    {
                        RefreshStatus = "Location permission denied";
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Permission request error: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    HasLocationPermission = false;
                    RefreshStatus = "Location permission error";
                });
            }
        }

        #endregion

        #region Event Handlers

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsBusy):
                    BusyStateChanged?.Invoke(this, IsBusy);
                    break;
                case nameof(ErrorMessage):
                    if (!string.IsNullOrEmpty(ErrorMessage))
                    {
                        ErrorOccurred?.Invoke(this, ErrorMessage);
                    }
                    break;
            }
        }

        #region Property Change Handlers

        partial void OnCurrentLatitudeChanged(double value)
        {
            if (value != 0)
            {
                FormattedLatitude = ConvertToDegreesMinutesSeconds(value, true);
                RefreshStatus = "Location updated";
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

        #endregion

        private async Task OnLocationServiceUpdated(object sender, LocationDataEntity location)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    CurrentLocation = location;
                    await OnLocationUpdatedAsync(location);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling location update: {ex.Message}");
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Sets the busy state and optionally updates the title
        /// </summary>
        /// <param name="busy">Whether the ViewModel is busy</param>
        /// <param name="busyTitle">Optional title to show while busy</param>
        protected virtual void SetBusy(bool busy, string busyTitle = "")
        {
            IsBusy = busy;

            if (busy && !string.IsNullOrEmpty(busyTitle))
            {
                Title = busyTitle;
            }
        }

        /// <summary>
        /// Sets an error state with message
        /// </summary>
        /// <param name="error">The error message</param>
        /// <param name="clearAfter">Time to clear the error (optional)</param>
        protected virtual void SetError(string error, TimeSpan? clearAfter = null)
        {
            HasError = true;
            ErrorMessage = error;

            if (clearAfter.HasValue)
            {
                // Clear error after specified time
                Task.Delay(clearAfter.Value).ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ClearError();
                    });
                });
            }
        }

        /// <summary>
        /// Clears any error state
        /// </summary>
        protected virtual void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Safely executes an async operation with error handling and busy state management
        /// </summary>
        /// <param name="operation">The async operation to execute</param>
        /// <param name="busyMessage">Message to show while busy</param>
        /// <param name="showErrorAlert">Whether to show error alerts to user</param>
        /// <returns>True if operation succeeded, false if it failed</returns>
        protected async Task<bool> ExecuteSafelyAsync(Func<Task> operation, string busyMessage = "", bool showErrorAlert = true)
        {
            if (IsBusy)
                return false;

            try
            {
                ClearError();
                SetBusy(true, busyMessage);

                await operation();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in {GetType().Name}: {ex.Message}");
                SetError(ex.Message, TimeSpan.FromSeconds(5));

                if (showErrorAlert)
                {
                    await ShowAlertAsync("Error", ex.Message);
                }

                return false;
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Safely executes an async operation that returns a value
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">The async operation to execute</param>
        /// <param name="defaultValue">Default value to return on error</param>
        /// <param name="busyMessage">Message to show while busy</param>
        /// <param name="showErrorAlert">Whether to show error alerts to user</param>
        /// <returns>Operation result or default value</returns>
        protected async Task<T> ExecuteSafelyAsync<T>(Func<Task<T>> operation, T defaultValue = default, string busyMessage = "", bool showErrorAlert = true)
        {
            if (IsBusy)
                return defaultValue;

            try
            {
                ClearError();
                SetBusy(true, busyMessage);

                return await operation();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in {GetType().Name}: {ex.Message}");
                SetError(ex.Message, TimeSpan.FromSeconds(5));

                if (showErrorAlert)
                {
                    await ShowAlertAsync("Error", ex.Message);
                }

                return defaultValue;
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Shows an alert to the user.
        /// This can be overridden in derived classes to provide custom alert functionality.
        /// </summary>
        /// <param name="title">The title of the alert.</param>
        /// <param name="message">The message to display in the alert.</param>
        public virtual async Task ShowAlertAsync(string title, string message)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var currentPage = GetCurrentPage();
                    if (currentPage != null)
                    {
                        await currentPage.DisplayAlert(title, message, "OK");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing alert: {ex.Message}");
            }
        }


        /// <summary>
        /// Shows a confirmation dialog to the user
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="accept">Accept button text</param>
        /// <param name="cancel">Cancel button text</param>
        /// <returns>True if user accepted, false if cancelled</returns>
        protected virtual async Task<bool> ShowConfirmationAsync(string title, string message, string accept = "Yes", string cancel = "No")
        {
            try
            {
                var page = GetCurrentPage();
                if (page != null)
                {
                    return await page.DisplayAlert(title, message, accept, cancel);
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show confirmation: {ex.Message}");
                return false;
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
                System.Diagnostics.Debug.WriteLine($"Failed to get current page: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes an action on the main UI thread
        /// </summary>
        /// <param name="action">Action to execute</param>
        protected static void ExecuteOnMainThread(Action action)
        {
            if (MainThread.IsMainThread)
            {
                action();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(action);
            }
        }

        /// <summary>
        /// Executes an async action on the main UI thread
        /// </summary>
        /// <param name="action">Async action to execute</param>
        protected static async Task ExecuteOnMainThreadAsync(Func<Task> action)
        {
            if (MainThread.IsMainThread)
            {
                await action();
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(action);
            }
        }

        #endregion

        #region Helper Methods

        public void UpdateLastUpdatedTime()
        {
            LocationLastUpdated = DateTime.Now;
            LocationLastUpdatedFormatted = $"Last updated: {LocationLastUpdated:HH:mm tt}";
        }
        #endregion

    }
}