using CommunityToolkit.Maui.Core.Extensions;
using System.Text.Json;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures
{
    public partial class LuresViewModel : BaseViewModel
    {
        private AddLureViewModel _addLureVM;

        #region Observable Properties

        [ObservableProperty]
        public ObservableCollection<LureDataEntity> lures = new();

        [ObservableProperty]
        public ObservableCollection<DiverDataEntity> divers = new();

        [ObservableProperty]
        public ObservableCollection<string> buoyancyList = Enum.GetValues<LureBuoyancys>()
            .Select(x => x.ToDisplayString())
            .ToObservableCollection();

        // Modal properties
        [ObservableProperty]
        private bool isImageModalVisible;

        [ObservableProperty]
        private string selectedImagePath = string.Empty;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the shared lures collection is reloaded (add, edit, or refresh).
        /// </summary>
        public event EventHandler? LuresUpdated;

        #endregion



        #region Constructor

        public LuresViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            //OpenImageCommand = new RelayCommand<string>(OpenImage);
            //CloseImageCommand = new RelayCommand(CloseImage);

            // Create the add lure view model
            _addLureVM = new AddLureViewModel(BaseLocationService, BaseDatabaseService);

            // Load data when ViewModel is created
            _ = InitializeAsync();

            // Add this to verify the command exists
            //Debug.WriteLine($"OpenImageCommand is null: {OpenImageCommand == null}");

        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the data needed for the catches 
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            try
            {
                await ExecuteSafelyAsync(async () =>
                {
                    Debug.WriteLine("Starting lures initialization...");
                    IsInitializing = true;

                    // Load lures when ViewModel is created
                    await LoadLuresAsync();

                    // Load divers when ViewModel is created
                    await LoadDiversAsync();

                    // Update Title
                    Title = "Lures";
                }, "Initializing lures...", showErrorAlert: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LuresViewModel InitializeAsync() failed: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }

        }

        #endregion

        #region Commands


        public Task RefreshLuresAsync() => LoadLuresAsync(showEmptyAlert: false);

        public async Task LoadLuresAsync(bool showEmptyAlert = true)
        {
            var lureList = await BaseDatabaseService.GetAllLureDataAsync();

            if (lureList == null || !lureList.Any())
            {
                Debug.WriteLine("No lures found in database");
                await MainThread.InvokeOnMainThreadAsync(() => Lures.Clear());
                if (showEmptyAlert)
                    await ShowAlertAsync("No Lures", "No lures found. Please add lures first from the Lures tab.");
                IsLoading = false;
                LuresUpdated?.Invoke(this, EventArgs.Empty);
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Lures.Clear();
                foreach (var lure in lureList)
                {
                    Lures.Add(lure);
                }
            });

            Debug.WriteLine($"Loaded {lureList.Count} lures for selection");
            IsLoading = false;
            LuresUpdated?.Invoke(this, EventArgs.Empty);
        }


        public async Task LoadDiversAsync()
        {
            var diverList = await BaseDatabaseService.GetAllDiversAsync();

            if (diverList == null || !diverList.Any())
            {
                Debug.WriteLine("No divers found in database");
                await ShowAlertAsync("No Divers", "No divers found. Please add divers first from the Lures tab.");
                IsLoading = false;
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Divers.Clear();
                foreach (var lure in diverList)
                {
                    Divers.Add(lure);
                }
            });

            Debug.WriteLine($"Loaded {diverList.Count} divers for selection");
            IsLoading = false;
        }

        [RelayCommand]
        private void OpenImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            SelectedImagePath = imagePath;
            IsImageModalVisible = true;
        }

        [RelayCommand]
        private void CloseImage()
        {
            IsImageModalVisible = false;
            SelectedImagePath = "";
        }

        [RelayCommand]
        private async Task AddLure() => await ShowLurePopupAsync();

        [RelayCommand]
        private async Task EditLure(LureDataEntity? lure)
        {
            if (lure == null) return;
            await ShowLurePopupAsync(lure);
        }

        private async Task ShowLurePopupAsync(LureDataEntity? lureToEdit = null)
        {
            try
            {
                if (lureToEdit == null)
                    _addLureVM.ResetForNewLure();
                else
                    _addLureVM.LoadForEdit(lureToEdit);

                _addLureVM.AddLureConfirmed += OnAddLureConfirmed;

                var popup = new AddLurePopup(_addLureVM);
                var page = Application.Current?.Windows[0]?.Page;
                if (page?.Navigation == null) return;

                await page.Navigation.PushModalAsync(popup);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR opening lure popup: {ex.Message}");
                await ShowAlertAsync("Error", "Failed to open lure editor. Please try again.");
            }
        }

        /// <summary>
        /// Handle add lure confirmation from the popup
        /// </summary>
        private async void OnAddLureConfirmed(object? sender, LureDataEntity lureEntity)
        {
            try
            {
                // Unsubscribe from the event
                if (_addLureVM != null)
                {
                    _addLureVM.AddLureConfirmed -= OnAddLureConfirmed;
                }

                Debug.WriteLine($"=== Add Lure Confirmed ===");
                //Debug.WriteLine($"Lure: {rodSetupEntity.Lure!.Manufacturer} - {rodSetupEntity.Lure.Color}");   // Lure cannot be null here so added (!) ignore
                //Debug.WriteLine($"Line Out: {rodSetupEntity.LineOut} feet");

                LureDataEntity newLure;

                if (lureEntity.Id == Guid.Empty)
                {
                    // Create a new rod with the selected lure and line out
                    newLure = new LureDataEntity
                    {
                        Manufacturer = lureEntity.Manufacturer,
                        LureType = lureEntity.LureType,
                        Description = lureEntity.Description,
                        Buoyancy = lureEntity.Buoyancy,
                        Length = lureEntity.Length,
                        Weight = lureEntity.Weight,
                        FrontColors = lureEntity.FrontColors,
                        BackColors = lureEntity.BackColors,
                        Images = lureEntity.Images,
                        PrimaryImageId = lureEntity.PrimaryImageId,
                    };
                }
                else
                {
                    newLure = new LureDataEntity
                    {
                        Id = lureEntity.Id,
                        Manufacturer = lureEntity.Manufacturer,
                        LureType = lureEntity.LureType,
                        Description = lureEntity.Description,
                        Buoyancy = lureEntity.Buoyancy,
                        Length = lureEntity.Length,
                        Weight = lureEntity.Weight,
                        FrontColors = lureEntity.FrontColors,
                        BackColors = lureEntity.BackColors,
                        Images = lureEntity.Images,
                        PrimaryImageId = lureEntity.PrimaryImageId,
                    };
                }

                //Debug.WriteLine($"Creating rod with Name: {newRod.Name}, LureId: {newRod.LureId}, LineOut: {newRod.LineOut}");

                // Save the rod to the database
                var result = await BaseDatabaseService.SaveLureAsync(newLure);
                Debug.WriteLine($"Database save returned: {result}");

                // Reload rods to show the new one
                await LoadLuresAsync();

                // Verify the rod was added
                Debug.WriteLine($"Total lures after reload: {Lures.Count}");

                var wasEdit = lureEntity.Id != Guid.Empty;
                await ShowAlertAsync("Success",
                    wasEdit
                        ? $"Lure updated:\n{newLure.DisplayName}"
                        : $"Lure added:\n{newLure.DisplayName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! ERROR in OnAddLureConfirmed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await ShowAlertAsync("Error", $"Failed to add lure: {ex.Message}");
            }
        }

        #endregion

    }
}