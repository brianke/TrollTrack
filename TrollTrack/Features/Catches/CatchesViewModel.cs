using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models;
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

    [ObservableProperty]
    private ObservableCollection<int> _waterTemps = new ObservableCollection<int>();

    [ObservableProperty]
    private int _waterTemp;

    [ObservableProperty]
    private ObservableCollection<int> _secchiNumberList = new();

    [ObservableProperty]
    private int _secchiDepth;

    [ObservableProperty]
    public ObservableCollection<string> _clarityOptions = new ObservableCollection<string>();

    [ObservableProperty]
    private string _clarity = "Clear";

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
        SecchiNumberList = new ObservableCollection<int>(Enumerable.Range(0, 100));
        WaterTemps = new ObservableCollection<int>(Enumerable.Range(35, 65));
        WaterTemp = 70;
        SecchiDepth = 10;

        ClarityOptions = new()
        {
            "Gin Clear",           // Can see bottom at 10+ feet
            "Clear",               // Can see 6-10 feet down
            "Lightly Stained",     // Can see 3-6 feet down
            "Stained",             // Can see 1-3 feet down
            "Heavily Stained",     // Can see 6-18 inches down
            "Muddy",               // Can see less than 6 inches
            "Chocolate Milk"       // Cannot see below surface
        };

        // Load saved custom clarities from database
        _ = LoadCustomClaritiesAsync();

        // Add "Custom..." at the end
        _clarityOptions.Add("Custom...");

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
            FishOptions = FishData.GetAllFishNames();

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
                WaterTemperature = WaterTemp,
                SecchiDepth = SecchiDepth,
                Clarity = Clarity,
                Notes = TripNotes,
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

    // Handle when clarity changes
    partial void OnClarityChanged(string value)
    {
        if (value == "Custom...")
        {
            // Trigger custom input
            MainThread.BeginInvokeOnMainThread(async () => await PromptForCustomClarityAsync());
        }
    }

    private async Task LoadCustomClaritiesAsync()
    {
        try
        {
            var customs = await _databaseService.GetCustomClaritiesAsync();

            foreach (var custom in customs)
            {
                if (!_clarityOptions.Contains(custom))
                {
                    // Insert before "Custom..." which is the last item
                    _clarityOptions.Insert(_clarityOptions.Count - 1, custom);
                    Debug.WriteLine($"Loaded custom clarity: {custom}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading custom clarities: {ex.Message}");
        }
    }

    private async Task PromptForCustomClarityAsync()
    {
        var result = await Application.Current.MainPage.DisplayPromptAsync(
            "Custom Water Clarity",
            "Describe the water clarity:",
            accept: "Save",
            cancel: "Cancel",
            placeholder: "e.g., Green-tinted, algae bloom",
            maxLength: 100,
            keyboard: Keyboard.Text);

        if (!string.IsNullOrWhiteSpace(result))
        {
            Debug.WriteLine($"Custom clarity entered: {result}");

            // Add to the collection if not already there
            if (!ClarityOptions.Contains(result))
            {
                // Insert before "Custom..." (last item)
                int insertIndex = ClarityOptions.Count - 1;
                ClarityOptions.Insert(insertIndex, result);

                // Save to database
                await _databaseService.SaveCustomClarityAsync(result);
            }

            // Set the selected value
            Clarity = result;
        }
        else
        {
            Clarity = "Clear";
        }
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
    private async Task AddNewCatchAsync(RodEntity rod)
    {
        if (string.IsNullOrWhiteSpace(SelectedFishOption))
        {
            await ShowAlertAsync("No Species Selected", "Please select a fish species before logging a catch.");
            return;
        }

        // Check if there's an active trip
        if (!HasActiveTrip)
        {
            await ShowAlertAsync("No Active Trip",
                "Please start a trip before logging catches. Go to the Trips tab to start a new trip.");
            return;
        }

        await ExecuteSafelyAsync(async () =>
        {
            var currentLocation = await _locationService.GetCurrentLocationAsync();
            var fishInfo = FishData.GetInfo(SelectedFishOption);

            var newCatch = new CatchDataEntity
            {
                Id = Guid.NewGuid(),                
                Timestamp = DateTime.Now,
                Latitude = currentLocation.Latitude,
                Longitude = currentLocation.Longitude,
                FishInfoId = fishInfo.Id,
                TripId = ActiveTrip!.Id,  // Set the TripId
                ProgramDataId = rod.ProgramDataId,           // Set the ProgramEntityId
            };

            // Save catch
            await _databaseService.SaveCatchAsync(newCatch);

            // Add catch to active trip's collection for UI
            if (ActiveTrip.Catches == null)
            {
                ActiveTrip.Catches = new List<CatchDataEntity>();
            }
            ActiveTrip.Catches.Add(newCatch);

            Catches.Insert(0, newCatch);
            TotalCatches++;
            if (newCatch.Timestamp.Date == DateTime.Today)
            {
                TodaysCatches++;
            }

            Debug.WriteLine($"Added new catch: {SelectedFishOption} at {newCatch.Timestamp}");

            // Clear selection
            SelectedFishOption = string.Empty;
        }, "Adding catch...");
    }

    #endregion
}
