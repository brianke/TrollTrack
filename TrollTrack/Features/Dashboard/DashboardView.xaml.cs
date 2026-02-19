namespace TrollTrack.Features.Dashboard;

public partial class DashboardView : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardView(DashboardViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"Error initializing Dashboard ViewModel: {ex.Message}");
            try
            {
                await DisplayAlert("Error", "Failed to load dashboard data. Please try again.", "OK");
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