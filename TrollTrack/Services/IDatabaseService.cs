using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public interface IDatabaseService
    {
        #region Catch methods

        Task<int> SaveCatchAsync(CatchData catchData);
        Task<List<CatchData>> GetCatchDataAsync();
        Task<List<CatchData>> GetCatchDataByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<CatchData>> GetTodaysCatchesAsync();
        Task<CatchData?> GetCatchByIdAsync(Guid id);
        Task<int> DeleteCatchAsync(Guid id);
        Task<CatchStatistics> GetCatchStatisticsAsync();
        Task ClearAllCatchDataAsync();
        Task<long> GetDatabaseSizeAsync();
        Task<string?> BackupDatabaseAsync();

        #endregion

        #region Catch methods

        Task<int> SaveLureAsync(LureData lureData);
        Task<List<LureData>> GetAllLureDataAsync();

        #endregion
    }
}
