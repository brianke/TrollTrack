using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures;

/// <summary>
/// Popup for selecting a lure and entering line out when adding a rod
/// </summary>
public partial class AddLurePopup : ContentPage
{
    private readonly AddLureViewModel _viewModel;
    private bool _hasInitialized = false;

    public AddLurePopup(AddLureViewModel viewModel)
    {
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Guard against multiple initializations
        if (_hasInitialized)
        {
            System.Diagnostics.Debug.WriteLine("AddLurePopup: Already initialized, skipping...");
            return;
        }

        _hasInitialized = true;

        // Initialize the ViewModel when the page appears
        try
        {
            System.Diagnostics.Debug.WriteLine("AddLurePopup: Starting initialization");
            await _viewModel.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("AddLurePopup: Initialization complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing AddLurePopup ViewModel: {ex.Message}");
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

        System.Diagnostics.Debug.WriteLine("AddLurePopup: OnDisappearing - reset initialization flag");
    }

    private void OnRemoveImageClicked(object sender, EventArgs e)
    {
        if (BindingContext is not AddLureViewModel vm)
            return;

        if (sender is not Button btn)
            return;

        if (btn.BindingContext is not LureImageEntity image)
            return;

        if (vm.RemoveLureImageCommand.CanExecute(image))
            vm.RemoveLureImageCommand.Execute(image);
    }

    private void OnSetPrimaryClicked(object sender, EventArgs e)
    {
        if (BindingContext is not AddLureViewModel vm) return;
        if (sender is not Button btn) return;
        if (btn.BindingContext is not LureImageEntity image) return;

        if (vm.SetPrimaryLureImageCommand.CanExecute(image))
            vm.SetPrimaryLureImageCommand.Execute(image);
    }

    private void OnColorClicked(object sender, EventArgs e)
    {
        if (BindingContext is not AddLureViewModel vm)
            return;

        if (sender is not Button btn)
            return;

        if (btn.BindingContext is not AddLureViewModel.LureColorOption option)
            return;

        if (vm.ToggleColorCommand.CanExecute(option))
            vm.ToggleColorCommand.Execute(option);
    }
}