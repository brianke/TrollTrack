using System.Collections.ObjectModel;
using System.Diagnostics;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.RodSetup
{
    /// <summary>
    /// ViewModel for the lure selection popup
    /// </summary>
    public partial class RodSetupViewModel : BaseViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<LureDataEntity> _lures = new();

        [ObservableProperty]
        private LureDataEntity? _selectedLure;

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a lure is selected
        /// </summary>
        public event EventHandler<LureDataEntity>? LureSelected;

        #endregion

        #region Constructor

        public RodSetupViewModel(ILocationService locationService, IDatabaseService databaseService)
            : base(locationService, databaseService)
        {
            Title = "Select Lure";
        }

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                IsInitializing = true;
                // Load active trip
                await LoadLuresAsync();

                //// Load catches for active trip or all catches
                //await LoadCatchesAsync();

                IsInitializing = false;
            }, "Initializing catches...");
        }

        /// <summary>
        /// Load available lures from the database
        /// </summary>
        public async Task LoadLuresAsync()
        {
            //await ExecuteSafelyAsync(async () =>
            //{
                IsLoading = true;

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
            //}, "Loading lures...", showErrorAlert: false);
        }

        /// <summary>
        /// Select a lure and raise the LureSelected event
        /// </summary>
        public void SelectLure(LureDataEntity lure)
        {
            SelectedLure = lure;
            LureSelected?.Invoke(this, lure);
            Debug.WriteLine($"Lure selected: {lure.Manufacturer} - {lure.Color}");
        }

        #endregion
    }
}