using System.Diagnostics;
using TrollTrack.Configuration;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;
    private bool _hasRequestedLocationOnStart;

    #region Observable Properties

    [ObservableProperty]
    private WeatherDataEntity? _weatherEntity;

    [ObservableProperty]
    private string _weatherSummary = "Loading weather...";

    [ObservableProperty]
    private ObservableCollection<TripDataEntity> _recentTrips = new();


    #endregion

    #region Constructor

    public DashboardViewModel(ILocationService locationService, IDatabaseService databaseService, IWeatherService weatherService)
        : base(locationService, databaseService)
    {
        _weatherService = weatherService;

        Title = "Dashboard";
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization and Data Loading

    public async Task InitializeAsync()
    {
        IsInitializing = true;
        await LoadDataAsync(isRefresh: false);

        IsInitializing = false;
    }

    [RelayCommand]
    private async Task RefreshDashboard()
    {
        await LoadDataAsync(isRefresh: true);
    }

    private async Task LoadDataAsync(bool isRefresh)
    {
        try
        {
            // Only request location on: (1) first app/dashboard load, (2) explicit refresh
            var shouldRequestLocation = isRefresh || !_hasRequestedLocationOnStart;

            if (shouldRequestLocation)
            {
                var statusMessage = isRefresh ? "Refreshing dashboard..." : "Initializing dashboard...";
                await ExecuteSafelyAsync(async () =>
                {
                    WeatherSummary = "Fetching location and weather...";

                    if (!await GetAndSetLocationAsync(showAlerts: isRefresh))
                    {
                        if (!isRefresh)
                        {
                            CurrentLatitude = AppConfig.Constants.DefaultLatitude;
                            CurrentLongitude = AppConfig.Constants.DefaultLongitude;
                            LocationName = "Default Location (Great Lakes)";
                        }
                        else
                        {
                            WeatherSummary = "Could not update location.";
                            return;
                        }
                    }

                    _hasRequestedLocationOnStart = true;

                    List<WeatherDataEntity>? weather = null;
                    int maxRetries = isRefresh ? 1 : 3;
                    for (int attempt = 0; attempt < maxRetries; attempt++)
                    {
                        try
                        {
                            weather = await _weatherService.GetWeatherForecastAsync(CurrentLatitude, CurrentLongitude);
                            if (weather != null) break;
                        }
                        catch (Exception ex) when (attempt < maxRetries - 1)
                        {
                            Debug.WriteLine($"Weather fetch attempt {attempt + 1} failed: {ex.Message}");
                            await Task.Delay(2000 * (attempt + 1));
                        }
                    }

                    if (weather != null && weather.Count > 0)
                    {
                        WeatherEntity = weather[0];
                        LocationName = weather[0].LocationName ?? "Location Unavailable";
                        WeatherSummary = $"Weather updated at {DateTime.Now:T}";
                        if (isRefresh)
                        {
                            RefreshStatus = "Dashboard updated";
                        }
                    }
                    else
                    {
                        WeatherSummary = "Weather data unavailable.";
                    }
                }, statusMessage, showErrorAlert: isRefresh);
            }
            else
            {
                // Subsequent tab switch - no location request, keep cached data
                if (WeatherEntity == null && (CurrentLatitude != 0 || CurrentLongitude != 0))
                {
                    var weather = await _weatherService.GetWeatherForecastAsync(CurrentLatitude, CurrentLongitude);
                    if (weather != null)
                        WeatherEntity = weather[0];
                }
                else if (WeatherEntity == null)
                {
                    CurrentLatitude = AppConfig.Constants.DefaultLatitude;
                    CurrentLongitude = AppConfig.Constants.DefaultLongitude;
                    var weather = await _weatherService.GetWeatherForecastAsync(CurrentLatitude, CurrentLongitude);
                    if (weather != null)
                        WeatherEntity = weather[0];
                }
            }

            // Load past trips
            await LoadRecentTripsAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DashboardViewModel LoadDataASync() failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region Navigation Commands

    private async Task NavigateToAsync(string route)
    {
        try
        {
            await ExecuteSafelyAsync(() => Shell.Current.GoToAsync(route), "Navigating...");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DashboardViewModel NavigateToAsync() failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogCatchAsync() => await NavigateToAsync(RouteConstants.Catches);

    //[RelayCommand]
    //private async Task ChangeTrollingMethodAsync() => await NavigateToAsync(RouteConstants.Programs);

    [RelayCommand]
    private async Task ViewCatchHistoryAsync() => await NavigateToAsync(RouteConstants.Catches);

    [RelayCommand]
    private async Task ManageLuresAsync() => await NavigateToAsync(RouteConstants.Lures);

    #endregion

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

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }

    #endregion
}