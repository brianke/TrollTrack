// FilterOption.cs
namespace TrollTrack.Features.RodSetup;

public partial class FilterOption : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public string Value { get; }
    public string DisplayName { get; }

    public FilterOption(string value, string displayName, bool isSelected = false)
    {
        Value = value;
        DisplayName = displayName;
        IsSelected = isSelected;
    }
}