using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class NewTripView : ContentView
{
    //private readonly CatchesViewModel _viewModel;

    public NewTripView()
    {
    	InitializeComponent();
    }

    private async void OnTripTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is TripDataEntity trip)
        {
            // Get the parent's ViewModel
            if (BindingContext is CatchesViewModel viewModel)
            {
                await viewModel.ViewTripDetailsCommand.ExecuteAsync(trip);
            }
        }
    }
}