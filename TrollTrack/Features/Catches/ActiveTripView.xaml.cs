using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class ActiveTripView : ContentView
{
    //private readonly CatchesViewModel _viewModel;
    CancellationTokenSource _cts;

    public ActiveTripView()
    {
    	InitializeComponent();
    }

    private void OnPointerPressed(object sender, PointerEventArgs e)
    {
        _cts = new CancellationTokenSource();
        DetectLongPress(_cts.Token);
    }

    private void OnPointerReleased(object sender, PointerEventArgs e)
    {
        _cts?.Cancel();
    }

    private async void DetectLongPress(CancellationToken token)
    {
        try
        {
            await Task.Delay(800, token); // long press duration
            if (!token.IsCancellationRequested)
            {
                // Long press action
                Console.WriteLine("Long press detected!");
            }
        }
        catch { }
    }

    private async void OnAddCatchTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is RodSetupEntity rod)
        {
            // Get the parent's ViewModel
            if (BindingContext is CatchesViewModel viewModel)
            {
                // Show picker for fish species
                string selectedFish = await Shell.Current.DisplayActionSheet(
                    "Select Fish Species",
                    "Cancel",
                    null,
                    viewModel.FishOptions.ToArray());

                // If user cancelled or no valid selection
                if (string.IsNullOrWhiteSpace(selectedFish) || selectedFish == "Cancel")
                {
                    return;
                }

                // Set the selected fish in the ViewModel
                viewModel.SelectedFishOption = selectedFish;

                // Execute the add catch command
                if (viewModel.AddNewCatchCommand.CanExecute(rod))
                {
                    await viewModel.AddNewCatchCommand.ExecuteAsync(rod);
                }
            }
        }
    }
}