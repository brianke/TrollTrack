using CommunityToolkit.Maui.Views;
using TrollTrack.Features.RodSetup;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class CatchesViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    //private readonly IRodSetupService _rodSetupService;
    private RodSetupViewModel? _rodSetupVM;
    private LuresViewModel? _luresVM;

    #region Trip Properties

    [ObservableProperty]
    private ObservableCollection<TripDataEntity> _recentTrips = new();

    [ObservableProperty]
    private TripDataEntity? _activeTrip;

    [ObservableProperty]
    private bool _hasActiveTrip;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartNewTripCommand))]
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

    private bool CanStartNewTrip() => !string.IsNullOrWhiteSpace(NewTripName);

    #endregion Trip Properties

    #region Rod Properites

    [ObservableProperty]
    private ObservableCollection<RodSetupEntity> _rods = [];

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

    /// <summary>
    /// Summary of catches by species for the active trip, e.g. "13 Walleye, 3 Perch"
    /// </summary>
    public string CatchesBySpeciesSummary => Catches == null || Catches.Count == 0
        ? "0 catches"
        : string.Join(", ", Catches
            .GroupBy(c => c.FishName)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Count()} {g.Key}"));

    #endregion Catch Properties

    #region Constructor

    public CatchesViewModel(ILocationService locationService, IDatabaseService databaseService, ITripService tripService, IWeatherService weatherService, LuresViewModel? luresViewModel)
        : base(locationService, databaseService)
    {
        _weatherService = weatherService;
        //_rodSetupService = rodSetupService;
        _luresVM = luresViewModel;

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
        ClarityOptions.Add("Custom...");

        //_ = InitializeAsync();

        // Subscribe to collection changes
        Rods.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasNoRods));
        Catches.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CatchesBySpeciesSummary));
    }

    #endregion

    #region Initialization

    public async Task InitializeAsync()
    {
        try
        {
            await ExecuteSafelyAsync(async () =>
            {
                IsInitializing = true;
                FishOptions = FishData.GetAllFishNames();

                // Load active trip
                await LoadActiveTripAsync();

                // Load past trips
                await LoadRecentTripsAsync();

                await LoadRodsAsync();

                IsInitializing = false;
                Debug.WriteLine($"Initialization complete - Active trip: {HasActiveTrip}, Catches: {Catches.Count}, Rods: {Rods.Count}");

            }, "Initializing catches...");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CatchesViewModel InitializeAsync() failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

    }

    #endregion

    #region Trip Management Commands

    [RelayCommand(CanExecute = nameof(CanStartNewTrip))]
    private async Task StartNewTripAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewTripName))
            {
                await ShowAlertAsync("Trip Name Required", "Please enter a name for your trip.");
                return;
            }

            await ExecuteSafelyAsync(async () =>
            {
                // Use last known location (from Dashboard/catch) or default - no GPS request when starting trip
                var lastKnown = await BaseLocationService.GetLastKnownLocationAsync();
                var lat = lastKnown?.Latitude ?? TrollTrack.Configuration.AppConfig.Constants.DefaultLatitude;
                var lon = lastKnown?.Longitude ?? TrollTrack.Configuration.AppConfig.Constants.DefaultLongitude;
                var weather = await _weatherService.GetCurrentWeatherAsync(lat, lon);

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
                await BaseDatabaseService.SaveTripAsync(trip);

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
        catch (Exception ex)
        {
            Debug.WriteLine($"CatchesViewModel StartNewTripAsync() failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

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
            var customs = await BaseDatabaseService.GetCustomClaritiesAsync();

            foreach (var custom in customs)
            {
                if (!ClarityOptions.Contains(custom))
                {
                    // Insert before "Custom..." which is the last item
                    ClarityOptions.Insert(ClarityOptions.Count - 1, custom);
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
        var page = Application.Current?.Windows[0]?.Page;
        if (page == null) return;

        var result = await page.DisplayPromptAsync(
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
                await BaseDatabaseService.SaveCustomClarityAsync(result);
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
        try
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

                await BaseDatabaseService.UpdateTripAsync(ActiveTrip);

                // Clear active trip
                ActiveTrip = null;
                HasActiveTrip = false;

                // Reload trips
                await LoadRecentTripsAsync();

                Debug.WriteLine("Trip ended successfully");
            }, "Ending trip...");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CatchesViewModel EndActiveTripAsync() failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadActiveTripAsync()
    {
        var trip = await BaseDatabaseService.GetActiveTripAsync();
        ActiveTrip = trip;
        HasActiveTrip = trip != null;

        if (HasActiveTrip)
        {
            Title = $"Catches - {ActiveTrip?.TripName}";

            // Load catches for the active trip
            await LoadCatchesAsync();

            Debug.WriteLine($"Active trip loaded: {ActiveTrip?.TripName} with {Catches.Count} catches");
        }
        else
        {
            Title = "Catches";

            // Clear catches if no active trip
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Catches.Clear();
                TotalCatches = 0;
                TodaysCatches = 0;
            });
        }
    }

    private async Task LoadRecentTripsAsync()
    {
        var trips = await BaseDatabaseService.GetRecentTripsAsync(10);

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
    {
        if (trip == null)
            return;

        try
        {
            var catches = await BaseDatabaseService.GetCatchesForTripAsync(trip.Id);
            var display = new TripCatchesDisplay(trip, catches);
            var popup = new TripCatchesPopup(display, BaseDatabaseService, OnPastTripDeletedFromPopupAsync);

            var page = Application.Current?.Windows[0]?.Page;
            if (page?.Navigation != null)
            {
                await page.Navigation.PushModalAsync(popup);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ViewTripDetailsAsync failed: {ex.Message}");
            await ShowAlertAsync("Error", "Failed to load trip catches.");
        }
    }

    //private bool CanViewTripDetails(TripDataEntity trip)
    //{
    //    Debug.WriteLine($"CanViewTripDetails called - trip is null: {trip == null}");
    //    return trip != null;
    //}

    /// <summary>
    /// Load catches for the active trip
    /// </summary>
    private async Task LoadCatchesAsync()
    {
        if (!HasActiveTrip || ActiveTrip == null)
        {
            Debug.WriteLine("No active trip - clearing catches");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Catches.Clear();
                TotalCatches = 0;
                TodaysCatches = 0;
            });
            return;
        }

        try
        {
            Debug.WriteLine($"Loading catches for trip: {ActiveTrip.TripName}");

            // Get catches for this trip from the database
            var catches = await BaseDatabaseService.GetCatchesForTripAsync(ActiveTrip.Id);

            Debug.WriteLine($"Loaded {catches.Count} catches from database");

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Catches.Clear();

                foreach (var catchData in catches.OrderByDescending(c => c.Timestamp))
                {
                    Catches.Add(catchData);
                }

                // Update counts
                TotalCatches = catches.Count;
                TodaysCatches = catches.Count(c => c.Timestamp.Date == DateTime.Today);

                Debug.WriteLine($"Catches loaded: Total={TotalCatches}, Today={TodaysCatches}");
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading catches: {ex.Message}");
            await ShowAlertAsync("Error", "Failed to load catches. Please try again.");
        }
    }

    #endregion Trip Management Commands


    #region Rod Commands

    private async Task LoadRodsAsync()
    {
        //await ExecuteSafelyAsync(async () =>
        //{
            var rodList = await BaseDatabaseService.GetAllRodSetupsAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Rods.Clear();
                foreach (var rod in rodList)
                {
                    Rods.Add(rod);
                }

                // Notify UI that HasNoRods property has changed
                OnPropertyChanged(nameof(HasNoRods));
            });

            Debug.WriteLine($"Loaded {rodList.Count} rods with their lure info");
        //}, "Loading rods...", showErrorAlert: false);
    }

    [RelayCommand]
    private async Task AddRod()
    {
        try
        {
            Debug.WriteLine("=== AddRod Command Started ===");

            if (_luresVM == null) return;

            // Create the rod setup view model
            _rodSetupVM = new RodSetupViewModel(BaseLocationService, BaseDatabaseService, _luresVM);
            _rodSetupVM.Name = $"Rod {Rods.Count + 1}";

            // Subscribe to the rod setup confirmed event (not just lure selected)
            _rodSetupVM.RodSetupConfirmed += OnRodSetupConfirmed;
            Debug.WriteLine("Subscribed to RodSetupConfirmed event");

            // Create and show the popup
            var popup = new RodSetupPopup(_rodSetupVM);
            Debug.WriteLine("Pushing modal popup");

            var page = Application.Current?.Windows[0]?.Page;
            if (page?.Navigation == null) return;

            await page?.Navigation.PushModalAsync(popup)!;
            Debug.WriteLine("Modal popup displayed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"!!! ERROR in AddRod: {ex.Message}");
            await ShowAlertAsync("Error", "Failed to open rod setup. Please try again.");
        }
    }

    /// <summary>
    /// Handle rod setup confirmation from the popup (includes lure and line out)
    /// </summary>
    private async void OnRodSetupConfirmed(object? sender, RodSetupEntity rodSetupEntity)
    {
        try
        {
            // Unsubscribe from the event
            if (_rodSetupVM != null)
            {
                _rodSetupVM.RodSetupConfirmed -= OnRodSetupConfirmed;
            }

            Debug.WriteLine($"=== Rod Setup Confirmed ===");
            Debug.WriteLine($"Lure: {rodSetupEntity.Lure!.DisplayName}");   // Lure cannot be null here so added (!) ignore
            Debug.WriteLine($"Line Out: {rodSetupEntity.LineOut} feet");

            RodSetupEntity newRod;

            if (rodSetupEntity.Id == 0)
            {
                // Create a new rod with the selected lure and line out (Name comes from popup; fallback if empty)
                var defaultName = $"Rod {Rods.Count + 1}";
                newRod = new RodSetupEntity
                {
                    Name = string.IsNullOrWhiteSpace(rodSetupEntity.Name) ? defaultName : rodSetupEntity.Name.Trim(),
                    LineOut = rodSetupEntity.LineOut,
                    Diver = rodSetupEntity.Diver,
                    DiverId = rodSetupEntity.Diver?.Id,
                    Lure = rodSetupEntity.Lure,
                    LureId = rodSetupEntity.Lure.Id,
                };
            }
            else
            {
                newRod = new RodSetupEntity
                {
                    Id = rodSetupEntity.Id,
                    Name = rodSetupEntity.Name,
                    LineOut = rodSetupEntity.LineOut,
                    Diver = rodSetupEntity.Diver,
                    DiverId = rodSetupEntity.Diver?.Id,
                    Lure = rodSetupEntity.Lure,
                    LureId = rodSetupEntity.Lure.Id,
                };
            }

            Debug.WriteLine($"Creating rod with Name: {newRod.Name}, LureId: {newRod.LureId}, LineOut: {newRod.LineOut}");

            // Save the rod to the database
            var result = await BaseDatabaseService.SaveRodSetupAsync(newRod);
            Debug.WriteLine($"Database save returned: {result}");

            // Reload rods to show the new one
            await LoadRodsAsync();

            // Verify the rod was added
            Debug.WriteLine($"Total rods after reload: {Rods.Count}");

            await ShowAlertAsync("Success",
                $"Rod added:\n" +
                $"Line Out: {rodSetupEntity.LineOut} feet" + 
                $"\n{rodSetupEntity.Lure.DisplayName}" +
                $"\n{((rodSetupEntity.Diver == null) ? String.Empty : rodSetupEntity.Diver.DisplayName)}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"!!! ERROR in OnRodSetupConfirmed: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await ShowAlertAsync("Error", $"Failed to save rod: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EditRod(RodSetupEntity rod)
    {
        try
        {
            Debug.WriteLine("=== EditRod Command Started ===");

            if (_luresVM == null) return;

            // Create the rod setup view model
            _rodSetupVM = new RodSetupViewModel(BaseLocationService, BaseDatabaseService, _luresVM)
            {
                Title = "Update Rod Setup",
                AddButtonText = "Update",
                Id = rod.Id,
                Name = rod.Name,
                LineOutText = rod.LineOut.ToString(),
                SelectedDiver = rod.Diver,
                SelectedLure = rod.Lure,
                CurrentSetup = $"Line Out: {rod.LineOut} feet" +
                                    $"\n{(rod.Lure == null ? string.Empty : rod.Lure.DisplayName)}" +
                                    $"\n{(rod.Diver == null ? string.Empty : rod.Diver.DisplayName)}"
            };

            if (rod != null)
            {
                // Subscribe to the rod setup confirmed event (not just lure selected)
                _rodSetupVM.RodSetupConfirmed += OnRodSetupConfirmed;
                Debug.WriteLine("Subscribed to RodSetupConfirmed event");

                // Create and show the popup
                var popup = new RodSetupPopup(_rodSetupVM);
                Debug.WriteLine("Pushing modal popup");

                var page = Application.Current?.Windows[0]?.Page;
                if (page?.Navigation == null) return;

                await page?.Navigation.PushModalAsync(popup)!;
                Debug.WriteLine("Modal popup displayed");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"!!! ERROR in AddRod: {ex.Message}");
            await ShowAlertAsync("Error", "Failed to open rod setup. Please try again.");
        }
    }

    /*
        /// <summary>
        /// Command to remove a rod
        /// </summary>
        [RelayCommand]
        private async Task RemoveRodAsync(RodSetupEntity rod)
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage == null) return;

            bool confirm = await mainPage.DisplayAlert(
                "Remove Rod",
                $"Remove {rod.Name}?",
                "Yes",
                "No");

            if (confirm)
            {
                try
                {
                    await ExecuteSafelyAsync(async () =>
                    {
                        await BaseDatabaseService.DeleteRodSetupAsync(rod.Id);
                        await LoadRodsAsync();
                        Debug.WriteLine($"Rod removed: {rod.Name}");
                    }, "Removing rod...");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CatchesViewModel RemoveRodAsync() failed: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }


        private async Task EditRodDetails(RodSetupEntity rod)
        {
            // Navigate to Edit Rod page
            var parameters = new Dictionary<string, object>
                {
                    { "RodId", rod.Id },
                    { "TripId", ActiveTrip?.Id ?? new Guid() }
                };

            await Shell.Current.GoToAsync("EditRodPage", parameters);
        }

        private async Task ChangeLure(RodSetupEntity rod)
        {
            // Navigate to lure selection page
            var parameters = new Dictionary<string, object>
                {
                    { "RodId", rod.Id },
                    { "Mode", "SelectLure" }
                };

            await Shell.Current.GoToAsync("SelectLurePage", parameters);
        }

        private async Task UpdateLineOut(RodSetupEntity rod)
        {
            // Show prompt to update line out
            string result = await Shell.Current.DisplayPromptAsync(
                "Update Line Out",
                $"Current: {rod.LineOut} feet\nEnter new line out distance:",
                "Update",
                "Cancel",
                "Enter feet",
                keyboard: Keyboard.Numeric,
                initialValue: rod.LineOut.ToString());

            if (!string.IsNullOrEmpty(result) && int.TryParse(result, out int newLineOut))
            {
                try
                {
                    rod.LineOut = newLineOut;
                    await _rodSetupService.UpdateSetupAsync(rod);

                    // Refresh the rod in the collection
                    var index = Rods.IndexOf(rod);
                    if (index >= 0)
                    {
                        Rods[index] = rod;
                    }

                    await Shell.Current.DisplayAlert("Success",
                        "Line out updated successfully", "OK");
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error",
                        $"Failed to update line out: {ex.Message}", "OK");
                }
            }
        }
    */

    private async Task DeleteRod(RodSetupEntity rod)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Rod",
            $"Are you sure you want to remove '{rod.Name}'?",
            "Yes",
            "No");

        if (confirm)
        {
            Rods.Remove(rod);

            // Notify UI that HasNoRods property has changed
            OnPropertyChanged(nameof(HasNoRods));
        }
    }
    #endregion


    #region Catches Commands

    [RelayCommand]
    private async Task ViewCatchDetailAsync(CatchDataEntity? catchData)
    {
        if (catchData == null)
            return;

        try
        {
            var popup = new CatchDetailPopup(catchData, BaseDatabaseService, OnCatchDeletedFromDetailAsync);
            var page = Application.Current?.Windows[0]?.Page;
            if (page?.Navigation != null)
            {
                await page.Navigation.PushModalAsync(popup);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CatchesViewModel ViewCatchDetailAsync failed: {ex.Message}");
            await ShowAlertAsync("Error", "Failed to display catch details.");
        }
    }

    private async Task OnPastTripDeletedFromPopupAsync(Guid tripId)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var match = RecentTrips.FirstOrDefault(t => t.Id == tripId);
            if (match != null)
                RecentTrips.Remove(match);
        });
    }

    private async Task OnCatchDeletedFromDetailAsync(CatchDataEntity deleted)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            var match = Catches.FirstOrDefault(c => c.Id == deleted.Id);
            if (match != null)
                Catches.Remove(match);

            if (ActiveTrip?.Catches != null)
            {
                var inTrip = ActiveTrip.Catches.FirstOrDefault(c => c.Id == deleted.Id);
                if (inTrip != null)
                    ActiveTrip.Catches.Remove(inTrip);
            }

            TotalCatches = Catches.Count;
            TodaysCatches = Catches.Count(c => c.Timestamp.Date == DateTime.Today);
        });
    }

    [RelayCommand]
    private async Task AddNewCatchAsync(RodSetupEntity rod)
    {
        try
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
                // Get exact location for this catch (do not reuse previous location)
                var currentLocation = await BaseLocationService.GetExactLocationAsync();

                // ✅ IMPORTANT: Save the location to the database FIRST
                await BaseDatabaseService.SaveLocationAsync(currentLocation);
                Debug.WriteLine($"Location saved: {currentLocation.Latitude}, {currentLocation.Longitude} (ID: {currentLocation.Id})");

                var fishInfo = FishData.GetInfoFromName(SelectedFishOption);

                var newCatch = new CatchDataEntity
                {
                    Id = Guid.NewGuid(),
                    TripId = ActiveTrip!.Id,  // ✅ Make sure to set TripId!
                    Timestamp = DateTime.Now,
                    LocationId = currentLocation.Id,  // Now this ID exists in the database
                    FishInfoId = fishInfo.Id,
                    LureId = rod.LureId,
                    DiverDataId = rod.DiverId,
                    LineOut = rod.LineOut,
                    Latitude = currentLocation.Latitude,
                    Longitude = currentLocation.Longitude,
                    FishName = FishData.GetFishNameById(fishInfo.Id),
                    LureDisplayName = rod.Lure?.DisplayName ?? "Unknown",
                    LureImagePath = rod.Lure?.PrimaryImage?.Path,
                    DiverDisplayName = rod.Diver?.DisplayName ?? "None",
                    Speed = currentLocation.Speed,
                    Direction = currentLocation.Course
                };

                // Save catch
                await BaseDatabaseService.SaveCatchAsync(newCatch);
                Debug.WriteLine($"Catch saved: {newCatch.FishName} at location {newCatch.LocationId}");

                // Add catch to active trip's collection for UI
                if (ActiveTrip != null)
                {
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
                }

                // Clear selection
                SelectedFishOption = string.Empty;
            }, "Adding catch...");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CatchesViewModel AddNewCatchAsync() failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }

    }

    #endregion
}
