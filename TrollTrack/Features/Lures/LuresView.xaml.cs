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
        _viewModel = viewModel;
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