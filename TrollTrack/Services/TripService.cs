using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public class TripService : ITripService
    {
        private readonly IDatabaseService _databaseService;
        private TripDataEntity? _activeTrip;

        public TripDataEntity? ActiveTrip
        {
            get => _activeTrip;
            private set
            {
                _activeTrip = value;
                ActiveTripChanged?.Invoke(this, _activeTrip);
            }
        }

        public event EventHandler<TripDataEntity?>? ActiveTripChanged;

        public TripService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> StartTripAsync(string tripName)
        {
            try
            {
                ActiveTrip = new TripDataEntity
                {
                    Id = Guid.NewGuid(),
                    TripName = tripName,
                    TripDate = DateTime.Now
                };

                // Save to database if you want persistence
                // await _databaseService.SaveTripAsync(ActiveTrip);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task EndTripAsync()
        {
            ActiveTrip = null;
            await Task.CompletedTask;
        }

        public async Task<List<TripDataEntity>> GetAllTripsAsync()
        {
            // Implement when you add trip database methods
            return await Task.FromResult(new List<TripDataEntity>());
        }

        public async Task<TripDataEntity?> GetTripByIdAsync(Guid id)
        {
            // Implement when you add trip database methods
            return await Task.FromResult<TripDataEntity?>(null);
        }
    }
}