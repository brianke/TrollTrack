using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class CatchesViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;

    #region Trip Properties

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

    [ObservableProperty]
    private string _tripNotes = string.Empty;

    #endregion Trip Properties

    #region Rod Properites

    [ObservableProperty]
    private ObservableCollection<RodEntity> _rods = [];

    public bool HasNoRods => Rods == null || Rods.Count == 0;


    #endregion

    #region Catch Properties

    [ObservableProperty]
    private List<string> _fishOptions = [];

    [ObservableProperty]
    private string _selectedFishOption = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CatchDataEntity> _catches = [];

    [ObservableProperty]
    private int _totalCatches;

    [ObservableProperty]
    private int _todaysCatches;

    #endregion Catch Properties

    #region Constructor

    public CatchesViewModel(ILocationService locationService, IDatabaseService databaseService, ITripService tripService, IWeatherService weatherService)
        : base(locationService, databaseService)
    {
        _weatherService = weatherService;

        Title = "Trips";
        //_ = InitializeAsync();

        // Subscribe to collection changes
        Rods.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasNoRods));
    }

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsInitializing = true;
            //FishOptions = FishData.GetAllFishNames();

            // Load active trip
            await LoadActiveTripAsync();

            // Load past trips
            await LoadRecentTripsAsync();

            //// Load catches for active trip or all catches
            //await LoadCatchesAsync();

            IsInitializing = false;
        }, "Initializing catches...");
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
            //StartDurationTimer();

            // Clear form
            NewTripName = string.Empty;
            NewTripDate = DateTime.Today;

            // Reload recent trips
            await LoadRecentTripsAsync();

            Title = "Trip Catches";

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

            await _databaseService.UpdateTripAsync(ActiveTrip);

            // Clear active trip
            ActiveTrip = null;
            HasActiveTrip = false;

            // Reload trips
            await LoadRecentTripsAsync();

            Debug.WriteLine("Trip ended successfully");
        }, "Ending trip...");
    }

    private async Task LoadActiveTripAsync()
    {
        var trip = await _databaseService.GetActiveTripAsync();
        ActiveTrip = trip;
        HasActiveTrip = trip != null;

        if (HasActiveTrip)
        {
            Title = $"Catches - {ActiveTrip?.TripName}";
        }
        else
        {
            Title = "Catches";
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

    [RelayCommand]
    private async Task ViewTripDetailsAsync(TripDataEntity trip)
    //[RelayCommand(CanExecute = nameof(CanViewTripDetails))]
    //private void ViewTripDetailsAsync(TripDataEntity trip)
    {
        Debug.WriteLine("=== ViewTripDetails EXECUTING ===");

        if (trip == null)
            return;

        // Navigate to trip details or set as active to view catches
        //if (trip.IsActive)
        //{
        //    ActiveTrip = trip;
        //    HasActiveTrip = true;
        //    //StartDurationTimer();
        //}
        //else
        //{
        // TODO: Navigate to trip history/details view
        await Shell.Current.DisplayAlert("Trip Details",
            $"Trip: {trip.TripName}\n" +
            $"Date: {trip.TripDate:d}\n" +
            $"Catches: {trip.CatchCount}\n", "OK");

        //await ShowAlertAsync("Trip Details",
        //        $"Trip: {trip.TripName}\n" +
        //        $"Date: {trip.TripDate:d}\n" +
        //        $"Catches: {trip.CatchCount}\n");
            //+ $"Duration: {trip.Duration?.ToString(@"hh\:mm\:ss") ?? "N/A"}");
        //}
    }

    //private bool CanViewTripDetails(TripDataEntity trip)
    //{
    //    Debug.WriteLine($"CanViewTripDetails called - trip is null: {trip == null}");
    //    return trip != null;
    //}

    #endregion Trip Management Commands


    #region Rod Commands

    [RelayCommand]
    private async Task AddRod()
    {
        await ExecuteSafelyAsync(async () =>
        {
            IsLoading = true;

            // Add new rod to trip
            var _rod = new RodEntity
            {
                Name = $"Rod {Rods.Count + 1}"
            };

            Rods.Add(_rod);
            Debug.WriteLine($"Added new rod: {_rod.Name}");

            IsLoading = false;

        }, "Adding rod...");
    }

    #endregion


    #region Catches COmmands

    [RelayCommand]
    private async Task AddNewCatch(RodEntity rod)
    {
        Debug.WriteLine("=== ADD NEW CATCH BUTTON CLICKED ===");
        Shell.Current.DisplayAlert("Test", "Button works!", "OK");
    }

    #endregion
}
