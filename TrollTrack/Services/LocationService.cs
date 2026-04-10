using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrollTrack.Configuration;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public class LocationService : ILocationService
    {
        private readonly IDatabaseService _databaseService;

        private static LocationDataEntity defaultLocation = new LocationDataEntity
        {
            Latitude = 0.00,
            Longitude = 0.00,
            Timestamp = DateTime.Now
        };

        public bool IsLocationEnabled { get; private set; }
        public bool IsListening { get; private set; }

        public event EventHandler<LocationDataEntity>? LocationUpdated;

        private readonly List<LocationDataEntity> _locationHistory = new();
        private LocationDataEntity? _lastKnownLocation;

        private LocationDataEntity? _latestListeningLocation;
        private Guid _listeningTripId;

        private const double MaxAccuracyMetersRoute = 50.0;
        private const double MaxAccuracyMetersCatch = 20.0;

        public LocationService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// Get the current location asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<LocationDataEntity> GetCurrentLocationAsync()
        {
            try
            {
                // Try last known location first for faster response (avoids ANR on slow emulators).
                // Skip if it lacks Course/Speed — those require an active GPS fix.
                var lastKnown = await Geolocation.GetLastKnownLocationAsync();
                if (lastKnown != null
                    && lastKnown.Timestamp > DateTimeOffset.UtcNow.AddMinutes(-5)
                    && lastKnown.Course.HasValue
                    && lastKnown.Speed.HasValue)
                {
                    IsLocationEnabled = true;
                    var locationEntity = CreateLocationEntity(lastKnown);
                    _lastKnownLocation = locationEntity;
                    await SaveLocationAsync(locationEntity);
                    LocationUpdated?.Invoke(this, locationEntity);
                    return locationEntity;
                }

                // High (or better) accuracy required for Speed and Course - Medium returns null for these
                // Use 10s timeout to reduce ANR risk on emulators (was 15s)
                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(10)
                };

                var location = await Geolocation.GetLocationAsync(request);

                if (location != null && location.Accuracy.HasValue && location.Accuracy.Value > MaxAccuracyMetersCatch)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"GetCurrentLocationAsync skipped — accuracy {location.Accuracy.Value:F0}m");
                    location = null;
                }

                if (location != null)
                {
                    IsLocationEnabled = true;
                    var locationEntity = CreateLocationEntity(location);
                    _lastKnownLocation = locationEntity;
                    await SaveLocationAsync(locationEntity);
                    LocationUpdated?.Invoke(this, locationEntity);
                    return locationEntity;
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
        /// Returns a location for recording a catch. Android often omits speed/course on a single
        /// one-shot fix; we poll, use <see cref="GeolocationAccuracy.Best"/>, derive from two fixes
        /// or from the trip track point vs a fresh fix when needed. Each call creates a new
        /// LocationDataEntity with a unique Id.
        /// </summary>
        public async Task<LocationDataEntity> GetExactLocationAsync()
        {
            try
            {
                // Fast path: trip listening already produced velocity + bearing on the latest point
                if (IsListening && _latestListeningLocation != null
                    && _latestListeningLocation.Timestamp > DateTimeOffset.UtcNow.AddSeconds(-30)
                    && HasCompleteVelocity(_latestListeningLocation))
                {
                    var catchLocation = new LocationDataEntity
                    {
                        Id = Guid.NewGuid(),
                        Latitude = _latestListeningLocation.Latitude,
                        Longitude = _latestListeningLocation.Longitude,
                        Timestamp = DateTimeOffset.Now,
                        Course = _latestListeningLocation.Course,
                        Speed = _latestListeningLocation.Speed
                    };
                    return await PersistCatchLocationAsync(catchLocation,
                        $"GetExactLocationAsync tracked: lat={catchLocation.Latitude:F4}, lon={catchLocation.Longitude:F4}, " +
                        $"speed={catchLocation.Speed}, course={catchLocation.Course}");
                }

                // Recent track point for lat/lon (may lack velocity) — pair with fresh fixes to derive
                LocationDataEntity? anchorSnapshot =
                    IsListening && _latestListeningLocation != null
                    && _latestListeningLocation.Timestamp > DateTimeOffset.UtcNow.AddSeconds(-45)
                        ? _latestListeningLocation
                        : null;

                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(12)
                };

                Location? previous = null;
                Location? last = null;
                const int maxAttempts = 6;
                const int delayMs = 450;

                for (var i = 0; i < maxAttempts; i++)
                {
                    var loc = await Geolocation.GetLocationAsync(request);
                    if (loc == null)
                    {
                        await Task.Delay(delayMs);
                        continue;
                    }

                    if (loc.Accuracy.HasValue && loc.Accuracy.Value > MaxAccuracyMetersCatch)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"GetExactLocationAsync skipped fix — accuracy {loc.Accuracy.Value:F0}m");
                        previous ??= loc;
                        await Task.Delay(delayMs);
                        continue;
                    }

                    last = loc;

                    if (HasPlatformVelocity(loc))
                    {
                        var entity = CreateLocationEntity(loc);
                        return await PersistCatchLocationAsync(entity,
                            $"GetExactLocationAsync platform velocity: lat={entity.Latitude:F4}, lon={entity.Longitude:F4}, " +
                            $"speed={entity.Speed}, course={entity.Course}");
                    }

                    if (previous != null
                        && TryDeriveFromTwoLocations(previous, loc, out var sk1, out var c1))
                    {
                        var entity = CreateLocationEntity(loc);
                        if (!entity.Speed.HasValue) entity.Speed = sk1;
                        if (!entity.Course.HasValue) entity.Course = c1;
                        return await PersistCatchLocationAsync(entity,
                            $"GetExactLocationAsync derived (two fixes): lat={entity.Latitude:F4}, lon={entity.Longitude:F4}, " +
                            $"speed={entity.Speed}, course={entity.Course}");
                    }

                    if (anchorSnapshot != null
                        && TryDeriveFromAnchorAndLocation(anchorSnapshot, loc, out var sk2, out var c2))
                    {
                        var entity = CreateLocationEntity(loc);
                        if (!entity.Speed.HasValue) entity.Speed = sk2;
                        if (!entity.Course.HasValue) entity.Course = c2;
                        return await PersistCatchLocationAsync(entity,
                            $"GetExactLocationAsync derived (track+fix): lat={entity.Latitude:F4}, lon={entity.Longitude:F4}, " +
                            $"speed={entity.Speed}, course={entity.Course}");
                    }

                    previous = loc;
                    await Task.Delay(delayMs);
                }

                if (last != null)
                {
                    var entity = CreateLocationEntity(last);
                    if (!HasCompleteVelocity(entity) && previous != null
                        && TryDeriveFromTwoLocations(previous, last, out var sk, out var c))
                    {
                        if (!entity.Speed.HasValue) entity.Speed = sk;
                        if (!entity.Course.HasValue) entity.Course = c;
                    }

                    if (!HasCompleteVelocity(entity) && anchorSnapshot != null
                        && TryDeriveFromAnchorAndLocation(anchorSnapshot, last, out sk, out c))
                    {
                        if (!entity.Speed.HasValue) entity.Speed = sk;
                        if (!entity.Course.HasValue) entity.Course = c;
                    }

                    IsLocationEnabled = true;
                    return await PersistCatchLocationAsync(entity,
                        $"GetExactLocationAsync final: lat={entity.Latitude:F4}, lon={entity.Longitude:F4}, " +
                        $"speed={entity.Speed}, course={entity.Course}");
                }

                var lastKnown = await Geolocation.GetLastKnownLocationAsync();
                if (lastKnown != null && lastKnown.Timestamp > DateTimeOffset.UtcNow.AddSeconds(-30))
                {
                    IsLocationEnabled = true;
                    var locationEntity = CreateLocationEntity(lastKnown);
                    return await PersistCatchLocationAsync(locationEntity,
                        $"GetExactLocationAsync last-known: lat={locationEntity.Latitude:F4}, lon={locationEntity.Longitude:F4}, " +
                        $"speed={locationEntity.Speed}, course={locationEntity.Course}");
                }

                return defaultLocation;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
                IsLocationEnabled = false;
                return defaultLocation;
            }
        }

        private static bool HasCompleteVelocity(LocationDataEntity e) =>
            e.Speed.HasValue && e.Course.HasValue;

        /// <summary>Platform reported speed and course (Android often leaves one or both null on a single fix).</summary>
        private static bool HasPlatformVelocity(Location loc) =>
            loc.Speed.HasValue && loc.Speed.Value >= 0 && loc.Course.HasValue;

        private static bool TryDeriveFromTwoLocations(Location a, Location b, out double speedKnots, out double courseDeg)
        {
            speedKnots = 0;
            courseDeg = 0;
            var dist = HaversineMeters(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
            var dt = GetSecondsBetweenFixes(a, b);
            if (dt <= 0)
                return false;

            var speedMps = dist / dt;
            if (speedMps > 45)
                speedMps = 45;

            speedKnots = speedMps * AppConfig.Constants.MetersPerSecondToKnots;
            courseDeg = BearingDegrees(a.Latitude, a.Longitude, b.Latitude, b.Longitude);

            if (dist < 2.0 && speedMps < 0.25)
                return false;

            return dist >= 1.5 || speedMps > 0.35;
        }

        private static bool TryDeriveFromAnchorAndLocation(LocationDataEntity older, Location newer, out double speedKnots, out double courseDeg)
        {
            speedKnots = 0;
            courseDeg = 0;
            var dist = HaversineMeters(older.Latitude, older.Longitude, newer.Latitude, newer.Longitude);
            var dt = (newer.Timestamp - older.Timestamp).TotalSeconds;
            if (dt <= 0.05)
                dt = 0.5;
            else if (dt > 600)
                dt = 600;

            var speedMps = dist / dt;
            if (speedMps > 45)
                speedMps = 45;

            speedKnots = speedMps * AppConfig.Constants.MetersPerSecondToKnots;
            courseDeg = BearingDegrees(older.Latitude, older.Longitude, newer.Latitude, newer.Longitude);

            if (dist < 2.0 && speedMps < 0.25)
                return false;

            return dist >= 1.5 || speedMps > 0.35;
        }

        private static double GetSecondsBetweenFixes(Location a, Location b)
        {
            var dt = Math.Abs((b.Timestamp - a.Timestamp).TotalSeconds);
            if (dt is >= 0.05 and <= 120)
                return dt;
            return 0.5;
        }

        private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000.0;
            var dLat = (lat2 - lat1) * (Math.PI / 180.0);
            var dLon = (lon2 - lon1) * (Math.PI / 180.0);
            var s1 = Math.Sin(dLat / 2);
            var s2 = Math.Sin(dLon / 2);
            var h = s1 * s1 + Math.Cos(lat1 * (Math.PI / 180.0)) * Math.Cos(lat2 * (Math.PI / 180.0)) * s2 * s2;
            return 2 * R * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
        }

        private static double BearingDegrees(double lat1, double lon1, double lat2, double lon2)
        {
            var lat1R = lat1 * (Math.PI / 180.0);
            var lat2R = lat2 * (Math.PI / 180.0);
            var dLon = (lon2 - lon1) * (Math.PI / 180.0);
            var y = Math.Sin(dLon) * Math.Cos(lat2R);
            var x = Math.Cos(lat1R) * Math.Sin(lat2R) - Math.Sin(lat1R) * Math.Cos(lat2R) * Math.Cos(dLon);
            var brng = Math.Atan2(y, x) * (180.0 / Math.PI);
            return (brng + 360.0) % 360.0;
        }

        private async Task<LocationDataEntity> PersistCatchLocationAsync(LocationDataEntity entity, string logLine)
        {
            IsLocationEnabled = true;
            _lastKnownLocation = entity;
            await SaveLocationAsync(entity);
            LocationUpdated?.Invoke(this, entity);
            System.Diagnostics.Debug.WriteLine(logLine);
            return entity;
        }

        public async Task StartListeningAsync(Guid tripId)
        {
            if (IsListening)
                return;

            _listeningTripId = tripId;

            try
            {
                var request = new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

                Geolocation.LocationChanged += OnLocationChanged;

                var success = await Geolocation.StartListeningForegroundAsync(request);
                IsListening = success;
                System.Diagnostics.Debug.WriteLine($"Foreground location listening started: {success} (tripId={tripId})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartListeningAsync error: {ex.Message}");
                IsListening = false;
            }
        }

        public Task StopListeningAsync()
        {
            if (!IsListening)
                return Task.CompletedTask;

            try
            {
                Geolocation.LocationChanged -= OnLocationChanged;
                Geolocation.StopListeningForeground();
                IsListening = false;
                _latestListeningLocation = null;
                System.Diagnostics.Debug.WriteLine("Foreground location listening stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StopListeningAsync error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private async void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            if (e.Location.Accuracy.HasValue && e.Location.Accuracy.Value > MaxAccuracyMetersRoute)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Listening update skipped — accuracy {e.Location.Accuracy.Value:F0}m > {MaxAccuracyMetersRoute}m threshold");
                return;
            }

            var entity = CreateLocationEntity(e.Location);
            _latestListeningLocation = entity;
            _lastKnownLocation = entity;
            IsLocationEnabled = true;
            LocationUpdated?.Invoke(this, entity);

            try
            {
                var routePoint = new RoutePointEntity
                {
                    TripId = _listeningTripId,
                    Latitude = entity.Latitude,
                    Longitude = entity.Longitude,
                    Speed = entity.Speed,
                    Course = entity.Course,
                    Timestamp = entity.Timestamp
                };
                await _databaseService.SaveRoutePointAsync(routePoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving route point: {ex.Message}");
            }

            System.Diagnostics.Debug.WriteLine(
                $"Listening update: lat={entity.Latitude:F4}, lon={entity.Longitude:F4}, " +
                $"speed={entity.Speed?.ToString("F1") ?? "null"} kt, course={entity.Course?.ToString("F0") ?? "null"}°");
        }

        public Task<LocationDataEntity?> GetLastKnownLocationAsync()
        {
            return Task.FromResult(_lastKnownLocation);
        }

        private static LocationDataEntity CreateLocationEntity(Location location)
        {
            double? speedKnots = null;
            if (location.Speed.HasValue && location.Speed.Value >= 0)
                speedKnots = location.Speed.Value * TrollTrack.Configuration.AppConfig.Constants.MetersPerSecondToKnots;

            return new LocationDataEntity
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Timestamp = DateTimeOffset.Now,
                Course = location.Course,
                Speed = speedKnots
            };
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
        public async Task<List<LocationDataEntity>> GetLocationHistoryAsync()
        {
            return await Task.FromResult(_locationHistory.ToList());
        }

        /// <summary>
        /// Save the current location to the list of historical locations asynchronously
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public async Task SaveLocationAsync(LocationDataEntity location)
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