using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class CatchesView : ContentPage
{
    private readonly CatchesViewModel _viewModel;

    public CatchesView(CatchesViewModel viewModel)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;

        // Add this for debugging
        Debug.WriteLine($"BindingContext set to: {BindingContext?.GetType().Name}");

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
            System.Diagnostics.Debug.WriteLine($"Error initializing Catches ViewModel: {ex.Message}");
            // Optionally show error message to user
            await DisplayAlert("Error", "Failed to load catch data. Please try again.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // The ViewModel will handle its own cleanup through BaseViewModel's Dispose
        // No additional cleanup needed here
    }


    private async void OnTripTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is TripDataEntity trip)
        {
            await _viewModel.ViewTripDetailsCommand.ExecuteAsync(trip);
        }
    }

    private async void OnAddCatchTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is RodEntity rod)
        {
            // Show picker for fish species
            string selectedFish = await DisplayActionSheet(
                "Select Fish Species",
                "Cancel",
                null,
                _viewModel.FishOptions.ToArray());

            // If user cancelled or no valid selection
            if (string.IsNullOrWhiteSpace(selectedFish) || selectedFish == "Cancel")
            {
                return;
            }

            // Set the selected fish in the ViewModel
            _viewModel.SelectedFishOption = selectedFish;

            // Execute the add catch command
            if (_viewModel.AddNewCatchCommand.CanExecute(rod))
            {
                await _viewModel.AddNewCatchCommand.ExecuteAsync(rod);
            }
        }
    }
}