namespace TrollTrack.MVVM.Views;

public partial class DashboardView : ContentPage
{
    private DashboardViewModel _viewModel;

    public DashboardView()
	{
		InitializeComponent();

        // Get the ViewModel from dependency injection when the page is created
        var viewModel = Handler?.MauiContext?.Services?.GetService<DashboardViewModel>();
        if (viewModel != null)
        {
            BindingContext = viewModel;
        }
        else
        {
            // Fallback: create manually (for XAML preview or if DI fails)
            BindingContext = CreateFallbackViewModel();
        }
    }

    private DashboardViewModel CreateFallbackViewModel()
    {
        try
        {
            var httpClient = new HttpClient();
            var locationService = new TrollTrack.Services.LocationService();
            var weatherService = new TrollTrack.Services.WeatherService(httpClient);

            return new DashboardViewModel(locationService, weatherService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to create fallback ViewModel: {ex.Message}");
            return null!;
        }
    }
}