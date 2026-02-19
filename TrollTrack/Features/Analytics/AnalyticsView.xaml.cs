namespace TrollTrack.Features.Analytics;

public partial class AnalyticsView : ContentPage
{
    private readonly AnalyticsViewModel _viewModel;

    public AnalyticsView(AnalyticsViewModel viewModel)
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
            System.Diagnostics.Debug.WriteLine($"Error initializing Analytics ViewModel: {ex.Message}");
            try
            {
                await DisplayAlert("Error", "Failed to load analytics data. Please try again.", "OK");
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