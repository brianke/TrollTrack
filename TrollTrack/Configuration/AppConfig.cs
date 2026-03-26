namespace TrollTrack.Configuration
{
    /// <summary>
    /// Configuration management for TrollTrack application
    /// Combines constants for stable values with secure storage for sensitive data
    /// </summary>
    public static class AppConfig
    {
        /// <summary>
        /// Application constants that don't change
        /// </summary>
        public static class Constants
        {
            // Weather API configuration
            public const string WeatherApiBaseUrl = "https://api.weatherapi.com/v1";
            public const int WeatherApiTimeoutSeconds = 30;
            public const int WeatherCacheExpirationMinutes = 15;

            // Location and GPS configuration
            public const double DefaultLatitude = 41.2033; // Columbus, Ohio (Great Lakes region)
            public const double DefaultLongitude = -81.5188;
            public const int LocationTimeoutSeconds = 10;

            // Speed conversion factors (to knots)
            public const double MetersPerSecondToKnots = 1.94384;
            public const double MilesPerHourToKnots = 0.868976;

            // Fishing data constants (in knots)
            public const double MinTrollingSpeed = 0.25;   // ~0.3 mph
            public const double MaxTrollingSpeed = 10.5; // ~12 mph
            public const double OptimalTrollingSpeed = 2.2; // ~2.5 mph

            // Depth ranges (feet)
            public const int MinDepth = 1;
            public const int MaxDepth = 200;

            // Temperature ranges for good fishing (Fahrenheit)
            public const double MinGoodFishingTemp = 45;
            public const double MaxGoodFishingTemp = 85;

            // Database and storage configuration
            public const string DatabaseName = "trolltrack.db";
            public const int DatabaseVersion = 1;

            /// <summary>
            /// Bump when bundled lures.json / divers.json defaults change so seeding runs again.
            /// Does not affect trips/catches; avoids re-importing JSON on every app launch.
            /// </summary>
            public const int CatalogSeedVersion = 1;

            // UI and display constants
            public const int RefreshIntervalMinutes = 5;
            public const int MaxRecentCatches = 10;
            public const string DateFormat = "MM/dd/yyyy";
            public const string TimeFormat = "hh:mm tt";
            public const string DateTimeFormat = "MM/dd/yyyy hh:mm tt";
        }
    }
}