namespace TrollTrack.MVVM.Views;

public partial class DashboardView : ContentPage
{
    private DashboardViewModel _viewModel;

    public DashboardView()
	{
		InitializeComponent();

        // Create and set the ViewModel
        var locationService = new LocationService();
        _viewModel = new DashboardViewModel(locationService);
        BindingContext = _viewModel;
    }
}