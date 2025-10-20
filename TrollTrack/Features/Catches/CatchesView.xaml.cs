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

    private void OnTripTapped(object sender, TappedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== OnTripTapped fired ===");

        // The sender is the Border that was tapped
        if (sender is Border border)
        {
            System.Diagnostics.Debug.WriteLine($"Sender is Border, BindingContext type: {border.BindingContext?.GetType().Name ?? "NULL"}");

            // The Border's BindingContext is the TripDataEntity from the DataTemplate
            if (border.BindingContext is TripDataEntity trip)
            {
                System.Diagnostics.Debug.WriteLine($"Trip found: {trip.TripName}, ID: {trip.Id}, Active: {trip.IsActive}");

                // Execute the ViewModel command with the trip
                _viewModel.ViewTripDetailsCommand.Execute(trip);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"BindingContext is not TripDataEntity, it's: {border.BindingContext?.GetType().Name ?? "NULL"}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Sender is not Border, it's: {sender?.GetType().Name ?? "NULL"}");
        }
    }

    private void OnAddCatchTapped(object sender, TappedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("=== OnAddCatchTapped fired ===");

        // The sender is the Border that was tapped
        if (sender is Border border)
        {
            System.Diagnostics.Debug.WriteLine($"Sender is Border, BindingContext type: {border.BindingContext?.GetType().Name ?? "NULL"}");

            // The Border's BindingContext is the RodEntity from the DataTemplate
            if (border.BindingContext is RodEntity rod)
            {
                System.Diagnostics.Debug.WriteLine($"Found: {rod.Name}, ID: {rod.Id}");

                // Execute the ViewModel command with the trip
                _viewModel.AddNewCatchCommand.Execute(rod);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"BindingContext is not RodEntity, it's: {border.BindingContext?.GetType().Name ?? "NULL"}");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"Sender is not Border, it's: {sender?.GetType().Name ?? "NULL"}");
        }
    }
}