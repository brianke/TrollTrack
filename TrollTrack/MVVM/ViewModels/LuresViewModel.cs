using System.Text.Json;

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
                Debug.WriteLine("Starting lures initialization...");
                IsInitializing = true;

                // Load lures when ViewModel is created
                _ = LoadLuresAsync();


                // Update Title
                Title = "Lures";
            }, "Initializing lures...", showErrorAlert: false);
        }

        #endregion

        #region Commands
        private async Task LoadLuresAsync()
        {
            await ExecuteSafelyAsync(async () =>
            {
                IsLoading = true;

                using var stream = await FileSystem.OpenAppPackageFileAsync("lures.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var lureList = JsonSerializer.Deserialize<List<LureData>>(json);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Lures.Clear();
                    foreach (var lure in lureList)
                    {
                        Lures.Add(lure);
                    }

                });

                System.Diagnostics.Debug.WriteLine($"Loaded {lureList.Count} lures");
            }, "Loading lures...", showErrorAlert: false);
        }

        #endregion

    }
}