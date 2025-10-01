using System.Diagnostics;
using TrollTrack.Configuration;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IWeatherService _weatherService;

    #region Observable Properties

    [ObservableProperty]
    private WeatherDataEntity? _weatherEntity;

    [ObservableProperty]
    private string _weatherSummary = "Loading weather...";

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

            var weather = await _weatherService.GetCurrentWeatherAsync(CurrentLatitude, CurrentLongitude);

            if (weather != null)
            {
                WeatherEntity = weather;
                LocationName = weather.LocationName ?? "Location Unavailable";
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