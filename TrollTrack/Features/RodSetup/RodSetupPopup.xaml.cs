using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.RodSetup;

/// <summary>
/// Popup for selecting a lure when adding a rod
/// </summary>
public partial class RodSetupPopup : ContentPage
{
    private readonly RodSetupViewModel _viewModel;

    public RodSetupPopup(RodSetupViewModel viewModel)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;

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
            System.Diagnostics.Debug.WriteLine($"Error initializing RodSetup ViewModel: {ex.Message}");
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

    /// <summary>
    /// Handle lure selection from the CollectionView
    /// </summary>
    private async void OnLureSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is LureDataEntity selectedLure)
        {
            _viewModel.SelectLure(selectedLure);
            await Navigation.PopModalAsync();
        }
    }

    /// <summary>
    /// Handle cancel button click
    /// </summary>
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
