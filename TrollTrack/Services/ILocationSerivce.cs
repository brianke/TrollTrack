using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public interface ILocationService
    {
        Task<Location> GetCurrentLocationAsync();

        Task<bool> RequestLocationPermissionAsync();
        
        Task SaveLocationAsync(Location location);
        
        //Task<List<Location>> GetLocationHistoryAsync();
        
        event EventHandler<Location> LocationUpdated;
        
        bool IsLocationEnabled { get; }
        
        //Task StartLocationTracking();
        
        //Task StopLocationTracking();
    }
}
