using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public class RodSetupService : IRodSetupService
    {
        private readonly IDatabaseService _databaseService;

        public RodSetupService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<RodSetupEntity>> GetAllSetupsAsync()
        {
            try
            {
                // Call the DatabaseService method to get all rod setups
                return await _databaseService.GetAllRodSetupsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting all setups: {ex.Message}");
                return await Task.FromResult(new List<RodSetupEntity>());
            }
        }

        public async Task<RodSetupEntity?> GetSetupByIdAsync(int setupId)
        {
            try
            {
                // Call the DatabaseService method to get rod setup by ID
                return await _databaseService.GetRodSetupByIdAsync(setupId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting setup by ID: {ex.Message}");
                return await Task.FromResult<RodSetupEntity?>(null);
            }
        }

        public async Task<int> AddSetupAsync(RodSetupEntity setup)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(setup.Name))
                {
                    throw new ArgumentException("Rod setup name is required");
                }

                // Set initial values
                //setup.CreatedAt = DateTime.Now;
                //setup.LastUsed = DateTime.Now;
                //setup.TimesUsed = 0;

                // Save to database
                return await _databaseService.SaveRodSetupAsync(setup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding setup: {ex.Message}");
                return 0;
            }
        }

        public async Task UpdateSetupAsync(RodSetupEntity setup)
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

                //setup.UpdatedAt = DateTime.Now;

                // Update in database
                await _databaseService.UpdateRodSetupAsync(setup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating setup: {ex.Message}");
            }
        }

        public async Task DeleteSetupAsync(int setupId)
        {
            try
            {
                if (setupId <= 0)
                {
                    throw new ArgumentException("Invalid setup ID");
                }

                // Delete from database
                await _databaseService.DeleteRodSetupAsync(setupId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting setup: {ex.Message}");
            }
        }

        //public async Task IncrementUsageAsync(int setupId)
        //{
        //    try
        //    {
        //        // Call the DatabaseService method to increment usage
        //        await _databaseService.IncrementRodSetupUsageAsync(setupId);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error incrementing usage: {ex.Message}");
        //    }
        //}

        //public async Task<List<RodSetupEntity>> GetRecentlyUsedSetupsAsync(int count = 10)
        //{
        //    try
        //    {
        //        // Call the DatabaseService method to get recently used setups
        //        return await _databaseService.GetRecentlyUsedRodSetupsAsync(count);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error getting recently used setups: {ex.Message}");
        //        return await Task.FromResult(new List<RodSetupEntity>());
        //    }
        //}

        //public async Task<List<RodSetupEntity>> GetMostUsedSetupsAsync(int count = 10)
        //{
        //    try
        //    {
        //        // Call the DatabaseService method to get most used setups
        //        return await _databaseService.GetMostUsedRodSetupsAsync(count);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error getting most used setups: {ex.Message}");
        //        return await Task.FromResult(new List<RodSetupEntity>());
        //    }
        //}

        public async Task<List<RodSetupEntity>> SaveMultipleSetupsAsync(List<RodSetupEntity> setups)
        {
            try
            {
                // Call the DatabaseService method to save multiple setups
                return await _databaseService.SaveMultipleRodSetupsAsync(setups);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving multiple setups: {ex.Message}");
                return await Task.FromResult(new List<RodSetupEntity>());
            }
        }

        public async Task DeleteMultipleSetupsAsync(List<int> setupIds)
        {
            try
            {
                // Call the DatabaseService method to delete multiple setups
                await _databaseService.DeleteMultipleRodSetupsAsync(setupIds);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting multiple setups: {ex.Message}");
            }
        }
    }
}