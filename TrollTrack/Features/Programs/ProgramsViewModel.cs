using TrollTrack.Features.Shared;

namespace TrollTrack.Features.Programs
{
    public partial class ProgramsViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public ProgramsViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            // Load data when ViewModel is created
            _ = InitializeAsync();

        }

        #endregion


        #region Initialization

        /// <summary>
        /// Initialize the data needed for the catches 
        /// </summary>
        /// <returns></returns>
        public async Task InitializeAsync()
        {
            Debug.WriteLine("Starting programs initialization...");
            IsInitializing = true;

            // Update Title
            Title = "Programs";
        }

        #endregion

    }
}