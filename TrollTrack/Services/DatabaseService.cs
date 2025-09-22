using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrollTrack.Configuration;
using TrollTrack.MVVM.Models;

namespace TrollTrack.Services
{
    /// <summary>
    /// SQLite database service for managing catch data and other app data
    /// </summary>
    public class DatabaseService : IDatabaseService, IDisposable
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _databasePath;

        public DatabaseService()
        {
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, AppConfig.Constants.DatabaseName);
        }

        /// <summary>
        /// Initialize the database connection and create tables
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_database != null)
                return;

            try
            {
                _database = new SQLiteAsyncConnection(_databasePath);
                await _database.ExecuteAsync("PRAGMA foreign_keys = ON;");

                // Create tables for your existing models
                await _database.CreateTableAsync<CatchDataEntity>();
                await _database.CreateTableAsync<LocationEntity>();
                await _database.CreateTableAsync<ProgramDataEntity>();
                await _database.CreateTableAsync<FishInfoEntity>();

                System.Diagnostics.Debug.WriteLine($"Database initialized at: {_databasePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            if (_database == null)
                await InitializeAsync();

            return _database!;
        }

        #region Catch Data Operations

        /// <summary>
        /// Save a new catch record
        /// </summary>
        public async Task<int> SaveCatchAsync(CatchData catchData)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = ConvertToEntity(catchData);

                await db.InsertOrReplaceWithChildrenAsync(entity, recursive: true);
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving catch: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get all catch records
        /// </summary>
        public async Task<List<CatchData>> GetCatchDataAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.GetAllWithChildrenAsync<CatchDataEntity>(recursive: true);

                var catchDataList = new List<CatchData>();
                foreach (var entity in entities.OrderByDescending(e => e.Timestamp))
                {
                    var catchData = ConvertFromEntity(entity);
                    catchDataList.Add(catchData);
                }

                return catchDataList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting catch data: {ex.Message}");
                return new List<CatchData>();
            }
        }

        /// <summary>
        /// Get catch records for a specific date range
        /// </summary>
        public async Task<List<CatchData>> GetCatchDataByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.GetAllWithChildrenAsync<CatchDataEntity>(c => c.Timestamp >= startDate && c.Timestamp <= endDate, recursive: true);

                var catchDataList = new List<CatchData>();
                foreach (var entity in entities.OrderByDescending(e => e.Timestamp))
                {
                    var catchData = ConvertFromEntity(entity);
                    catchDataList.Add(catchData);
                }

                return catchDataList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting catch data by date range: {ex.Message}");
                return new List<CatchData>();
            }
        }

        /// <summary>
        /// Get today's catches
        /// </summary>
        public async Task<List<CatchData>> GetTodaysCatchesAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await GetCatchDataByDateRangeAsync(today, tomorrow);
        }

        /// <summary>
        /// Get a specific catch by ID
        /// </summary>
        public async Task<CatchData?> GetCatchByIdAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = await db.GetWithChildrenAsync<CatchDataEntity>(id, recursive: true);

                if (entity == null)
                    return null;

                return ConvertFromEntity(entity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting catch by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Delete a catch record
        /// </summary>
        public async Task<int> DeleteCatchAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                return await db.DeleteAsync<CatchDataEntity>(id, recursive: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting catch: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get catch statistics
        /// </summary>
        public async Task<CatchStatistics> GetCatchStatisticsAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var weekAgo = today.AddDays(-7);
                var monthAgo = today.AddDays(-30);

                var totalCatches = await db.Table<CatchDataEntity>().CountAsync();
                var todaysCatches = await db.Table<CatchDataEntity>().Where(c => c.Timestamp >= today && c.Timestamp < tomorrow).CountAsync();
                var weekCatches = await db.Table<CatchDataEntity>().Where(c => c.Timestamp >= weekAgo).CountAsync();
                var monthCatches = await db.Table<CatchDataEntity>().Where(c => c.Timestamp >= monthAgo).CountAsync();

                var lastCatch = await db.Table<CatchDataEntity>().OrderByDescending(c => c.Timestamp).FirstOrDefaultAsync();

                var stats = new CatchStatistics
                {
                    TotalCatches = totalCatches,
                    TodaysCatches = todaysCatches,
                    WeekCatches = weekCatches,
                    MonthCatches = monthCatches,
                    LastCatchDate = lastCatch?.Timestamp ?? DateTime.MinValue
                };

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting statistics: {ex.Message}");
                return new CatchStatistics();
            }
        }

        #endregion

        #region Helper Methods

        private CatchDataEntity ConvertToEntity(CatchData catchData)
        {
            var entity = new CatchDataEntity
            {
                Id = catchData.Id == Guid.Empty ? Guid.NewGuid() : catchData.Id,
                Timestamp = catchData.Timestamp
            };

            if (catchData.Location != null)
            {
                entity.Location = new LocationEntity
                {
                    Id = Guid.NewGuid(),
                    Latitude = catchData.Location.Latitude,
                    Longitude = catchData.Location.Longitude,
                    Altitude = catchData.Location.Altitude,
                    Accuracy = catchData.Location.Accuracy,
                    Course = catchData.Location.Course,
                    Speed = catchData.Location.Speed,
                    Timestamp = catchData.Location.Timestamp
                };
                entity.LocationId = entity.Location.Id;
            }

            if (catchData.CatchProgram != null)
            {
                // TODO: The ProgramData model is incomplete.
                entity.ProgramData = new ProgramDataEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Placeholder Program",
                    Description = "Placeholder Description"
                };
                entity.ProgramDataId = entity.ProgramData.Id;
            }

            if (catchData.FishCaught.HasValue)
            {
                var fishInfo = FishData.GetInfo(catchData.FishCaught.Value);
                entity.FishInfo = new FishInfoEntity
                {
                    Id = fishInfo.Id,
                    CommonName = fishInfo.CommonName,
                    ScientificName = fishInfo.ScientificName,
                    Habitat = fishInfo.Habitat
                };
                entity.FishInfoId = entity.FishInfo.Id;
            }

            return entity;
        }

        private CatchData ConvertFromEntity(CatchDataEntity entity)
        {
            var catchData = new CatchData
            {
                Id = entity.Id,
                Timestamp = entity.Timestamp
            };

            if (entity.Location != null)
            {
                catchData.Location = new Location
                {
                    Latitude = entity.Location.Latitude,
                    Longitude = entity.Location.Longitude,
                    Altitude = entity.Location.Altitude,
                    Accuracy = entity.Location.Accuracy,
                    Course = entity.Location.Course,
                    Speed = entity.Location.Speed,
                    Timestamp = entity.Location.Timestamp
                };
            }

            if (entity.ProgramData != null)
            {
                // TODO: The ProgramData model is incomplete.
                catchData.CatchProgram = new ProgramData();
            }

            if (entity.FishInfo != null && !string.IsNullOrEmpty(entity.FishInfo.CommonName))
            {
                if (Enum.TryParse<FishCommonName>(entity.FishInfo.CommonName, out var fishCommonName))
                {
                    catchData.FishCaught = fishCommonName;
                }
            }

            return catchData;
        }

        #endregion

        #region Database Maintenance

        /// <summary>
        /// Clear all catch data
        /// </summary>
        public async Task ClearAllCatchDataAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                await db.DeleteAllAsync<CatchDataEntity>();
                await db.DeleteAllAsync<LocationEntity>();
                await db.DeleteAllAsync<ProgramDataEntity>();
                await db.DeleteAllAsync<FishInfoEntity>();

                System.Diagnostics.Debug.WriteLine("All catch data cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing catch data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get database file size
        /// </summary>
        public Task<long> GetDatabaseSizeAsync()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    var fileInfo = new FileInfo(_databasePath);
                    return Task.FromResult(fileInfo.Length);
                }
                return Task.FromResult(0L);
            }
            catch
            {
                return Task.FromResult(0L);
            }
        }

        /// <summary>
        /// Export database to backup location
        /// </summary>
        public async Task<string?> BackupDatabaseAsync()
        {
            try
            {
                var backupPath = Path.Combine(FileSystem.CacheDirectory, $"trolltrack_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                File.Copy(_databasePath, backupPath, true);
                return backupPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database backup error: {ex.Message}");
                return null;
            }
        }

        #endregion

        public void Dispose()
        {
            _database?.CloseAsync().GetAwaiter().GetResult();
        }
    }

    #region Database Entities

    /// <summary>
    /// SQLite entity for CatchData
    /// </summary>
    [Table("CatchData")]
    public class CatchDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [Indexed]
        public DateTime Timestamp { get; set; }

        [ForeignKey(typeof(LocationEntity))]
        public Guid? LocationId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public LocationEntity Location { get; set; }

        [ForeignKey(typeof(ProgramDataEntity))]
        public Guid? ProgramDataId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public ProgramDataEntity ProgramData { get; set; }

        [ForeignKey(typeof(FishInfoEntity))]
        public Guid? FishInfoId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public FishInfoEntity FishInfo { get; set; }
    }

    /// <summary>
    /// SQLite entity for Location
    /// </summary>
    [Table("Locations")]
    public class LocationEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Course { get; set; }
        public double? Speed { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity> Catches { get; set; }
    }

    /// <summary>
    /// SQLite entity for ProgramData
    /// </summary>
    [Table("ProgramData")]
    public class ProgramDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity> Catches { get; set; }
    }

    /// <summary>
    /// SQLite entity for FishInfo
    /// </summary>
    [Table("FishInfo")]
    public class FishInfoEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? CommonName { get; set; }
        public string? ScientificName { get; set; }
        public string? Habitat { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity> Catches { get; set; }
    }

    #endregion

    #region Statistics Model

    public class CatchStatistics
    {
        public int TotalCatches { get; set; }
        public int TodaysCatches { get; set; }
        public int WeekCatches { get; set; }
        public int MonthCatches { get; set; }
        public DateTime LastCatchDate { get; set; }
    }

    #endregion
}