using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures;

public partial class LuresView : ContentPage
{
    private readonly LuresViewModel _viewModel;
    
    public LuresView(LuresViewModel viewModel)
	{
        InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
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
            System.Diagnostics.Debug.WriteLine($"Error initializing Lures ViewModel: {ex.Message}");
            try
            {
                await DisplayAlert("Error", "Failed to load lure data. Please try again.", "OK");
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

    private void OnImageTapped(object sender, EventArgs e)
    {
        Debug.WriteLine("IMAGE WAS TAPPED!");

        if (sender is Image image &&
        image.BindingContext is LureDataEntity lure &&
        BindingContext is LuresViewModel viewModel)
        {
            Debug.WriteLine($"Image path: {lure.PrimaryImage?.Path ?? "NULL"}");
            Debug.WriteLine($"Command is null: {viewModel.OpenImageCommand == null}");
            Debug.WriteLine($"Command can execute: {viewModel.OpenImageCommand?.CanExecute(lure.PrimaryImage?.Path)}");

            viewModel.OpenImageCommand?.Execute(lure.PrimaryImage?.Path);
        }
        else
        {
            Debug.WriteLine("Binding context issue!");
        }
    }
}