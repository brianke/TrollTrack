using TrollTrack.Features.Catches;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public interface IDatabaseService
    {
        #region Catch methods

        Task<int> SaveCatchAsync(CatchDataEntity catchData);
        Task<List<CatchDataEntity>> GetCatchDataAsync();
        Task<List<CatchDataEntity>> GetCatchDataByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<CatchDataEntity>> GetTodaysCatchesAsync();
        Task<CatchDataEntity?> GetCatchByIdAsync(Guid id);
        Task<int> DeleteCatchAsync(Guid id);
        Task<CatchStatistics> GetCatchStatisticsAsync();
        Task ClearAllCatchDataAsync();
        Task<long> GetDatabaseSizeAsync();
        Task<string?> BackupDatabaseAsync();

        #endregion

        #region Lure methods

        Task<int> SaveLureAsync(LureDataEntity lureData);
        Task<List<LureDataEntity>> GetAllLureDataAsync();

        #endregion
    }
}
