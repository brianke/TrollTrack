using TrollTrack.Features.Shared.Models.Entities;

public interface IRodSetupService
{
    // Basic CRUD
    Task<List<RodSetupEntity>> GetAllSetupsAsync();
    Task<RodSetupEntity?> GetSetupByIdAsync(int setupId);
    Task<int> AddSetupAsync(RodSetupEntity setup);
    Task UpdateSetupAsync(RodSetupEntity setup);
    Task DeleteSetupAsync(int setupId);

    // Usage tracking
    //Task IncrementUsageAsync(int setupId);
    //Task<List<RodSetupEntity>> GetRecentlyUsedSetupsAsync(int count = 10);
    //Task<List<RodSetupEntity>> GetMostUsedSetupsAsync(int count = 10);

    // Bulk operations
    Task<List<RodSetupEntity>> SaveMultipleSetupsAsync(List<RodSetupEntity> setups);
    Task DeleteMultipleSetupsAsync(List<int> setupIds);

}
