namespace TrollTrack.MVVM.ViewModels
{
    public partial class ProgramsViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public ProgramsViewModel(ILocationService locationService) : base(locationService)
        {
            Title = "Programs Page";

        }

        #endregion

    }
}