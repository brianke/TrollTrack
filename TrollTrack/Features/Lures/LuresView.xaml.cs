using System.Timers;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures;

public partial class LuresView : ContentPage
{
    private readonly LuresViewModel _viewModel;
    private readonly System.Timers.Timer _longPressTimer;
    private object? _currentLure;
    private bool _isLongPress;

    public LuresView(LuresViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;

        _longPressTimer = new System.Timers.Timer(500);
        _longPressTimer.AutoReset = false;
        _longPressTimer.Elapsed += OnLongPressTimerElapsed;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Lures ViewModel: {ex.Message}");
            await DisplayAlert("Error", "Failed to load lures data. Please try again.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }

    #region Lure Long Press (Using Pressed/Released)

    private void OnLurePressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            _currentLure = button.CommandParameter;
            _isLongPress = false;
            _longPressTimer.Start();
        }
    }

    private void OnLureReleased(object sender, EventArgs e)
    {
        _longPressTimer.Stop();

        Task.Delay(100).ContinueWith(_ => _currentLure = null);
    }

    private async void OnLongPressTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _isLongPress = true;

        if (_currentLure is not LureDataEntity lure)
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (_viewModel.EditLureCommand?.CanExecute(lure) == true)
                await _viewModel.EditLureCommand.ExecuteAsync(lure);
        });
    }

    #endregion

    #region Lure Tap - View Image

    private async void OnLureClicked(object sender, EventArgs e)
    {
        await Task.Delay(50);

        if (_isLongPress)
        {
            _isLongPress = false;
            return;
        }

        if (sender is not Button button || button.CommandParameter is not LureDataEntity lure)
            return;

        var imagePath = lure.PrimaryImage?.Path;
        if (string.IsNullOrEmpty(imagePath))
            return;

        if (_viewModel.OpenImageCommand?.CanExecute(imagePath) == true)
            _viewModel.OpenImageCommand.Execute(imagePath);
    }

    #endregion
}
