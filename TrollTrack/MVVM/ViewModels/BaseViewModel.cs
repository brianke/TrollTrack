using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace TrollTrack.MVVM.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels providing common functionality
    /// </summary>
    public partial class BaseViewModel : ObservableObject
    {
        #region Private Fields - Services injected via constructor
        protected readonly ILocationService _locationService;
        #endregion

        #region Protected Properties - Access services through these
        protected ILocationService LocationService => _locationService;
        #endregion

        #region Properties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool isBusy = false;
        private bool _disposed = false;

        /// <summary>
        /// Inverse of IsBusy for binding to UI elements that should be enabled when not busy
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string subtitle = string.Empty;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isInitializing;

        #region Location Properties

        [ObservableProperty]
        private Location currentLocation;

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

        [ObservableProperty]
        private bool isLocationEnabled;

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

        public BaseViewModel(ILocationService locationService)
        {
            _locationService = locationService;

            // Subscribe to location updates
            _locationService.LocationUpdated += OnLocationServiceUpdated;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _locationService.LocationUpdated -= OnLocationServiceUpdated;
            }
        }


        // Common command that all ViewModels can use
        [RelayCommand]
        public async Task UpdateLocationAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                CurrentLocation = await _locationService.GetCurrentLocationAsync();
                HasLocationPermission = _locationService.IsLocationEnabled;

                if (CurrentLocation != null)
                {
                    await OnLocationUpdatedAsync(CurrentLocation);
                }
            }, "Updating location...");
        }

        // Virtual method that derived ViewModels can override
        protected virtual async Task OnLocationUpdatedAsync(Location location)
        {
            // Override in derived classes for specific behavior
            await Task.CompletedTask;
        }

        // Event handler for service location updates
        private async void OnLocationServiceUpdated(object sender, Location location)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CurrentLocation = location;
                OnLocationUpdatedAsync(location);
            });
        }

        protected async Task<bool> ExecuteSafelyAsync(Func<Task> operation, string busyMessage = null)
        {
            if (IsBusy) return false;

            try
            {
                IsBusy = true;
                await operation();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                IsBusy = false;
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
                    await ShowErrorAlertAsync("Error", ex.Message);
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
                    await ShowErrorAlertAsync("Error", ex.Message);
                }

                return defaultValue;
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Shows an error alert to the user
        /// </summary>
        /// <param name="title">Alert title</param>
        /// <param name="message">Alert message</param>
        protected virtual async Task ShowErrorAlertAsync(string title, string message)
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
                System.Diagnostics.Debug.WriteLine($"Failed to show alert: {ex.Message}");
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

        #region Validation Support

        private readonly Dictionary<string, List<string>> _validationErrors = new();

        /// <summary>
        /// Gets whether the ViewModel has any validation errors
        /// </summary>
        public bool HasValidationErrors => _validationErrors.Any(x => x.Value?.Count > 0);

        /// <summary>
        /// Gets all validation errors as a formatted string
        /// </summary>
        public string ValidationErrorsText =>
            string.Join(Environment.NewLine, _validationErrors.SelectMany(x => x.Value));

        /// <summary>
        /// Adds a validation error for a property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="error">Error message</param>
        protected void AddValidationError(string propertyName, string error)
        {
            if (!_validationErrors.ContainsKey(propertyName))
                _validationErrors[propertyName] = new List<string>();

            if (!_validationErrors[propertyName].Contains(error))
            {
                _validationErrors[propertyName].Add(error);
                OnPropertyChanged(nameof(HasValidationErrors));
                OnPropertyChanged(nameof(ValidationErrorsText));
            }
        }

        /// <summary>
        /// Removes all validation errors for a property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void ClearValidationErrors(string propertyName)
        {
            if (_validationErrors.ContainsKey(propertyName))
            {
                _validationErrors[propertyName].Clear();
                OnPropertyChanged(nameof(HasValidationErrors));
                OnPropertyChanged(nameof(ValidationErrorsText));
            }
        }

        /// <summary>
        /// Clears all validation errors
        /// </summary>
        protected void ClearAllValidationErrors()
        {
            _validationErrors.Clear();
            OnPropertyChanged(nameof(HasValidationErrors));
            OnPropertyChanged(nameof(ValidationErrorsText));
        }

        /// <summary>
        /// Gets validation errors for a specific property
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns>List of errors for the property</returns>
        protected List<string> GetValidationErrors(string propertyName)
        {
            return _validationErrors.ContainsKey(propertyName)
                ? _validationErrors[propertyName]
                : new List<string>();
        }

        #endregion

    }
}