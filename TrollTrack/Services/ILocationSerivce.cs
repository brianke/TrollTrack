using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public interface ILocationService
    {
        Task<LocationDataEntity> GetCurrentLocationAsync();
        /// <summary>
        /// Requests a fresh GPS fix. Use when recording a catch so each catch gets its own exact location.
        /// Does not reuse last-known location.
        /// </summary>
        Task<LocationDataEntity> GetExactLocationAsync();
        /// <summary>
        /// Returns the last location retrieved (from Dashboard/refresh/catch) without requesting GPS.
        /// Returns null if no location has been retrieved yet.
        /// </summary>
        Task<LocationDataEntity?> GetLastKnownLocationAsync();
        Task<bool> RequestLocationPermissionAsync();
        Task<List<LocationDataEntity>> GetLocationHistoryAsync();
        Task SaveLocationAsync(LocationDataEntity location);
        event EventHandler<LocationDataEntity> LocationUpdated;
        bool IsLocationEnabled { get; }
    }
}
