using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public class LocationService
    {
        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var location = await Geolocation.GetLocationAsync(request);

                //TODO: remove before release, just for testing
                location.Latitude = Random.Shared.NextDouble() * 90;
                location.Longitude = Random.Shared.NextDouble() * 180;

                return location;
            }
            catch (Exception ex)
            {
                // Handle location errors
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RequestLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
    }
}
