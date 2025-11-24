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
                    TripDate = DateTime.Now,
                    IsActive = true
                };

                // Save to database if you want persistence
                await _databaseService.SaveTripAsync(ActiveTrip);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task EndTripAsync()
        {
            try
            {
                if (ActiveTrip != null)
                {
                    ActiveTrip.IsActive = false;
                    ActiveTrip.EndTime = DateTime.Now;

                    // Update in database
                    await _databaseService.UpdateTripAsync(ActiveTrip);
                }

                ActiveTrip = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ending trip: {ex.Message}");
            }
        }

        public async Task<List<TripDataEntity>> GetAllTripsAsync()
        {
            try
            {
                // Call the DatabaseService method to get all trips
                return await _databaseService.GetAllTripsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all trips: {ex.Message}");
                return await Task.FromResult(new List<TripDataEntity>());
            }
        }

        public async Task<TripDataEntity?> GetTripByIdAsync(Guid id)
        {
            try
            {
                // Call the DatabaseService method to get trip by ID
                return await _databaseService.GetTripByIdAsync(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting trip by ID: {ex.Message}");
                return await Task.FromResult<TripDataEntity?>(null);
            }
        }

        public async Task<TripDataEntity?> GetActiveTripAsync()
        {
            try
            {
                // Get active trip from database
                var activeTrip = await _databaseService.GetActiveTripAsync();

                if (activeTrip != null)
                {
                    ActiveTrip = activeTrip;
                }

                return activeTrip;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting active trip: {ex.Message}");
                return await Task.FromResult<TripDataEntity?>(null);
            }
        }
    }
}