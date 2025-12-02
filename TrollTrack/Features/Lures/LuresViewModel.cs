using System.Text.Json;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures
{
    public partial class LuresViewModel : BaseViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        public ObservableCollection<LureDataEntity> lures = new();

        [ObservableProperty]
        public ObservableCollection<DiverDataEntity> divers = new();

        // Modal properties
        [ObservableProperty]
        private bool isImageModalVisible;

        [ObservableProperty]
        private string selectedImagePath = string.Empty;

        #endregion


        #region Constructor

        public LuresViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            //OpenImageCommand = new RelayCommand<string>(OpenImage);
            //CloseImageCommand = new RelayCommand(CloseImage);

            // Load data when ViewModel is created
            //_ = InitializeAsync();

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
            //await ExecuteSafelyAsync(async () =>
            //{
                Debug.WriteLine("Starting lures initialization...");
                IsInitializing = true;

            // Load lures when ViewModel is created
            await LoadLuresAsync();

            // Load divers when ViewModel is created
            await LoadDiversAsync();

            // Update Title
            Title = "Lures";
            //}, "Initializing lures...", showErrorAlert: false);
        }

        #endregion

        #region Commands

        //public ICommand OpenImageCommand { get; }
        //public ICommand CloseImageCommand { get; }

        public async Task LoadLuresAsync()
        {
            var lureList = await _databaseService.GetAllLureDataAsync();

            if (lureList == null || !lureList.Any())
            {
                Debug.WriteLine("No lures found in database");
                await ShowAlertAsync("No Lures", "No lures found. Please add lures first from the Lures tab.");
                IsLoading = false;
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
        }


        public async Task LoadDiversAsync()
        {
            var diverList = await _databaseService.GetAllDiversAsync();

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
        #endregion

    }
}