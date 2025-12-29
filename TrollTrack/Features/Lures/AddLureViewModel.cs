using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures;

public partial class AddLureViewModel : BaseViewModel
{
    #region Observable Properties

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

    #endregion

    #region Events

    public event EventHandler<LureDataEntity>? AddLureConfirmed;

    #endregion

    public AddLureViewModel(ILocationService locationService, IDatabaseService databaseService)
        : base(locationService, databaseService)
    {
        Title = "Add New Lure";
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

        var wasPrimary = (image.Id == PrimaryImageId);
        LureImages.Remove(image);

        if (wasPrimary)
            PrimaryImageId = LureImages.FirstOrDefault()?.Id ?? Guid.Empty;
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
            PrimaryImageId = primaryId
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
    }

    #endregion
}
