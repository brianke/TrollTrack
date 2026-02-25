namespace TrollTrack.Features.RodSetup;

public partial class LureFilterPopup : ContentPage
{
    public LureFilterPopup(RodSetupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }
}