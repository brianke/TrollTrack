namespace TrollTrack.MVVM.ViewModels
{
    public partial class LuresViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public LuresViewModel(ILocationService locationService) : base(locationService)
        {
            Title = "Lures Page";

        }

        #endregion

    }
}