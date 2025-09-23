namespace TrollTrack.MVVM.ViewModels
{
    public partial class LuresViewModel : BaseViewModel
    {
        #region Observable Properties

        [ObservableProperty]
        public ObservableCollection<LureData> lures = new ();

        #endregion


        #region Constructor

        public LuresViewModel(ILocationService locationService, IDatabaseService databaseService) : base(locationService, databaseService)
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
            await ExecuteSafelyAsync(async () =>
            {
                Debug.WriteLine("Starting catches initialization...");
                IsInitializing = true;

                // Load catches when ViewModel is created
                _ = LoadLuresAsync();


                // Update Title
                Title = "Lures";
            }, "Initializing lures...", showErrorAlert: false);
        }

        #endregion

        #region Commands
        private async Task LoadCatchesAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                IsLoading = true;

                var allCatches = await _databaseService.GetCatchDataAsync();
                var todaysCatchList = await _databaseService.GetTodaysCatchesAsync();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Catches.Clear();
                    foreach (var catchData in allCatches)
                    {
                        Catches.Add(catchData);
                    }

                    TotalCatches = allCatches.Count;
                    TodaysCatches = todaysCatchList.Count;
                });

                System.Diagnostics.Debug.WriteLine($"Loaded {allCatches.Count} catch records");
            }, "Loading catches...", showErrorAlert: false);
        }

        #endregion

    }
}