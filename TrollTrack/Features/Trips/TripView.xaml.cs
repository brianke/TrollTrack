namespace TrollTrack.Features.Trips;

public partial class TripView : ContentView
{
    private readonly TripViewModel _viewModel;

    public TripView(TripViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
    }

/*
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Trip ViewModel: {ex.Message}");
            await DisplayAlert("Error", "Failed to load trip data. Please try again.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
*/
}