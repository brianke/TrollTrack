using System.Timers;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Catches;

public partial class ActiveTripView : ContentView
{
    private System.Timers.Timer _longPressTimer;
    private object? _currentRod;
    private bool _isLongPress = false;

    /// <summary>
    /// ActiveTripView ctor
    /// </summary>
    public ActiveTripView()
    {
        InitializeComponent();

        // Initialize timer for long press detection
        _longPressTimer = new System.Timers.Timer(500); // 500ms = long press
        _longPressTimer.AutoReset = false;
        _longPressTimer.Elapsed += OnLongPressTimerElapsed;
    }

    #region Button Long Press (Using Pressed/Released)

    /// <summary>
    /// Button pressed - start long press timer
    /// </summary>
    private void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            Debug.WriteLine("Button Pressed - Starting long press timer");
            _currentRod = button.CommandParameter;
            _isLongPress = false;
            _longPressTimer.Start();
        }
    }

    /// <summary>
    /// Button released - cancel long press timer
    /// </summary>
    private void OnButtonReleased(object sender, EventArgs e)
    {
        Debug.WriteLine("Button Released - Stopping timer");
        _longPressTimer.Stop();

        // Small delay to let long press complete if it triggered
        Task.Delay(100).ContinueWith(_ =>
        {
            _currentRod = null;
        });
    }

    /// <summary>
    /// Timer elapsed - long press detected!
    /// </summary>
    private async void OnLongPressTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _isLongPress = true;
        Debug.WriteLine("??? LONG PRESS DETECTED! ???");

        if (_currentRod is RodSetupEntity rod)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (BindingContext is CatchesViewModel viewModel)
                {
                    Debug.WriteLine($"Long press on rod: {rod.Name}");

                    if (viewModel.EditRodCommand?.CanExecute(rod) == true)
                    {
                        await viewModel.EditRodCommand.ExecuteAsync(rod);
                    }
                    else
                    {
                        Debug.WriteLine("EditRodCommand cannot execute or doesn't exist");
                        await Shell.Current.DisplayAlert("Info",
                            $"Long press detected on {rod.Name}\nEdit command not available",
                            "OK");
                    }
                }
            });
        }
    }

    #endregion

    #region Catch Detail - View on Tap

    /// <summary>
    /// Handle tap on catch item - calls ViewCatchDetailCommand on the ViewModel
    /// </summary>
    private async void OnCatchTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            // Sender is the Border (the view the gesture is attached to), not the TapGestureRecognizer
            if (sender is not Border border || border.BindingContext is not CatchDataEntity catchData)
                return;

            if (BindingContext is not CatchesViewModel viewModel)
                return;

            if (viewModel.ViewCatchDetailCommand?.CanExecute(catchData) == true)
            {
                await viewModel.ViewCatchDetailCommand.ExecuteAsync(catchData);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnCatchTapped failed: {ex.Message}");
        }
    }

    #endregion

    #region Quick Tap - Add Catch

    /// <summary>
    /// Handle button click (quick tap) to add catch
    /// Only processes if NOT a long press
    /// </summary>
    private async void OnAddCatchClicked(object sender, EventArgs e)
    {
        // Wait a tiny bit to see if long press was triggered
        await Task.Delay(50);

        if (_isLongPress)
        {
            Debug.WriteLine("Ignoring click - was a long press");
            _isLongPress = false;
            return;
        }

        try
        {
            Debug.WriteLine("=== OnAddCatchClicked FIRED ===");

            if (sender is not Button button)
            {
                Debug.WriteLine("ERROR: Sender is not Button");
                return;
            }

            if (button.CommandParameter is not RodSetupEntity rod)
            {
                Debug.WriteLine("ERROR: CommandParameter is not RodSetupEntity");
                return;
            }

            Debug.WriteLine($"? Rod clicked: {rod.Name}");

            if (BindingContext is not CatchesViewModel viewModel)
            {
                Debug.WriteLine("ERROR: BindingContext is not CatchesViewModel");
                return;
            }

            string? selectedFish;

            var tripTarget = viewModel.ActiveTrip?.TargetSpecies;
            if (!string.IsNullOrWhiteSpace(tripTarget))
            {
                selectedFish = tripTarget;
                Debug.WriteLine($"Using target species from trip: {selectedFish}");
            }
            else
            {
                if (viewModel.FishOptions == null || !viewModel.FishOptions.Any())
                {
                    Debug.WriteLine("ERROR: No fish options available");
                    await Shell.Current.DisplayAlert("Error", "Fish species list not loaded", "OK");
                    return;
                }

                Debug.WriteLine($"Fish options available: {viewModel.FishOptions.Count}");

                var fishPopup = new FishSelectionPopup(viewModel.FishOptions);
                await Shell.Current.Navigation.PushModalAsync(fishPopup);
                selectedFish = await fishPopup.ResultTask;

                Debug.WriteLine($"User selected: {selectedFish}");

                if (string.IsNullOrWhiteSpace(selectedFish))
                {
                    Debug.WriteLine("User cancelled fish selection");
                    return;
                }
            }

            viewModel.SelectedFishOption = selectedFish;
            Debug.WriteLine($"Set SelectedFishOption to: {selectedFish}");

            if (viewModel.AddNewCatchCommand == null)
            {
                Debug.WriteLine("ERROR: AddNewCatchCommand is null");
                await Shell.Current.DisplayAlert("Error", "Add catch command not available", "OK");
                return;
            }

            bool canExecute = viewModel.AddNewCatchCommand.CanExecute(rod);
            Debug.WriteLine($"AddNewCatchCommand.CanExecute: {canExecute}");

            if (!canExecute)
            {
                Debug.WriteLine("Command cannot execute - possibly no active trip");
                await Shell.Current.DisplayAlert("Error", "Cannot add catch. Make sure you have an active trip.", "OK");
                return;
            }

            Debug.WriteLine("Executing AddNewCatchCommand...");
            await viewModel.AddNewCatchCommand.ExecuteAsync(rod);
            Debug.WriteLine("? AddNewCatchCommand executed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EXCEPTION in OnAddCatchClicked: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await Shell.Current.DisplayAlert("Error", $"Failed to add catch: {ex.Message}", "OK");
        }
    }

    #endregion
}