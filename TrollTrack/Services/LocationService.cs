using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.Services
{
    public class LocationService : ILocationService
    {
        // set default locaiton which will be used as a return value if actual position cannot be obtained
        private static Location defaultLocation = new Location(0.00, 0.00);

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


                // Set enabled flag and fire event
                if (location != null)
                {
                    IsLocationEnabled = true;
                    await SaveLocationAsync(location);
                    LocationUpdated?.Invoke(this, location);

                    //TODO: remove before release, just for testing
                    var (town, coords) = LocationData.GetRandomLocation();
                    location.Latitude = coords.Latitude;
                    location.Longitude = coords.Longitude;

                    return location;
                }

                return defaultLocation; 
            }
            catch (Exception ex)
            {
                // Handle location errors
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
                IsLocationEnabled = false;
                return defaultLocation;
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
        /// Save the current location to the list of historical locations asynchronously
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
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


    // Following enum, class, methods are for testing only

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