using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public interface ILocationService
    {
        Task<Location?> GetCurrentLocationAsync();
        Task<bool> RequestLocationPermissionAsync();
        Task<List<Location>> GetLocationHistoryAsync();
        void SaveLocation(Location location);
        event EventHandler<Location> LocationUpdated;
        bool IsLocationEnabled { get; }
    }
}
