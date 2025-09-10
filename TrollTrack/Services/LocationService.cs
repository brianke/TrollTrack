using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public class LocationService : ILocationService
    {
        private readonly List<Location> _locationHistory = new();

        public bool IsLocationEnabled { get; private set; }
        public event EventHandler<Location> LocationUpdated;


        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                // Check if location is enabled first
                if (!await RequestLocationPermissionAsync())
                {
                    return null;
                }

                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var location = await Geolocation.GetLocationAsync(request);

                //TODO: remove before release, just for testing
#pragma warning disable CS8602
                var (town, coords) = LocationData.GetRandomLocation();
                location.Latitude = coords.Latitude;
                location.Longitude = coords.Longitude;
#pragma warning restore CS8602    
                // TODO: remove to here
                
                if (location != null)
                {
                    IsLocationEnabled = true;
                    //await SaveLocationAsync(location);
                    LocationUpdated?.Invoke(this, location);
                }



                return location;
            }
            catch (FeatureNotSupportedException)
            {
                throw new InvalidOperationException("Geolocation is not supported on this device");
            }
            catch (FeatureNotEnabledException)
            {
                throw new InvalidOperationException("Geolocation is not enabled on this device");
            }
            catch (PermissionException)
            {
                throw new UnauthorizedAccessException("Location permission was denied");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
                throw new InvalidOperationException($"Unable to get location: {ex.Message}");
            }
        }

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
                return false;
            }
        }

        public async Task SaveLocationAsync(Location location)
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

            await Task.CompletedTask;
        }

    }

    public enum LocationReference
    {
        CatawbaOH = 1,
        LudingtonMI = 2,
        TraverseCityMI = 3,
        ConneautOH = 4,
        FortMyersFL = 5,
        NagsHeadNC = 6,
        BainbridgeMD = 7
    }

    public struct LocationCoordinates
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public LocationCoordinates(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }

    public static class LocationData
    {
        public static readonly Dictionary<LocationReference, LocationCoordinates> Locations = new()
        {
            { LocationReference.CatawbaOH, new LocationCoordinates(39.9981, -83.6205) },
            { LocationReference.LudingtonMI, new LocationCoordinates(43.9550, -86.4526) },
            { LocationReference.TraverseCityMI, new LocationCoordinates(44.7631, -85.6206) },
            { LocationReference.ConneautOH, new LocationCoordinates(41.9478, -80.5545) },
            { LocationReference.FortMyersFL, new LocationCoordinates(26.6406, -81.8723) },
            { LocationReference.NagsHeadNC, new LocationCoordinates(35.9579, -75.6241) },
            { LocationReference.BainbridgeMD, new LocationCoordinates(39.6101, -76.1336) }
        };


        private static readonly Random _random = new();

        public static (LocationReference location, LocationCoordinates coords) GetRandomLocation()
        {
            var values = Enum.GetValues(typeof(LocationReference));
            var randomLocation = (LocationReference)values.GetValue(_random.Next(values.Length))!;
            return (randomLocation, Locations[randomLocation]);
        }

    }
}
