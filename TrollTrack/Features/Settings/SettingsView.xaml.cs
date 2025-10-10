namespace TrollTrack.Features.Settings;

public partial class SettingsView : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsView(SettingsViewModel viewModel)
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
        //try
        //{
        //    await _viewModel.InitializeAsync();
        //}
        //catch (Exception ex)
        //{
        //    System.Diagnostics.Debug.WriteLine($"Error initializing Settings ViewModel: {ex.Message}");
        //    // Optionally show error message to user
        //    await DisplayAlert("Error", "Failed to load settings data. Please try again.", "OK");
        //}
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // The ViewModel will handle its own cleanup through BaseViewModel's Dispose
        // No additional cleanup needed here
    }
}
