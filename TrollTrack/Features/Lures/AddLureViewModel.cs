using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures;

public partial class AddLureViewModel : BaseViewModel
{
    #region Observable Properties

    private readonly NotifyCollectionChangedEventHandler _frontColorsChangedHandler;
    private readonly NotifyCollectionChangedEventHandler _backColorsChangedHandler;

    [ObservableProperty]
    private string _addButtonText = "Add Lure";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
    private string _manufacturer = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
    private LureTypes _selectedLureType = LureTypes.NA;

    [ObservableProperty]
    private ObservableCollection<LureTypes> _lureTypeList = Enum.GetValues<LureTypes>()
        .Cast<LureTypes>()
        .ToObservableCollection();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
    private string _lureDescription = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
    private LureBuoyancys _selectedBuoyancy = LureBuoyancys.NA;

    [ObservableProperty]
    private ObservableCollection<LureBuoyancys> _buoyancyList = Enum.GetValues<LureBuoyancys>()
        .Cast<LureBuoyancys>()
        .ToObservableCollection();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
    private string _lengthText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
    private string _weightText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<LureImageEntity> _lureImages = new();

    // Selected "primary" image id for this lure
    [ObservableProperty]
    private Guid _primaryImageId = Guid.Empty;

    // Color picker modal state
    [ObservableProperty]
    private bool _isColorPickerVisible;

    [ObservableProperty]
    private string _colorPickerTitle = "Select Colors";

    // What the modal displays
    [ObservableProperty]
    private ObservableCollection<ColorGroup> _groupedColorOptions = new();

    // Selected colors (front/back)
    [ObservableProperty]
    private ObservableCollection<LureColor> _selectedFrontColors = new();

    [ObservableProperty]
    private ObservableCollection<LureColor> _selectedBackColors = new();

    // Text shown on AddLurePopup
    [ObservableProperty]
    private string _frontColorsDisplay = string.Empty;

    [ObservableProperty]
    private string _backColorsDisplay = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPickingBack))]
    private bool _isPickingFront = true;

    public bool IsPickingBack => !IsPickingFront;
    #endregion

    #region Events

    public event EventHandler<LureDataEntity>? AddLureConfirmed;

    #endregion

    public AddLureViewModel(ILocationService locationService, IDatabaseService databaseService)
        : base(locationService, databaseService)
    {
        Title = "Add New Lure";

        _frontColorsChangedHandler = (_, __) => OnFrontColorsChanged();
        _backColorsChangedHandler = (_, __) => OnBackColorsChanged();

        SelectedFrontColors.CollectionChanged += _frontColorsChangedHandler;
        SelectedBackColors.CollectionChanged += _backColorsChangedHandler;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    private static double ParseLengthOrWeight(string? text) => double.TryParse(text, out var v) ? v : 0;

    public bool CanAddLure() =>
        !string.IsNullOrWhiteSpace(Manufacturer)
        && !string.IsNullOrWhiteSpace(LureDescription)
        && Enum.IsDefined<LureBuoyancys>(SelectedBuoyancy);

    #region Image helpers

    private static async Task<string> PersistToAppDataAsync(FileResult file)
    {
        var folder = Path.Combine(FileSystem.AppDataDirectory, "lure_images");
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".jpg";

        var destPath = Path.Combine(folder, $"{Guid.NewGuid():N}{ext}");

        await using var src = await file.OpenReadAsync();
        await using var dest = File.OpenWrite(destPath);
        await src.CopyToAsync(dest);

        return destPath;
    }

    private void EnsurePrimaryIsSet()
    {
        if (PrimaryImageId != Guid.Empty)
            return;

        var first = LureImages.FirstOrDefault();
        PrimaryImageId = first?.Id ?? Guid.Empty;
    }

    #endregion

    #region Image Commands

    [RelayCommand]
    private async Task PickLureImagesAsync()
    {
        try
        {
            var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select lure images",
                FileTypes = FilePickerFileType.Images
            });

            if (results == null)
                return;

            foreach (var file in results)
            {
                var savedPath = await PersistToAppDataAsync(file);

                var img = new LureImageEntity { Path = savedPath };
                LureImages.Add(img);

                // If nothing selected yet, first image becomes primary
                if (PrimaryImageId == Guid.Empty)
                    PrimaryImageId = img.Id;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PickLureImagesAsync error: {ex}");
            await Shell.Current.DisplayAlert("Error", "Failed to pick images.", "OK");
        }
    }

    [RelayCommand]
    private void RemoveLureImage(LureImageEntity? image)
    {
        if (image == null) return;

        var match = LureImages.FirstOrDefault(x => x.Id == image.Id);
        if (match != null)
            LureImages.Remove(match);
    }

    [RelayCommand]
    private async Task TakeLurePhotosAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Shell.Current.DisplayAlert("Camera", "Camera capture is not supported on this device/emulator.", "OK");
                return;
            }

            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permission Needed", "Camera permission was not granted.", "OK");
                return;
            }

            bool addAnother = true;
            while (addAnother)
            {
                FileResult? photo;
                try
                {
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CapturePhotoAsync failed: {ex}");
                    await Shell.Current.DisplayAlert("Failed to take photo", ex.Message, "OK");
                    return;
                }

                if (photo == null)
                    return;

                var savedPath = await PersistToAppDataAsync(photo);
                var img = new LureImageEntity { Path = savedPath };
                LureImages.Add(img);

                if (PrimaryImageId == Guid.Empty)
                    PrimaryImageId = img.Id;

                addAnother = await Shell.Current.DisplayAlert("Photo added", "Take another photo?", "Yes", "No");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TakeLurePhotosAsync error: {ex}");
            await Shell.Current.DisplayAlert("Failed to take photo", ex.Message, "OK");
        }
    }


    [RelayCommand]
    private void ClearLureImages()
    {
        LureImages.Clear();
        PrimaryImageId = Guid.Empty;
    }

    [RelayCommand]
    private void SetPrimaryLureImage(LureImageEntity? image)
    {
        if (image == null) return;
        PrimaryImageId = image.Id;
    }

    #endregion

    #region Color Picker helpers
    private void RefreshColorDisplays()
    {
        FrontColorsDisplay = SelectedFrontColors.Count == 0
            ? "(none)"
            : string.Join(", ", SelectedFrontColors);

        BackColorsDisplay = SelectedBackColors.Count == 0
            ? "(none)"
            : string.Join(", ", SelectedBackColors);
    }

    private void OnFrontColorsChanged()
    {
        OnPropertyChanged(nameof(SelectedFrontColors));
        RefreshColorDisplays();
    }

    private void OnBackColorsChanged()
    {
        OnPropertyChanged(nameof(SelectedBackColors));
        RefreshColorDisplays();
    }

    partial void OnSelectedFrontColorsChanging(ObservableCollection<LureColor> value)
    {
        if (_frontColorsChangedHandler != null)
            SelectedFrontColors.CollectionChanged -= _frontColorsChangedHandler;
    }

    partial void OnSelectedFrontColorsChanged(ObservableCollection<LureColor> value)
    {
        if (_frontColorsChangedHandler != null)
            value.CollectionChanged += _frontColorsChangedHandler;

        OnFrontColorsChanged();
    }

    partial void OnSelectedBackColorsChanging(ObservableCollection<LureColor> value)
    {
        if (_backColorsChangedHandler != null)
            SelectedBackColors.CollectionChanged -= _backColorsChangedHandler;
    }

    partial void OnSelectedBackColorsChanged(ObservableCollection<LureColor> value)
    {
        if (_backColorsChangedHandler != null)
            value.CollectionChanged += _backColorsChangedHandler;

        OnBackColorsChanged();
    }

    private IEnumerable<LureColor> AllPickableColors =>
        Enum.GetValues<LureColor>().Where(c => c != LureColor.NA); // enum is in LureColorEntity.cs

    private void BuildColorOptions()
    {
        var selected = IsPickingFront ? SelectedFrontColors : SelectedBackColors;

        var allColorOptions = AllPickableColors
            .Select(c => new LureColorOption(c, selected.Contains(c)))
            .ToList();

        var grouped = allColorOptions
            .GroupBy(o => ColorPalette.GetCategory(o.Color))
            .Select(g => new ColorGroup(g.Key, g.OrderBy(o => o.Color.ToString())));

        GroupedColorOptions = new ObservableCollection<ColorGroup>(grouped);
    }

    #endregion Color Picker helpers

    #region Color Picker commands

    [RelayCommand]
    private void OpenFrontColors()
    {
        IsPickingFront = true;
        ColorPickerTitle = "Select Front Colors";
        BuildColorOptions();
        IsColorPickerVisible = true;
    }

    [RelayCommand]
    private void OpenBackColors()
    {
        IsPickingFront = false;
        ColorPickerTitle = "Select Back Colors";
        BuildColorOptions();
        IsColorPickerVisible = true;
    }

    [RelayCommand]
    private void CloseColorPicker()
    {
        IsColorPickerVisible = false;
    }

    [RelayCommand]
    private void ToggleColor(LureColorOption? option)
    {
        if (option == null) return;

        option.IsSelected = !option.IsSelected;

        var selectedColors = GroupedColorOptions
            .SelectMany(g => g)
            .Where(c => c.IsSelected)
            .Select(c => c.Color)
            .ToList();

        if (IsPickingFront)
            SelectedFrontColors = new ObservableCollection<LureColor>(selectedColors);
        else
            SelectedBackColors = new ObservableCollection<LureColor>(selectedColors);

        RefreshColorDisplays();
    }

    #endregion Color Picker commands

    #region Add/Cancel

    [RelayCommand(CanExecute = nameof(CanAddLure))]
    public async Task AddLureAsync()
    {
        var images = LureImages.ToList();

        var primaryId = PrimaryImageId;
        if (primaryId == Guid.Empty && images.Count > 0)
            primaryId = images[0].Id;

        var lureData = new LureDataEntity
        {
            Id = Guid.Empty,
            Manufacturer = Manufacturer,
            LureType = SelectedLureType,
            Description = LureDescription,
            Buoyancy = SelectedBuoyancy,
            Length = ParseLengthOrWeight(LengthText),
            Weight = ParseLengthOrWeight(WeightText),
            Images = images,

            // Only if your LureDataEntity has this property (you said you are using it)
            PrimaryImageId = primaryId,

            // Save colors back to the DB-backed JSON columns via the JSON-facing helpers
            FrontColors = SelectedFrontColors.Select(c => c.ToString()).ToList(),
            BackColors = SelectedBackColors.Select(c => c.ToString()).ToList()
        };

        AddLureConfirmed?.Invoke(this, lureData);
        await Shell.Current.Navigation.PopModalAsync();
    }


    [RelayCommand]
    public async Task CancelAddLureAsync()
    {
        ResetForNewLure();
        await Shell.Current.Navigation.PopModalAsync();
    }

    public void ResetForNewLure()
    {
        Manufacturer = string.Empty;
        SelectedLureType = LureTypes.NA;
        LureDescription = string.Empty;
        SelectedBuoyancy = LureBuoyancys.NA;
        LengthText = string.Empty;
        WeightText = string.Empty;

        LureImages.Clear();
        PrimaryImageId = Guid.Empty;

        SelectedFrontColors = new ObservableCollection<LureColor>();
        SelectedBackColors = new ObservableCollection<LureColor>();
    }

    #endregion
    /// <summary>
    /// Class for displaying Lure Color Options during setup of new lure
    /// </summary>
    public partial class LureColorOption : ObservableObject
    {
        public LureColorOption(LureColor color, bool isSelected)
        {
            Color = color;
            _isSelected = isSelected;
        }

        public LureColor Color { get; }

        [ObservableProperty]
        private bool _isSelected;
    }

    public class ColorGroup : ObservableCollection<LureColorOption>
    {
        public string Name { get; }

        public ColorGroup(string name, IEnumerable<LureColorOption> colors) : base(colors)
        {
            Name = name;
        }
    }
}


