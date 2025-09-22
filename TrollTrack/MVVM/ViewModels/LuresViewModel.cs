namespace TrollTrack.MVVM.ViewModels
{
    public partial class LuresViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public LuresViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            Title = "Lures Page";

        }

        #endregion

    }
}