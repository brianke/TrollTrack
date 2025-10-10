using TrollTrack.Features.Catches;

namespace TrollTrack.Features.Trips;

public partial class TripCatchesContainerView : ContentPage
{
    private readonly TripViewModel _tripViewModel;
    private readonly CatchesViewModel _catchesViewModel;
    private readonly IServiceProvider _serviceProvider;

    private TripView? _tripView;
    private CatchesView? _catchesView;

    public TripCatchesContainerView(
        TripViewModel tripViewModel,
        CatchesViewModel catchesViewModel,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _tripViewModel = tripViewModel ?? throw new ArgumentNullException(nameof(tripViewModel));
        _catchesViewModel = catchesViewModel ?? throw new ArgumentNullException(nameof(catchesViewModel));
        _serviceProvider = serviceProvider;

        // Subscribe to trip state changes
        _tripViewModel.PropertyChanged += OnTripViewModelPropertyChanged;

        // Set initial view
        UpdateCurrentView();
    }

    private void OnTripViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TripViewModel.HasActiveTrip))
        {
            MainThread.BeginInvokeOnMainThread(UpdateCurrentView);
        }
    }

    private void UpdateCurrentView()
    {
        if (_tripViewModel.HasActiveTrip)
        {
            // Show Catches view
            Title = "Catches";
            ShowCatchesView();
        }
        else
        {
            // Show Trip management view
            Title = "Trips";
            ShowTripView();
        }
    }

    private void ShowTripView()
    {
        if (_tripView == null)
        {
            _tripView = _serviceProvider.GetRequiredService<TripView>();
        }

        ContentContainer.Content = _tripView;
    }

    private void ShowCatchesView()
    {
        if (_catchesView == null)
        {
            _catchesView = _serviceProvider.GetRequiredService<CatchesView>();
        }

        ContentContainer.Content = _catchesView;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _tripViewModel.InitializeAsync();

            if (_tripViewModel.HasActiveTrip)
            {
                await _catchesViewModel.InitializeAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing container: {ex.Message}");
            await DisplayAlert("Error", "Failed to load view. Please try again.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _tripViewModel.PropertyChanged -= OnTripViewModelPropertyChanged;
    }
}