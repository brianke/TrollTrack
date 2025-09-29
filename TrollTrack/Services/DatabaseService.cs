using SQLiteNetExtensionsAsync.Extensions;
using TrollTrack.Configuration;
using TrollTrack.Features.Catches;
using TrollTrack.Features.Shared.Models;
using TrollTrack.Features.Shared.Models.Entities;
using TrollTrack.Models.Entities;

namespace TrollTrack.Services
{
    /// <summary>
    /// SQLite database service for managing catch data and other app data
    /// </summary>
    public class DatabaseService : IDatabaseService, IAsyncDisposable
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _databasePath;
        private bool _isInitialized;
        private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        public DatabaseService()
        {
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, AppConfig.Constants.DatabaseName);
        }

        /// <summary>
        /// Initialize the database connection and create tables
        /// </summary>
        private async Task InitializeAsync()
        {
            await _initializationSemaphore.WaitAsync();
            try
            {
                if (_isInitialized)
                    return;

                _database = new SQLiteAsyncConnection(_databasePath);
                await _database.ExecuteAsync("PRAGMA foreign_keys = ON;");

                // Create tables for your existing models
                await _database.CreateTableAsync<CatchDataEntity>();
                await _database.CreateTableAsync<LocationDataEntity>();
                await _database.CreateTableAsync<ProgramDataEntity>();
                await _database.CreateTableAsync<FishInfoEntity>();
                await _database.CreateTableAsync<LureDataEntity>();
                await _database.CreateTableAsync<LureImageEntity>();
                await _database.CreateTableAsync<LureImageEntity>();

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine($"Database initialized at: {_databasePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
            return _database!;
        }

        #region Catch Data Operations

        /// <summary>
        /// Save a new catch record
        /// </summary>
        public async Task<int> SaveCatchAsync(CatchDataEntity catchData)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = ConvertToCatchEntity(catchData);

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
        public async Task<List<CatchDataEntity>> GetCatchDataAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.GetAllWithChildrenAsync<CatchDataEntity>(recursive: true);

                return entities.OrderByDescending(e => e.Timestamp)
                               .Select(ConvertFromCatchEntity)
                               .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting catch data: {ex.Message}");
                return new List<CatchDataEntity>();
            }
        }

        /// <summary>
        /// Get catch records for a specific date range
        /// </summary>
        public async Task<List<CatchDataEntity>> GetCatchDataByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.GetAllWithChildrenAsync<CatchDataEntity>(c => c.Timestamp >= startDate && c.Timestamp <= endDate, recursive: true);

                return entities.OrderByDescending(e => e.Timestamp)
                               .Select(ConvertFromCatchEntity)
                               .ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting catch data by date range: {ex.Message}");
                return new List<CatchDataEntity>();
            }
        }

        /// <summary>
        /// Get today's catches
        /// </summary>
        public async Task<List<CatchDataEntity>> GetTodaysCatchesAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            return await GetCatchDataByDateRangeAsync(today, tomorrow);
        }

        /// <summary>
        /// Get a specific catch by ID
        /// </summary>
        public async Task<CatchDataEntity?> GetCatchByIdAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = await db.GetWithChildrenAsync<CatchDataEntity>(id, recursive: true);

                if (entity == null)
                    return null;

                return ConvertFromCatchEntity(entity);
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
                var entityToDelete = await db.GetWithChildrenAsync<CatchDataEntity>(id);
                if (entityToDelete != null)
                {
                    await db.DeleteAsync(entityToDelete, recursive: true);
                    return 1;
                }
                return 0;
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

        #region Lure Methods

        public async Task<int> SaveLureAsync(LureDataEntity lureData)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = ConvertToLureEntity(lureData);

                await db.InsertOrReplaceWithChildrenAsync(entity, recursive: true);
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving lure: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LureDataEntity>> GetAllLureDataAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.GetAllWithChildrenAsync<LureDataEntity>(recursive: true);

                return entities.Select(ConvertFromLureEntity).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all lures: {ex.Message}");
                return new List<LureDataEntity>();
            }
        }

        #endregion

        #region Helper Methods

        private CatchDataEntity ConvertToCatchEntity(CatchDataEntity catchData)
        {
            var entity = new CatchDataEntity
            {
                Id = catchData.Id == Guid.Empty ? Guid.NewGuid() : catchData.Id,
                Timestamp = catchData.Timestamp
            };

            if (catchData.Location != null)
            {
                entity.Location = new LocationDataEntity
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

            if (catchData.ProgramData != null)
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

            // TODO: FishInfo should be selected from predefined list, not created new each time
            var fishInfo = FishData.GetInfo(catchData.FishInfo.CommonName);
            entity.FishInfo = new FishInfoEntity
            {
                Id = fishInfo.Id,
                CommonName = fishInfo.CommonName,
                ScientificName = fishInfo.ScientificName,
                Habitat = fishInfo.Habitat
            };
            entity.FishInfoId = entity.FishInfo.Id;
            

            return entity;
        }

        private CatchDataEntity ConvertFromCatchEntity(CatchDataEntity entity)
        {
            var catchData = new CatchDataEntity
            {
                Id = entity.Id,
                Timestamp = entity.Timestamp
            };

            if (entity.Location != null)
            {
                catchData.Location = new LocationDataEntity
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
                catchData.ProgramData = new ProgramDataEntity();
            }

            if (entity.FishInfo != null)
            {
                catchData.FishInfo = entity.FishInfo;
            }

            return catchData;
        }



        private LureDataEntity ConvertToLureEntity(LureDataEntity lureData)
        {
            var entity = new LureDataEntity
            {
                Id = lureData.Id == Guid.Empty ? Guid.NewGuid() : lureData.Id,
                Manufacturer = lureData.Manufacturer,
                Color = lureData.Color,
                Buoyancy = lureData.Buoyancy,
                Weight = lureData.Weight,
                Length = lureData.Length,
                Images = new List<LureImageEntity>()
            };

            if (lureData.Images != null)
            {
                foreach (var image in lureData.Images)
                {
                    entity.Images.Add(new LureImageEntity { Id = Guid.NewGuid(), ImagePath = image.ImagePath });
                }
            }

            return entity;
        }

        private LureDataEntity ConvertFromLureEntity(LureDataEntity entity)
        {
            var lureData = new LureDataEntity
            {
                Id = entity.Id,
                Manufacturer = entity.Manufacturer,
                Color = entity.Color,
                Buoyancy = entity.Buoyancy,
                Weight = entity.Weight,
                Length = entity.Length,
                Images = new List<LureImageEntity>()
            };

            if (entity.Images != null)
            {
                foreach (var imageEntity in entity.Images)
                {
                    lureData.Images.Add(imageEntity);
                }
            }

            return lureData;
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
                await db.DeleteAllAsync<LocationDataEntity>();
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

        public async ValueTask DisposeAsync()
        {
            if (_database != null)
            {
                await _database.CloseAsync();
            }
        }
    }
}