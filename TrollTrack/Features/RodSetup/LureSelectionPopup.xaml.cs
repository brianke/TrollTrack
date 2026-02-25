using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.RodSetup;

public partial class LureSelectionPopup : ContentPage
{
    private readonly RodSetupViewModel _viewModel;

    public LureSelectionPopup(RodSetupViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = viewModel;
    }

    private async void OnLureSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.FirstOrDefault() is LureDataEntity lure)
        {
            _viewModel.SelectedLure = lure;
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}