namespace TrollTrack.MVVM.ViewModels
{
    public partial class AnalyticsViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public AnalyticsViewModel(ILocationService locationService) : base(locationService)
        {
            Title = "Analytics Page";

        }

        #endregion

    }
}