namespace TrollTrack.MVVM.ViewModels
{
    public partial class AnalyticsViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public AnalyticsViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            Title = "Analytics Page";

        }

        #endregion

    }
}