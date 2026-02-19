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


    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = InitializeWhenAppearingAsync();
    }

    private async Task InitializeWhenAppearingAsync()
    {
        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Catches ViewModel: {ex.Message}");
            try
            {
                await DisplayAlert("Error", "Failed to load catch data. Please try again.", "OK");
            }
            catch (Exception alertEx)
            {
                // Page may have been navigated away before alert could show
                System.Diagnostics.Debug.WriteLine($"Error showing alert: {alertEx.Message}");
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // No additional cleanup needed here
    }

}