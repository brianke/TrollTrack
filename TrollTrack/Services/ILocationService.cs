using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public interface ILocationService
    {
        Task<LocationDataEntity> GetCurrentLocationAsync();
        Task<bool> RequestLocationPermissionAsync();
        Task<List<LocationDataEntity>> GetLocationHistoryAsync();
        Task SaveLocationAsync(LocationDataEntity location);
        event EventHandler<LocationDataEntity> LocationUpdated;
        bool IsLocationEnabled { get; }
    }
}
