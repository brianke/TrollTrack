using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TrollTrack.Configuration;
using TrollTrack.MVVM.Models;

namespace TrollTrack.Services
{
    /// <summary>
    /// SQLite database service for managing catch data and other app data
    /// </summary>
    public class DatabaseService : IDatabaseService
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

                // Convert your CatchData model to database entity
                var entity = await ConvertToEntityAsync(catchData);

                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                    return await db.InsertAsync(entity);
                }
                else
                {
                    return await db.UpdateAsync(entity);
                }
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
                var entities = await db.Table<CatchDataEntity>()
                    .OrderByDescending(c => c.Timestamp)
                    .ToListAsync();

                var catchDataList = new List<CatchData>();
                foreach (var entity in entities)
                {
                    var catchData = await ConvertFromEntityAsync(entity);
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
                var entities = await db.Table<CatchDataEntity>()
                    .Where(c => c.Timestamp >= startDate && c.Timestamp <= endDate)
                    .OrderByDescending(c => c.Timestamp)
                    .ToListAsync();

                var catchDataList = new List<CatchData>();
                foreach (var entity in entities)
                {
                    var catchData = await ConvertFromEntityAsync(entity);
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
                var entity = await db.Table<CatchDataEntity>()
                    .Where(c => c.Id == id)
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                return await ConvertFromEntityAsync(entity);
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
                return await db.Table<CatchDataEntity>()
                    .Where(c => c.Id == id)
                    .DeleteAsync();
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
                var catches = await db.Table<CatchDataEntity>().ToListAsync();

                var stats = new CatchStatistics
                {
                    TotalCatches = catches.Count,
                    TodaysCatches = catches.Count(c => c.Timestamp.Date == DateTime.Today),
                    WeekCatches = catches.Count(c => c.Timestamp >= DateTime.Today.AddDays(-7)),
                    MonthCatches = catches.Count(c => c.Timestamp >= DateTime.Today.AddDays(-30)),
                    LastCatchDate = catches.Any() ? catches.Max(c => c.Timestamp) : DateTime.MinValue
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

        private async Task<CatchDataEntity> ConvertToEntityAsync(CatchData catchData)
        {
            var entity = new CatchDataEntity
            {
                Id = catchData.Id,
                Timestamp = catchData.Timestamp
            };

            // Handle Location
            if (catchData.Location != null)
            {
                var locationEntity = new LocationEntity
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

                var db = await GetDatabaseAsync();
                await db.InsertOrReplaceAsync(locationEntity);
                entity.LocationId = locationEntity.Id;
            }

            // Handle ProgramData
            if (catchData.CatchProgram != null)
            {
                // You'll need to implement ProgramData to entity conversion
                // based on your ProgramData model structure
                entity.ProgramDataId = Guid.NewGuid(); // Placeholder
            }

            // Handle FishInfo
            if (catchData.FishCaught != null)
            {
                // You'll need to implement FishInfo to entity conversion
                // based on your FishInfo model structure
                entity.FishCommonNameId = Guid.NewGuid(); // Placeholder
            }

            return entity;
        }

        private async Task<CatchData> ConvertFromEntityAsync(CatchDataEntity entity)
        {
            var catchData = new CatchData
            {
                Id = entity.Id,
                Timestamp = entity.Timestamp
            };

            var db = await GetDatabaseAsync();

            // Retrieve Location
            if (entity.LocationId.HasValue)
            {
                var locationEntity = await db.Table<LocationEntity>()
                    .Where(l => l.Id == entity.LocationId.Value)
                    .FirstOrDefaultAsync();

                if (locationEntity != null)
                {
                    catchData.Location = new Location
                    {
                        Latitude = locationEntity.Latitude,
                        Longitude = locationEntity.Longitude,
                        Altitude = locationEntity.Altitude,
                        Accuracy = locationEntity.Accuracy,
                        Course = locationEntity.Course,
                        Speed = locationEntity.Speed,
                        Timestamp = locationEntity.Timestamp
                    };
                }
            }

            // Retrieve ProgramData
            if (entity.ProgramDataId.HasValue)
            {
                // You'll need to implement entity to ProgramData conversion
                // catchData.CatchProgram = await GetProgramDataAsync(entity.ProgramDataId.Value);
            }

            // Retrieve FishCommonName
            if (entity.FishCommonNameId.HasValue)
            {
                // You'll need to implement entity to FishCommonName conversion
                // catchData.FishCaught = await GetFishCommonNameAsync(entity.FishCommonNameId.Value);
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
        public async Task<long> GetDatabaseSizeAsync()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    var fileInfo = new FileInfo(_databasePath);
                    return fileInfo.Length;
                }
                return 0;
            }
            catch
            {
                return 0;
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
            _database?.CloseAsync();
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

        // Foreign Keys
        public Guid? LocationId { get; set; }
        public Guid? ProgramDataId { get; set; }
        public Guid? FishCommonNameId { get; set; }
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
    }

    /// <summary>
    /// SQLite entity for ProgramData - You'll need to expand this based on your ProgramData model
    /// </summary>
    [Table("ProgramData")]
    public class ProgramDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        // Add properties based on your ProgramData model
        public string? Name { get; set; }
        public string? Description { get; set; }
        // Add other ProgramData properties here
    }

    /// <summary>
    /// SQLite entity for FishInfo - You'll need to expand this based on your FishInfo model
    /// </summary>
    [Table("FishInfo")]
    public class FishInfoEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        // Add properties based on your FishData model
        public string? CommonName { get; set; }
        public string? ScientificName { get; set; }
        public string? Habitat { get; set; }
        // Add other FishCommonName properties here
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