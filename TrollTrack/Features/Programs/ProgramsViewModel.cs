using TrollTrack.Fetures.Shared;

namespace TrollTrack.Features.Programs
{
    public partial class ProgramsViewModel : BaseViewModel
    {        
        #region Observable Properties


        #endregion


        #region Constructor

        public ProgramsViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
        {
            Title = "Programs Page";

        }

        #endregion

    }
}