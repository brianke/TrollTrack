using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Trips
{
    public partial class TripViewModel : BaseViewModel
    {
        private readonly IWeatherService _weatherService;

        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<TripDataEntity> _recentTrips = new();

        [ObservableProperty]
        private TripDataEntity? _activeTrip;

        [ObservableProperty]
        private bool _hasActiveTrip;

        [ObservableProperty]
        private string _newTripName = string.Empty;

        [ObservableProperty]
        private DateTime _newTripDate = DateTime.Today;

        [ObservableProperty]
        private string _activeTripDuration = "00:00:00";

        [ObservableProperty]
        private int _activeTripCatchCount;

        #endregion

        #region Constructor

        public TripViewModel(
            ILocationService locationService,
            IDatabaseService databaseService,
            IWeatherService weatherService)
            : base(locationService, databaseService)
        {
            _weatherService = weatherService;
            Title = "Trips";
            _ = InitializeAsync();
        }

        #endregion

        #region Initialization

        public async Task InitializeAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                IsInitializing = true;

                // Load active trip if exists
                await LoadActiveTripAsync();

                // Load recent trips
                await LoadRecentTripsAsync();

                // Start duration timer if trip is active
                if (HasActiveTrip)
                {
                    StartDurationTimer();
                }

                IsInitializing = false;
            }, "Initializing trips...");
        }

        #endregion

        #region Trip Management Commands

        [RelayCommand]
        private async Task StartNewTripAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTripName))
            {
                await ShowAlertAsync("Trip Name Required", "Please enter a name for your trip.");
                return;
            }

            await ExecuteSafelyAsync(async () =>
            {
                // Get current location and weather
                var location = await _locationService.GetCurrentLocationAsync();
                var weather = await _weatherService.GetCurrentWeatherAsync(
                    location.Latitude,
                    location.Longitude);

                // Create new trip
                var trip = new TripDataEntity
                {
                    Id = Guid.NewGuid(),
                    TripName = NewTripName,
                    TripDate = NewTripDate,
                    StartTime = DateTime.Now,
                    IsActive = true,
                    //Location = weather?.LocationName ?? "Unknown",
                    WeatherEntity = weather,
                    WeatherEntityId = weather?.Id,
                    Catches = new List<CatchDataEntity>()
                };

                // Save to database
                await _databaseService.SaveTripAsync(trip);

                // Set as active trip
                ActiveTrip = trip;
                HasActiveTrip = true;

                // Start duration timer
                StartDurationTimer();

                // Clear form
                NewTripName = string.Empty;
                NewTripDate = DateTime.Today;

                // Reload recent trips
                await LoadRecentTripsAsync();

                Debug.WriteLine($"Started new trip: {trip.TripName}");
            }, "Starting trip...");
        }

        [RelayCommand]
        private async Task EndActiveTripAsync()
        {
            if (!HasActiveTrip || ActiveTrip == null)
                return;

            var confirm = await ShowConfirmationAsync(
                "End Trip?",
                $"Are you sure you want to end '{ActiveTrip.TripName}'?",
                "End Trip",
                "Cancel");

            if (!confirm)
                return;

            await ExecuteSafelyAsync(async () =>
            {
                ActiveTrip.EndTime = DateTime.Now;
                ActiveTrip.IsActive = false;

                await _databaseService.SaveTripAsync(ActiveTrip);

                // Stop duration timer
                StopDurationTimer();

                // Clear active trip
                ActiveTrip = null;
                HasActiveTrip = false;

                // Reload trips
                await LoadRecentTripsAsync();

                Debug.WriteLine("Trip ended successfully");
            }, "Ending trip...");
        }

        [RelayCommand]
        private async Task ViewTripDetailsAsync(TripDataEntity trip)
        {
            if (trip == null)
                return;

            // Navigate to trip details or set as active to view catches
            if (trip.IsActive)
            {
                ActiveTrip = trip;
                HasActiveTrip = true;
                StartDurationTimer();
            }
            else
            {
                // TODO: Navigate to trip history/details view
                await ShowAlertAsync("Trip Details",
                    $"Trip: {trip.TripName}\n" +
                    $"Date: {trip.TripDate:d}\n" +
                    $"Catches: {trip.CatchCount}\n"); 
                    //+ $"Duration: {trip.Duration?.ToString(@"hh\:mm\:ss") ?? "N/A"}");
            }
        }

        [RelayCommand]
        private async Task DeleteTripAsync(TripDataEntity trip)
        {
            if (trip == null)
                return;

            var confirm = await ShowConfirmationAsync(
                "Delete Trip?",
                $"Are you sure you want to delete '{trip.TripName}'? This will also delete all catches for this trip.",
                "Delete",
                "Cancel");

            if (!confirm)
                return;

            await ExecuteSafelyAsync(async () =>
            {
                await _databaseService.DeleteTripAsync(trip.Id);
                RecentTrips.Remove(trip);

                if (ActiveTrip?.Id == trip.Id)
                {
                    ActiveTrip = null;
                    HasActiveTrip = false;
                    StopDurationTimer();
                }

                Debug.WriteLine($"Deleted trip: {trip.TripName}");
            }, "Deleting trip...");
        }

        #endregion

        #region Helper Methods

        private async Task LoadActiveTripAsync()
        {
            var activeTrip = await _databaseService.GetActiveTripAsync();
            if (activeTrip != null)
            {
                ActiveTrip = activeTrip;
                HasActiveTrip = true;
                ActiveTripCatchCount = activeTrip.CatchCount;
            }
        }

        private async Task LoadRecentTripsAsync()
        {
            var trips = await _databaseService.GetRecentTripsAsync(10);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                RecentTrips.Clear();
                foreach (var trip in trips)
                {
                    RecentTrips.Add(trip);
                }
            });
        }

        #endregion

        #region Duration Timer

        private System.Threading.Timer? _durationTimer;

        private void StartDurationTimer()
        {
            StopDurationTimer();

            _durationTimer = new System.Threading.Timer(
                UpdateDuration,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1));
        }

        private void StopDurationTimer()
        {
            _durationTimer?.Dispose();
            _durationTimer = null;
        }

        private void UpdateDuration(object? state)
        {
            if (ActiveTrip?.StartTime == null)
                return;

            var duration = DateTime.Now - ActiveTrip.StartTime.Value;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ActiveTripDuration = duration.ToString(@"hh\:mm\:ss");
            });
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopDurationTimer();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}