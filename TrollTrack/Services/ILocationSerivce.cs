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
        /// When foreground listening is active, returns the latest tracked location (which includes speed/course).
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

        /// <summary>
        /// Start continuous foreground GPS listening. Call when a trip begins so that
        /// speed and course are populated on every location update.
        /// Each location fix is saved as a RoutePoint for the given trip.
        /// </summary>
        Task StartListeningAsync(Guid tripId);

        /// <summary>
        /// Stop continuous foreground GPS listening. Call when a trip ends.
        /// </summary>
        Task StopListeningAsync();

        /// <summary>
        /// True when foreground listening is active (between StartListeningAsync/StopListeningAsync).
        /// </summary>
        bool IsListening { get; }

        event EventHandler<LocationDataEntity> LocationUpdated;
        bool IsLocationEnabled { get; }
    }
}
