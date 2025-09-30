using TrollTrack.Features.Shared;

namespace TrollTrack.Features.Analytics
{
    public partial class AnalyticsViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public AnalyticsViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
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
            Debug.WriteLine("Starting analytics initialization...");
            IsInitializing = true;

            // Update Title
            Title = "Anlytics";
        }

        #endregion

    }
}