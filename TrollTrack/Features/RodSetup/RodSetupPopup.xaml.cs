using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.RodSetup;

/// <summary>
/// Popup for selecting a lure and entering line out when adding a rod
/// </summary>
public partial class RodSetupPopup : ContentPage
{
    private readonly RodSetupViewModel _viewModel;
    //private readonly RodSetupEntity _rodToEdit;
    private bool _hasInitialized = false;

    public RodSetupPopup(RodSetupViewModel viewModel)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        //_rodToEdit = _viewModel.RodToEdit ?? new RodSetupEntity();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Guard against multiple initializations
        if (_hasInitialized)
        {
            System.Diagnostics.Debug.WriteLine("RodSetupPopup: Already initialized, skipping...");
            return;
        }

        _hasInitialized = true;

        // Initialize the ViewModel when the page appears
        try
        {
            System.Diagnostics.Debug.WriteLine("RodSetupPopup: Starting initialization");
            await _viewModel.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("RodSetupPopup: Initialization complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing RodSetup ViewModel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

            // Show error message to user
            await DisplayAlert("Error", "Failed to load lure data. Please try again.", "OK");

            // Reset flag so user can try again
            _hasInitialized = false;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Reset the initialization flag when popup is dismissed
        // This allows the popup to reinitialize if shown again
        _hasInitialized = false;

        System.Diagnostics.Debug.WriteLine("RodSetupPopup: OnDisappearing - reset initialization flag");
    }

    /// <summary>
    /// Handle OK button click - validate and save the rod configuration
    /// </summary>
    private async void OnOkClicked(object sender, EventArgs e)
    {
        if (_viewModel.SelectedLure == null)
        {
            await DisplayAlert("No Lure Selected", "Please select a lure for this rod.", "OK");
            return;
        }

        if (_viewModel.LineOut <= 0)
        {
            await DisplayAlert("Invalid Line Out", "Please enter a valid line out distance greater than 0.", "OK");
            return;
        }

        // Create the rod configuration with both lure and line out
        _viewModel.ConfirmRodSetup(_viewModel.Id);
        await Navigation.PopModalAsync();
    }

    /// <summary>
    /// Handle cancel button click
    /// </summary>
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        // Clear any selections
        _viewModel.CancelRodSetup();
        await Navigation.PopModalAsync();
    }
}