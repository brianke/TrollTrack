using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.Generic;
using System.Text.Json;
using TrollTrack.Configuration;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    /// <summary>
    /// SQLite database service for managing catch data and other app data
    /// </summary>
    public class DatabaseService : IDatabaseService, IAsyncDisposable
    {
        private SQLiteAsyncConnection? _database;
        private readonly string _databasePath;
        private Task? _initializationTask;  // Store the Task itself
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
                if (_database != null)  // Check if already done
                    return;

                _database = new SQLiteAsyncConnection(_databasePath);
                await _database.ExecuteAsync("PRAGMA foreign_keys = ON;");

                // Create tables for your existing models
                await _database.CreateTableAsync<CatchDataEntity>();
                await _database.CreateTableAsync<LocationDataEntity>();
                await _database.CreateTableAsync<FishInfoEntity>();
                await _database.CreateTableAsync<DiverDataEntity>();
                await _database.CreateTableAsync<LureDataEntity>();
                await _database.CreateTableAsync<LureImageEntity>();
                await _database.CreateTableAsync<TripDataEntity>();
                await _database.CreateTableAsync<RodSetupEntity>();
                await _database.CreateTableAsync<WeatherDataEntity>();
                await _database.CreateTableAsync<CustomClarityEntity>();

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
            // If we have a task, await it (whether in progress or completed)
            if (_initializationTask == null)
            {
                // Use Interlocked to ensure only ONE thread creates the task
                var newTask = InitializeAsync();
                if (Interlocked.CompareExchange(ref _initializationTask, newTask, null) != null)
                {
                    // Another thread beat us to it, use their task instead
                    // Our newTask will be garbage collected
                }
            }

            await _initializationTask;
            return _database!;
        }

        #region Trip Data Operations

        /// <summary>
        /// Save or update a trip
        /// </summary>
        public async Task<int> SaveTripAsync(TripDataEntity trip)
        {
            try
            {
                var db = await GetDatabaseAsync();

                // Save weather entity first if it exists
                if (trip.WeatherEntity != null)
                {
                    await db.InsertOrReplaceAsync(trip.WeatherEntity);
                    trip.WeatherEntityId = trip.WeatherEntity.Id;
                }

                // Save the trip
                await db.InsertOrReplaceAsync(trip);

                // Save catches with TripId set
                if (trip.Catches != null && trip.Catches.Any())
                {
                    foreach (var catchEntity in trip.Catches)
                    {
                        catchEntity.TripId = trip.Id; // Ensure TripId is set
                        await db.InsertOrReplaceWithChildrenAsync(catchEntity, recursive: true);
                    }
                }

                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving trip: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing trip record
        /// </summary>
        public async Task<int> UpdateTripAsync(TripDataEntity tripData)
        {
            try
            {
                var db = await GetDatabaseAsync();

                // Update the trip and its children (catches)
                await db.UpdateWithChildrenAsync(tripData);

                Debug.WriteLine($"Updated trip: {tripData.TripName}, IsActive: {tripData.IsActive}");
                return 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating trip: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get a specific trip by ID
        /// </summary>
        public async Task<TripDataEntity?> GetTripByIdAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var trip = await db.FindAsync<TripDataEntity>(id);

                if (trip == null)
                    return null;

                // Load weather entity separately if needed
                if (trip.WeatherEntityId != null)
                {
                    try
                    {
                        trip.WeatherEntity = await db.GetAsync<WeatherDataEntity>(trip.WeatherEntityId.Value);
                    }
                    catch
                    {
                        // Weather entity not found, continue
                    }
                }

                // Load catches for this trip
                trip.Catches = await db.Table<CatchDataEntity>()
                    .Where(c => c.TripId == id)
                    .ToListAsync();

                // Load children for each catch
                foreach (var catchEntity in trip.Catches)
                {
                    await db.GetChildrenAsync(catchEntity, recursive: true);
                }

                return trip;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting trip by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get the currently active trip
        /// </summary>
        public async Task<TripDataEntity?> GetActiveTripAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var activeTrip = await db.Table<TripDataEntity>()
                    .Where(t => t.IsActive)
                    .FirstOrDefaultAsync();

                if (activeTrip != null)
                {
                    // Load weather entity
                    if (activeTrip.WeatherEntityId != null)
                    {
                        try
                        {
                            activeTrip.WeatherEntity = await db.GetAsync<WeatherDataEntity>(
                                activeTrip.WeatherEntityId.Value);
                        }
                        catch
                        {
                            // Weather entity not found, continue
                        }
                    }

                    // Load catches for this trip
                    activeTrip.Catches = await db.Table<CatchDataEntity>()
                        .Where(c => c.TripId == activeTrip.Id)
                        .ToListAsync();

                    // Load children for each catch
                    foreach (var catchEntity in activeTrip.Catches)
                    {
                        await db.GetChildrenAsync(catchEntity, recursive: true);
                    }
                }

                return activeTrip;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting active trip: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get all trips
        /// </summary>
        public async Task<List<TripDataEntity>> GetAllTripsAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var trips = await db.Table<TripDataEntity>()
                    .OrderByDescending(t => t.TripDate)
                    .ToListAsync();

                // Load weather entities and catches for each trip
                foreach (var trip in trips)
                {
                    if (trip.WeatherEntityId != null)
                    {
                        try
                        {
                            trip.WeatherEntity = await db.GetAsync<WeatherDataEntity>(
                                trip.WeatherEntityId.Value);
                        }
                        catch
                        {
                            // Weather entity not found, continue
                        }
                    }

                    // Load catches
                    trip.Catches = await db.Table<CatchDataEntity>()
                        .Where(c => c.TripId == trip.Id)
                        .ToListAsync();

                    // Load children for each catch
                    foreach (var catchEntity in trip.Catches)
                    {
                        await db.GetChildrenAsync(catchEntity, recursive: true);
                    }
                }

                return trips;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all trips: {ex.Message}");
                return new List<TripDataEntity>();
            }
        }

        /// <summary>
        /// Get recent trips
        /// </summary>
        public async Task<List<TripDataEntity>> GetRecentTripsAsync(int count = 10)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var trips = await db.Table<TripDataEntity>()
                    .OrderByDescending(t => t.TripDate)
                    .Take(count)
                    .ToListAsync();

                // Load weather and catches for each trip
                foreach (var trip in trips)
                {
                    if (trip.WeatherEntityId != null)
                    {
                        try
                        {
                            trip.WeatherEntity = await db.GetAsync<WeatherDataEntity>(
                                trip.WeatherEntityId.Value);
                        }
                        catch
                        {
                            // Weather entity not found, continue
                        }
                    }

                    // Load catches
                    trip.Catches = await db.Table<CatchDataEntity>()
                        .Where(c => c.TripId == trip.Id)
                        .ToListAsync();

                    // Load children for each catch
                    foreach (var catchEntity in trip.Catches)
                    {
                        await db.GetChildrenAsync(catchEntity, recursive: true);
                    }
                }

                return trips;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting recent trips: {ex.Message}");
                return new List<TripDataEntity>();
            }
        }

        /// <summary>
        /// Get trips within a date range
        /// </summary>
        public async Task<List<TripDataEntity>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var trips = await db.Table<TripDataEntity>()
                    .Where(t => t.TripDate >= startDate && t.TripDate <= endDate)
                    .ToListAsync();

                // Load weather and catches for each trip
                foreach (var trip in trips)
                {
                    if (trip.WeatherEntityId != null)
                    {
                        try
                        {
                            trip.WeatherEntity = await db.GetAsync<WeatherDataEntity>(
                                trip.WeatherEntityId.Value);
                        }
                        catch
                        {
                            // Weather entity not found, continue
                        }
                    }

                    // Load catches
                    trip.Catches = await db.Table<CatchDataEntity>()
                        .Where(c => c.TripId == trip.Id)
                        .ToListAsync();

                    // Load children for each catch
                    foreach (var catchEntity in trip.Catches)
                    {
                        await db.GetChildrenAsync(catchEntity, recursive: true);
                    }
                }

                return trips.OrderByDescending(t => t.TripDate).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting trips by date range: {ex.Message}");
                return new List<TripDataEntity>();
            }
        }

        /// <summary>
        /// Get all catches for a specific trip
        /// </summary>
        public async Task<List<CatchDataEntity>> GetCatchesForTripAsync(Guid tripId)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var catches = await db.Table<CatchDataEntity>()
                    .Where(c => c.TripId == tripId)
                    .OrderByDescending(c => c.Timestamp)
                    .ToListAsync();

                // Load children for each catch
                foreach (var catchEntity in catches)
                {
                    await db.GetChildrenAsync(catchEntity, recursive: true);
                }

                return catches;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting catches for trip: {ex.Message}");
                return new List<CatchDataEntity>();
            }
        }

        /// <summary>
        /// Delete a trip and all associated catches
        /// </summary>
        public async Task<int> DeleteTripAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();

                // Delete all catches associated with this trip
                var catches = await db.Table<CatchDataEntity>()
                    .Where(c => c.TripId == id)
                    .ToListAsync();

                foreach (var catchEntity in catches)
                {
                    await db.DeleteAsync(catchEntity, recursive: true);
                }

                // Delete the trip
                var trip = await db.FindAsync<TripDataEntity>(id);
                if (trip != null)
                {
                    await db.DeleteAsync(trip);
                    return 1;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting trip: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Location Methods

        /// <summary>
        /// Gets a location by its ID
        /// This is used by CatchDataEntity async methods to fetch location data
        /// </summary>
        public async Task<LocationDataEntity?> GetLocationByIdAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var location = await db.GetAsync<LocationDataEntity>(id);
                return location;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Saves a location to the database
        /// </summary>
        public async Task<int> SaveLocationAsync(LocationDataEntity location)
        {
            try
            {
                var db = await GetDatabaseAsync();

                if (location.Id == Guid.Empty)
                {
                    location.Id = Guid.NewGuid();
                }

                await db.InsertOrReplaceAsync(location);
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving location: {ex.Message}");
                throw;
            }
        }

        #endregion
        
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

    #region Rod Setup Operations

        /// <summary>
        /// Get all rod setups
        /// </summary>
        public async Task<List<RodSetupEntity>> GetAllRodSetupsAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var setups = await db.Table<RodSetupEntity>()
                    //.OrderByDescending(r => r.IsFavorite)
                    //.ThenByDescending(r => r.LastUsed)
                    .ToListAsync();

                // Load lure information for each setup
                foreach (var setup in setups)
                {
                    if (setup.LureId.HasValue)
                    {
                        setup.Lure = await GetLureByIdAsync(setup.LureId.Value);
                    }
                    if (setup.DiverId.HasValue)
                    {
                        setup.Diver = await GetDiverByIdAsync(setup.DiverId.Value);
                    }
                }

                return setups;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all rod setups: {ex.Message}");
                return new List<RodSetupEntity>();
            }
        }

        /// <summary>
        /// Get a specific rod setup by ID
        /// </summary>
        public async Task<RodSetupEntity?> GetRodSetupByIdAsync(int setupId)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var setup = await db.Table<RodSetupEntity>()
                    .Where(r => r.Id == setupId)
                    .FirstOrDefaultAsync();

                if (setup != null)
                {
                    // Load lure information
                    if (setup.LureId.HasValue)
                    {
                        setup.Lure = await GetLureByIdAsync(setup.LureId.Value);
                    }
                    if (setup.DiverId.HasValue)
                    {
                        setup.Diver = await GetDiverByIdAsync(setup.DiverId.Value);
                    }
                }

                return setup;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting rod setup by ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save or update a rod setup
        /// </summary>
        public async Task<int> SaveRodSetupAsync(RodSetupEntity setup)
        {
            try
            {
                var db = await GetDatabaseAsync();

                if (setup.Id == 0 || setup.Id == default)
                {
                    // This is a new record - INSERT
                    //setup.CreatedAt = DateTime.Now;
                    //setup.LastUsed = DateTime.Now;
                    //setup.TimesUsed = 0;
                    //setup.CatchCount = 0;

                    await db.InsertAsync(setup);
                    // setup.Id now contains the auto-generated ID
                }
                else
                {
                    // This is an existing record - UPDATE
                    var existing = await db.GetAsync<RodSetupEntity>(setup.Id);
                    if (existing != null)
                    {
                        await db.UpdateAsync(setup);
                    }
                    else
                    {
                        // Weird case: Id is set but doesn't exist
                        setup.Id = 0; // Reset to trigger INSERT
                        await db.InsertAsync(setup);
                    }
                }

                return setup.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving rod setup: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update an existing rod setup
        /// </summary>
        public async Task<int> UpdateRodSetupAsync(RodSetupEntity setup)
        {
            try
            {
                if (setup.Id <= 0)
                {
                    throw new ArgumentException("Invalid setup ID");
                }

                if (string.IsNullOrWhiteSpace(setup.Name))
                {
                    throw new ArgumentException("Rod setup name is required");
                }

                var db = await GetDatabaseAsync();
                await db.UpdateAsync(setup);
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating rod setup: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete a rod setup by ID
        /// </summary>
        public async Task<int> DeleteRodSetupAsync(int setupId)
        {
            try
            {
                if (setupId <= 0)
                {
                    throw new ArgumentException("Invalid setup ID");
                }

                var db = await GetDatabaseAsync();
                await db.DeleteAsync<RodSetupEntity>(setupId);
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting rod setup: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get recently used rod setups
        /// </summary>
        //public async Task<List<RodSetupEntity>> GetRecentlyUsedRodSetupsAsync(int count = 10)
        //{
        //    try
        //    {
        //        var db = await GetDatabaseAsync();
        //        var setups = await db.Table<RodSetupEntity>()
        //            .OrderByDescending(r => r.LastUsed)
        //            .Take(count)
        //            .ToListAsync();

        //        // Load lure information
        //        foreach (var setup in setups)
        //        {
        //            if (setup.LureId.HasValue)
        //            {
        //                setup.Lure = await GetLureByIdAsync(setup.LureId.Value);
        //            }
        //            if (setup.DiverId.HasValue)
        //            {
        //                setup.Diver = await GetDiverByIdAsync(setup.DiverId.Value);
        //            }
        //        }

        //        return setups;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error getting recently used rod setups: {ex.Message}");
        //        return new List<RodSetupEntity>();
        //    }
        //}

        /// <summary>
        /// Get most frequently used rod setups
        /// </summary>
        //public async Task<List<RodSetupEntity>> GetMostUsedRodSetupsAsync(int count = 10)
        //{
        //    try
        //    {
        //        var db = await GetDatabaseAsync();
        //        var setups = await db.Table<RodSetupEntity>()
        //            .OrderByDescending(r => r.TimesUsed)
        //            .ThenByDescending(r => r.LastUsed)
        //            .Take(count)
        //            .ToListAsync();

        //        // Load lure information
        //        foreach (var setup in setups)
        //        {
        //            if (setup.LureId.HasValue)
        //            {
        //                setup.Lure = await GetLureByIdAsync(setup.LureId.Value);
        //            }
        //            if (setup.DiverId.HasValue)
        //            {
        //                setup.Diver = await GetDiverByIdAsync(setup.DiverId.Value);
        //            }
        //        }

        //        return setups;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error getting most used rod setups: {ex.Message}");
        //        return new List<RodSetupEntity>();
        //    }
        //}

        /// <summary>
        /// Increment usage counter for a rod setup
        /// </summary>
        //public async Task<int> IncrementRodSetupUsageAsync(int setupId)
        //{
        //    try
        //    {
        //        var setup = await GetRodSetupByIdAsync(setupId);
        //        if (setup != null)
        //        {
        //            setup.TimesUsed++;
        //            setup.LastUsed = DateTime.Now;
        //            await UpdateRodSetupAsync(setup);
        //            return 1;
        //        }
        //        return 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error incrementing rod setup usage: {ex.Message}");
        //        throw;
        //    }
        //}

        /// <summary>
        /// Save multiple rod setups in a transaction
        /// </summary>
        public async Task<List<RodSetupEntity>> SaveMultipleRodSetupsAsync(List<RodSetupEntity> setups)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var savedSetups = new List<RodSetupEntity>();

                await db.RunInTransactionAsync(tran =>
                {
                    foreach (var setup in setups)
                    {
                        if (setup.Id > 0)
                        {
                            tran.Update(setup);
                        }
                        else
                        {
                            //setup.CreatedAt = DateTime.Now;
                            //setup.LastUsed = DateTime.Now;
                            //setup.TimesUsed = 0;
                            //setup.CatchCount = 0;
                            tran.Insert(setup);
                        }
                        savedSetups.Add(setup);
                    }
                });

                return savedSetups;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving multiple rod setups: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Delete multiple rod setups in a transaction
        /// </summary>
        public async Task<int> DeleteMultipleRodSetupsAsync(List<int> setupIds)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var count = 0;

                await db.RunInTransactionAsync(tran =>
                {
                    foreach (var id in setupIds)
                    {
                        tran.Delete<RodSetupEntity>(id);
                        count++;
                    }
                });

                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting multiple rod setups: {ex.Message}");
                throw;
            }
        }

        #endregion




        #region Custom Clarity Operations

        public async Task<int> SaveCustomClarityAsync(string clarity)
        {
            try
            {
                var db = await GetDatabaseAsync();

                // Check if already exists
                var existing = await db.Table<CustomClarityEntity>()
                    .Where(c => c.Description == clarity)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    Debug.WriteLine($"Custom clarity '{clarity}' already exists");
                    return 0;
                }

                var entity = new CustomClarityEntity
                {
                    Id = Guid.NewGuid(),
                    Description = clarity,
                    CreatedAt = DateTime.Now
                };

                await db.InsertAsync(entity);
                Debug.WriteLine($"Saved custom clarity: {clarity}");
                return 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving custom clarity: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetCustomClaritiesAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.Table<CustomClarityEntity>()
                    .OrderBy(c => c.Description)
                    .ToListAsync();

                return entities.Select(e => e.Description).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting custom clarities: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<int> DeleteCustomClarityAsync(string clarity)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = await db.Table<CustomClarityEntity>()
                    .Where(c => c.Description == clarity)
                    .FirstOrDefaultAsync();

                if (entity != null)
                {
                    return await db.DeleteAsync(entity);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting custom clarity: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Lure Methods

        public async Task<LureDataEntity?> GetLureByIdAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entity = await db.GetWithChildrenAsync<LureDataEntity>(id, recursive: true);
                return entity;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting lure by ID: {ex.Message}");
                return null;
            }
        }

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
                var lures = entities.Select(ConvertFromLureEntity).ToList();

                using var stream = await FileSystem.OpenAppPackageFileAsync("lures.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var lureList = JsonSerializer.Deserialize<List<LureDataEntity>>(json);

                if (lureList != null)
                {
                    foreach (var lure in lureList)
                    {
                        lures.Add(lure);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Loaded {lureList.Count} lures");
                return lureList;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all lures: {ex.Message}");
                return new List<LureDataEntity>();
            }
        }

        #endregion

        #region Diver Methods

        public async Task<int> SaveDiverAsync(DiverDataEntity diver)
        {
            try
            {
                var db = await GetDatabaseAsync();
                if (diver.Id == Guid.Empty)
                {
                    diver.Id = Guid.NewGuid();
                }
                await db.InsertOrReplaceAsync(diver);
                return 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving diver: {ex.Message}");
                throw;
            }
        }

        public async Task<List<DiverDataEntity>> GetAllDiversAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entities = await db.Table<DiverDataEntity>().ToListAsync();
                return entities.OrderBy(d => d.Name).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all divers: {ex.Message}");
                return new List<DiverDataEntity>();
            }
        }

        public async Task<DiverDataEntity?> GetDiverByIdAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                return await db.GetAsync<DiverDataEntity>(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting diver by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<int> DeleteDiverAsync(Guid id)
        {
            try
            {
                var db = await GetDatabaseAsync();
                var entityToDelete = await db.GetAsync<DiverDataEntity>(id);
                if (entityToDelete != null)
                {
                    await db.DeleteAsync(entityToDelete);
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting diver: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private CatchDataEntity ConvertToCatchEntity(CatchDataEntity catchData)
        {
            var entity = catchData;

            //if (catchData.ProgramData != null)
            //{
            //    // TODO: The ProgramData model is incomplete.
            //    entity.ProgramData = new ProgramDataEntity
            //    {
            //        Id = Guid.NewGuid(),
            //        Name = "Placeholder Program",
            //        Description = "Placeholder Description"
            //    };
            //    entity.ProgramDataId = entity.ProgramData.Id;
            //}

            entity.LocationId = catchData.LocationId;
            entity.FishInfoId = catchData.FishInfoId;
            entity.LureId = catchData.LureId;
            entity.LineOut = catchData.LineOut;
            entity.DiverDataId = catchData.DiverDataId;
            entity.Latitude = catchData.Latitude;
            entity.Longitude = catchData.Longitude;

            return entity;
        }

        /// <summary>
        /// Updated ConvertFromCatchEntity - now uses RodSetup instead of ProgramData
        /// </summary>
        private CatchDataEntity ConvertFromCatchEntity(CatchDataEntity entity)
        {
            var catchData = new CatchDataEntity
            {
                Id = entity.Id,
                Timestamp = entity.Timestamp
            };


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
                    entity.Images.Add(new LureImageEntity { Id = Guid.NewGuid(), Path = image.Path });
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

        public async Task ClearAllTablesAsync()
        {
            try
            {
                var db = await GetDatabaseAsync();

                await db.DeleteAllAsync<CatchDataEntity>();
                await db.DeleteAllAsync<CustomClarityEntity>();
                await db.DeleteAllAsync<LocationDataEntity>();      
                await db.DeleteAllAsync<FishInfoEntity>();          
                await db.DeleteAllAsync<DiverDataEntity>();         
                await db.DeleteAllAsync<LureDataEntity>();          
                await db.DeleteAllAsync<LureImageEntity>();         
                await db.DeleteAllAsync<TripDataEntity>();
                await db.DeleteAllAsync<RodSetupEntity>();
                await db.DeleteAllAsync<WeatherDataEntity>();       
                                                                    
                System.Diagnostics.Debug.WriteLine("All database tables cleared successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing database: {ex.Message}");
                throw;
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