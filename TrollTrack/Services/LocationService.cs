using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public class LocationService : ILocationService
    {
        // Required properties and events from interface

        /// <summary>
        /// Indicates if location is enabled on the device
        /// </summary>
        public bool IsLocationEnabled { get; private set; }

        /// <summary>
        /// Event that is fired whenever the locaiton is updated
        /// This event can be subscribed to by other classes/models when needing to do something when location is updated
        /// </summary>
        public event EventHandler<Location> LocationUpdated;


        // List for tracking location history
        private readonly List<Location> _locationHistory = new();

        /// <summary>
        /// Get the current location asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<Location?> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var location = await Geolocation.GetLocationAsync(request);


                // Set enabled flag and fire event
                if (location != null)
                {
                    IsLocationEnabled = true;
                    SaveLocation(location);
                    LocationUpdated?.Invoke(this, location);
                    return location;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Handle location errors
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
                IsLocationEnabled = false;
                return null;
            }
        }

        /// <summary>
        /// Request location permission asynchronously
        /// This is a request to the device to allow the app access to the device location
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                IsLocationEnabled = status == PermissionStatus.Granted;
                return IsLocationEnabled;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permission error: {ex.Message}");
                IsLocationEnabled = false;
                return false;
            }
        }

        /// <summary>
        /// Retrieve a list of historical locations asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<List<Location>> GetLocationHistoryAsync()
        {
            return await Task.FromResult(_locationHistory.ToList());
        }

        /// <summary>
        /// Save the current location to the list of historical locations
        /// </summary>
        /// <param name="location"></param>
        public void SaveLocation(Location location)
        {
            if (location != null)
            {
                _locationHistory.Add(location);

                // Keep only last 100 locations to prevent memory issues
                if (_locationHistory.Count > 100)
                {
                    _locationHistory.RemoveAt(0);
                }
            }
        }
    }
}