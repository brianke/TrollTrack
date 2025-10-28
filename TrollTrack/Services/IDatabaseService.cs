using TrollTrack.Features.Catches;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public interface IDatabaseService
    {
        #region Trip methods

        Task<int> SaveTripAsync(TripDataEntity trip);
        Task<int> UpdateTripAsync(TripDataEntity tripData);
        Task<TripDataEntity?> GetTripByIdAsync(Guid id);
        Task<TripDataEntity?> GetActiveTripAsync();
        Task<List<TripDataEntity>> GetAllTripsAsync();
        Task<List<TripDataEntity>> GetRecentTripsAsync(int count = 10);
        Task<List<TripDataEntity>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> DeleteTripAsync(Guid id);

        #endregion

        #region Catch methods

        Task<int> SaveCatchAsync(CatchDataEntity catchData);
        Task<List<CatchDataEntity>> GetCatchDataAsync();
        Task<List<CatchDataEntity>> GetCatchDataByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<CatchDataEntity>> GetCatchesForTripAsync(Guid tripId);
        Task<List<CatchDataEntity>> GetTodaysCatchesAsync();
        Task<CatchDataEntity?> GetCatchByIdAsync(Guid id);
        Task<int> DeleteCatchAsync(Guid id);
        Task<CatchStatistics> GetCatchStatisticsAsync();
        Task ClearAllCatchDataAsync();

        #endregion

        #region Lure methods

        Task<int> SaveLureAsync(LureDataEntity lureData);
        Task<List<LureDataEntity>> GetAllLureDataAsync();

        #endregion

        #region Custom Clarity methods
        Task<int> SaveCustomClarityAsync(string clarity);
        Task<List<string>> GetCustomClaritiesAsync();
        Task<int> DeleteCustomClarityAsync(string clarity);

        #endregion

        #region Database maintenance

        Task<long> GetDatabaseSizeAsync();
        Task<string?> BackupDatabaseAsync();
        Task ClearAllTablesAsync();

        #endregion
    }
}