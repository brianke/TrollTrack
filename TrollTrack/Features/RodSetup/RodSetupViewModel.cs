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
        private LuresViewModel _luresVM;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddRodCommand))]
        [NotifyPropertyChangedFor(nameof(HasSelectedLure))]
        private LureDataEntity? _selectedLure;

        [ObservableProperty]
        private DiverDataEntity? _selectedDiver;

        [ObservableProperty]
        private int _id = 0;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddRodCommand))]
        private string _lineOutText = string.Empty;

        private static int ParseLineOut(string? text) => int.TryParse(text, out var v) ? v : 0;

        [ObservableProperty]
        private string _addButtonText = "Add Rod";

        [ObservableProperty]
        private string _currentSetup = string.Empty;

        /// <summary>
        /// Computed property to check if a lure has been selected
        /// </summary>
        public bool HasSelectedLure => SelectedLure != null;

        #endregion

        #region Events

        /// <summary>
        /// Event raised when a rod setup is confirmed with lure and line out
        /// </summary>
        public event EventHandler<RodSetupEntity>? RodSetupConfirmed;

        #endregion

        #region Constructor

        public RodSetupViewModel(ILocationService locationService, IDatabaseService databaseService, LuresViewModel luresViewModel)
            : base(locationService, databaseService)
        {
            Title = "Add New Rod";
            _luresVM = luresViewModel;
            //_ = InitializeAsync();

        }

        #endregion


        #region Methods

        /// <summary>
        /// Method to check if rod can be added (lure selected and valid line out)
        /// This is used by the RelayCommand's CanExecute
        /// NOTE: This MUST be a method, not a property, for [RelayCommand(CanExecute = ...)] to work
        /// </summary>
        private bool CanAddRod()
        {
            var lineOut = ParseLineOut(LineOutText);
            return SelectedLure != null && lineOut > 0;
        }

        public Task InitializeAsync()
        {
            // set SelectedDiver to "Not Used" here since it is not required for a RodSetup
            // Don't need to set SelectedLure as it is required for a RodSetup and will be there when new RodSetup is created
            //if (SelectedDiver != null) return;

            //SelectedDiver = await _databaseService.GetDiverByIdAsync(new Guid("68E2F4AD-23EB-4A4C-932E-7886362532E6"));

            return Task.CompletedTask;
        }

        /*
                /// <summary>
                /// Load available lures from the database
                /// </summary>
                public async Task LoadLuresAsync()
                {
                    //await ExecuteSafelyAsync(async () =>
                    //{
                    IsLoading = true;

                    var lureList = await BaseDatabaseService.GetAllLureDataAsync();

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
                /// Load available divers from the database
                /// </summary>
                public async Task LoadDiversAsync()
                {
                    //await ExecuteSafelyAsync(async () =>
                    //{
                    IsLoading = true;

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
                        foreach (var diver in diverList)
                        {
                            Divers.Add(diver);
                        }
                    });

                    Debug.WriteLine($"Loaded {diverList.Count} divers for selection");
                    IsLoading = false;
                    //}, "Loading lures...", showErrorAlert: false);
                }
        */

        /// <summary>
        /// Confirm the rod setup and raise the event
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAddRod))]
        public async Task AddRodAsync()
        {
            if (SelectedLure == null)
            {
                Debug.WriteLine("ERROR: ConfirmRodSetup called with no lure selected");
                return;
            }

            if (ParseLineOut(LineOutText) <= 0)
            {
                Debug.WriteLine("ERROR: ConfirmRodSetup called with invalid line out");
                return;
            }

            var rodSetupData = new RodSetupEntity
            {
                Id = Id,
                Name = Name,
                Lure = SelectedLure,
                Diver = SelectedDiver,
                LineOut = ParseLineOut(LineOutText)
            };

            Debug.WriteLine($"Rod setup confirmed: {SelectedLure.Manufacturer} - {SelectedLure.Description}, Line Out: {ParseLineOut(LineOutText)} feet");

            RodSetupConfirmed?.Invoke(this, rodSetupData);

            // Close the popup
            await Shell.Current.Navigation.PopModalAsync();

        }

        /// <summary>
        /// Cancel the rod setup
        /// </summary>
        [RelayCommand]
        public async Task CancelRodAsync()
        {
            Debug.WriteLine("Rod setup cancelled");
            LineOutText = String.Empty;
            SelectedDiver = null;
            SelectedLure = null;

            // Close the popup
            await Shell.Current.Navigation.PopModalAsync();

        }

        #endregion
    }
}