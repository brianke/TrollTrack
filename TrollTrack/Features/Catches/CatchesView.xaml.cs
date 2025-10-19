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

    private void OnTripSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("=== OnTripSelectionChanged ===");

        if (e.CurrentSelection?.Count > 0 && e.CurrentSelection[0] is TripDataEntity trip)
        {
            Debug.WriteLine($"Trip selected: {trip.TripName}, ID: {trip.Id}");

            // Execute the command
            _viewModel.ViewTripDetailsCommand.Execute(trip);

            // Clear the selection so user can tap the same item again
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        }
    }
}