using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
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

        [ObservableProperty]
        private DiverDataEntity? _selectedDiver;

        [ObservableProperty]
        private int _lineOut = 0;

        /// <summary>
        /// Computed property to check if a lure has been selected
        /// </summary>
        public bool HasSelectedLure => SelectedLure != null;

        /// <summary>
        /// Computed property to check if rod can be added (lure selected and valid line out)
        /// </summary>
        public bool CanAddRod => SelectedLure != null && LineOut > 0;


        #endregion

        #region Events

        /// <summary>
        /// Event raised when a rod setup is confirmed with lure and line out
        /// </summary>
        public event EventHandler<RodSetupData>? RodSetupConfirmed;

        #endregion

        #region Constructor

        public RodSetupViewModel(ILocationService locationService, IDatabaseService databaseService)
            : base(locationService, databaseService)
        {
            Title = "Select Lure";
        }

        #endregion

        #region Property Change Handlers

        /// <summary>
        /// Handle when selected lure changes
        /// </summary>
        partial void OnSelectedLureChanged(LureDataEntity? value)
        {
            OnPropertyChanged(nameof(HasSelectedLure));
            OnPropertyChanged(nameof(CanAddRod));
            Debug.WriteLine($"Selected lure changed: {value?.Manufacturer} - {value?.Color}");
        }

        /// <summary>
        /// Handle when line out changes
        /// </summary>
        partial void OnLineOutChanged(int value)
        {
            OnPropertyChanged(nameof(CanAddRod));
            Debug.WriteLine($"Line out changed: {value} feet");
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
        /// Confirm the rod setup and raise the event
        /// </summary>
        public void ConfirmRodSetup()
        {
            if (SelectedLure == null)
            {
                Debug.WriteLine("ERROR: ConfirmRodSetup called with no lure selected");
                return;
            }

            if (LineOut <= 0)
            {
                Debug.WriteLine("ERROR: ConfirmRodSetup called with invalid line out");
                return;
            }

            var rodSetupData = new RodSetupData
            {
                Lure = SelectedLure,
                LineOut = LineOut
            };

            Debug.WriteLine($"Rod setup confirmed: {SelectedLure.Manufacturer} - {SelectedLure.Color}, Line Out: {LineOut} feet");

            RodSetupConfirmed?.Invoke(this, rodSetupData);
        }

        /// <summary>
        /// Cancel the rod setup
        /// </summary>
        public void CancelRodSetup()
        {
            Debug.WriteLine("Rod setup cancelled");
            SelectedLure = null;
            LineOut = 0;
        }

        #endregion
    }

    /// <summary>
    /// Data class to pass rod setup information
    /// </summary>
    public class RodSetupData
    {
        public LureDataEntity Lure { get; set; } = null!;

        public int LineOut { get; set; }
    }
}