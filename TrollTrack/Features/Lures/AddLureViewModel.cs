using CommunityToolkit.Maui.Core.Extensions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using TrollTrack.Features.Shared;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Lures
{
    /// <summary>
    /// ViewModel for the add lure popup
    /// </summary>
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
        private ObservableCollection<LureTypes> lureTypeList = Enum.GetValues<LureTypes>()
            .Cast<LureTypes>()
            .ToObservableCollection();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLureCommand))] 
        private string _lureDescription = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLureCommand))] 
        private LureBuoyancys _selectedBuoyancy = LureBuoyancys.NA;

        [ObservableProperty]
        private ObservableCollection<LureBuoyancys> buoyancyList = Enum.GetValues<LureBuoyancys>()
            .Cast<LureBuoyancys>()
            .ToObservableCollection();


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
        private string _lengthText = String.Empty;


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddLureCommand))]
        private string _weightText = String.Empty;

        private static double ParseLengthOrWeight(string? text) => double.TryParse(text, out var v) ? v : 0;


        #endregion


        #region Events

        /// <summary>
        /// Event raised when a add lure is confirmed
        /// </summary>
        public event EventHandler<LureDataEntity>? AddLureConfirmed;

        #endregion

        #region Constructor

        public AddLureViewModel(ILocationService locationService, IDatabaseService databaseService)
            : base(locationService, databaseService)
        {
            Title = "Add New Lure";
            _ = InitializeAsync();

        }

        #endregion

        #region Property Change Handlers
      
        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            return; // Task.CompletedTask;
        }

        /// <summary>
        /// Computed property to check if lure can be added (manufacturer, description, buoyancy set)
        /// </summary>
        public bool CanAddLure() => !string.IsNullOrEmpty(Manufacturer)
            && !string.IsNullOrEmpty(LureDescription)
            && Enum.IsDefined<LureBuoyancys>(SelectedBuoyancy);

        /// <summary>
        /// Confirm the rod setup and raise the event
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAddLure))]
        public async Task AddLureAsync()
        {
            var lureData = new LureDataEntity
            {
                Manufacturer = Manufacturer,
                LureType = SelectedLureType,
                Description = LureDescription,
                Buoyancy = SelectedBuoyancy,
                Length = ParseLengthOrWeight(LengthText),
                Weight = ParseLengthOrWeight(WeightText)
            };

            Debug.WriteLine($"Add lure confirmed: {Manufacturer} - {LureDescription}");

            AddLureConfirmed?.Invoke(this, lureData);

            // Close the popup
            await Shell.Current.Navigation.PopModalAsync();
        }

        /// <summary>
        /// Cancel the rod setup
        /// </summary>
        [RelayCommand]
        public async Task CancelAddLureAsync()
        {
            Manufacturer = string.Empty;
            SelectedLureType = LureTypes.NA;
            LureDescription = string.Empty;
            SelectedBuoyancy = LureBuoyancys.NA;
            LengthText = string.Empty;
            WeightText = string.Empty;

            // Close the popup
            Debug.WriteLine("Add lure cancelled");
            await Shell.Current.Navigation.PopModalAsync();
        }

        #endregion
    }
}