using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Dashboard;

public partial class DashboardView : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private CatchesViewModel? _catchesVM;

    public DashboardView(DashboardViewModel viewModel, CatchesViewModel? catchesVM)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
        _catchesVM = catchesVM;
    }

    private async void OnTripTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is TripDataEntity trip)
        {
            // Get the parent's ViewModel
            if (BindingContext is DashboardViewModel viewModel)
            {
                await _catchesVM.ViewTripDetailsCommand.ExecuteAsync(trip);

            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Initialize the ViewModel when the page appears
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Dashboard ViewModel: {ex.Message}");
            // Optionally show error message to user
            await DisplayAlert("Error", "Failed to load dashboard data. Please try again.", "OK");
        }
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // The ViewModel will handle its own cleanup through BaseViewModel's Dispose
        // No additional cleanup needed here
    }
}