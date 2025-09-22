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
            public const int MaxForecastDays = 3;
            public const string ImperialTemperatureUnit = "F";
            public const string ImperialSpeedUnit = "mph";
            public const double InHgToHpaConversionFactor = 33.8639;
            
            // Location and GPS configuration
            public const double DefaultLatitude = 41.2033; // Columbus, Ohio (Great Lakes region)
            public const double DefaultLongitude = -81.5188;
            public const int LocationTimeoutSeconds = 10;
            
            // Fishing data constants
            public const double MinTrollingSpeed = 0.5;
            public const double MaxTrollingSpeed = 8.0;
            public const double OptimalTrollingSpeed = 2.5;
            
            // Depth ranges (feet)
            public const int MinDepth = 1;
            public const int MaxDepth = 200;
            
            // Temperature ranges for good fishing (Fahrenheit)
            public const double MinGoodFishingTemp = 45;
            public const double MaxGoodFishingTemp = 85;
            
            // Database and storage configuration
            public const string DatabaseName = "trolltrack.db";
            public const int DatabaseVersion = 1;
            
            // UI and display constants
            public const int RefreshIntervalMinutes = 5;
            public const int MaxRecentCatches = 10;
            public const string DateFormat = "MM/dd/yyyy";
            public const string TimeFormat = "hh:mm tt";
            public const string DateTimeFormat = "MM/dd/yyyy hh:mm tt";
        }
        
        /// <summary>
        /// Runtime configuration that can be set/changed
        /// </summary>
        public static class Runtime
        {
            private static string? _weatherApiKey;
            private static string? _userName;
            private static bool _useMetricUnits = false;
            
            /// <summary>
            /// Weather API key - stored securely
            /// </summary>
            public static string WeatherApiKey 
            { 
                get => _weatherApiKey ?? GetFromSecureStorageSync("WeatherApiKey") ?? "YOUR_API_KEY_HERE";
                set 
                { 
                    _weatherApiKey = value;
                    _ = SaveToSecureStorageAsync("WeatherApiKey", value);
                }
            }
            
            /// <summary>
            /// User's preferred name
            /// </summary>
            public static string UserName
            {
                get => _userName ?? GetFromSecureStorageSync("UserName") ?? "Angler";
                set
                {
                    _userName = value;
                    _ = SaveToSecureStorageAsync("UserName", value);
                }
            }
            
            /// <summary>
            /// Whether to use metric units instead of imperial
            /// </summary>
            public static bool UseMetricUnits
            {
                get => _useMetricUnits;
                set
                {
                    _useMetricUnits = value;
                    _ = SaveToSecureStorageAsync("UseMetricUnits", value.ToString());
                }
            }
            
            private static string? GetFromSecureStorageSync(string key)
            {
                try
                {
                    return SecureStorage.GetAsync(key).Result;
                }
                catch
                {
                    return null;
                }
            }
            
            private static async Task SaveToSecureStorageAsync(string key, string value)
            {
                try
                {
                    await SecureStorage.SetAsync(key, value);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save {key} to secure storage: {ex.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Configuration service for managing app settings
    /// </summary>
    public class ConfigurationService
    {
        /// <summary>
        /// Initialize configuration on app startup
        /// </summary>
        public static async Task InitializeAsync()
        {
            try
            {
                // Load any saved configuration from secure storage
                await LoadConfigurationAsync();
                
                // Validate critical configuration
                ValidateConfiguration();
                
                System.Diagnostics.Debug.WriteLine("Configuration initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize configuration: {ex.Message}");
            }
        }
        
        private static async Task LoadConfigurationAsync()
        {
            try
            {
                // Load API key
                var apiKey = await SecureStorage.GetAsync("WeatherApiKey");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    AppConfig.Runtime.WeatherApiKey = apiKey;
                }
                
                // Load user name
                var userName = await SecureStorage.GetAsync("UserName");
                if (!string.IsNullOrEmpty(userName))
                {
                    AppConfig.Runtime.UserName = userName;
                }
                
                // Load metric units preference
                var useMetric = await SecureStorage.GetAsync("UseMetricUnits");
                if (bool.TryParse(useMetric, out var metric))
                {
                    AppConfig.Runtime.UseMetricUnits = metric;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load configuration: {ex.Message}");
            }
        }
        
        private static void ValidateConfiguration()
        {
            // Check if API key is configured
            if (AppConfig.Runtime.WeatherApiKey == "YOUR_API_KEY_HERE")
            {
                System.Diagnostics.Debug.WriteLine("Warning: Weather API key not configured - weather features will not work");
            }
        }
        
        /// <summary>
        /// Set the weather API key and save it securely
        /// </summary>
        public static async Task SetWeatherApiKeyAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty");
                
            await SecureStorage.SetAsync("WeatherApiKey", apiKey);
            AppConfig.Runtime.WeatherApiKey = apiKey;
            
            System.Diagnostics.Debug.WriteLine("Weather API key updated");
        }
        
        /// <summary>
        /// Set user preferences
        /// </summary>
        public static async Task SetUserPreferencesAsync(string userName, bool useMetric)
        {
            await SecureStorage.SetAsync("UserName", userName);
            await SecureStorage.SetAsync("UseMetricUnits", useMetric.ToString());
            
            AppConfig.Runtime.UserName = userName;
            AppConfig.Runtime.UseMetricUnits = useMetric;
            
            System.Diagnostics.Debug.WriteLine("User preferences updated");
        }
        
        /// <summary>
        /// Check if the weather API key is properly configured
        /// </summary>
        public static bool IsWeatherApiConfigured()
        {
            var apiKey = AppConfig.Runtime.WeatherApiKey;
            return !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_API_KEY_HERE";
        }
        
        /// <summary>
        /// Get API key validation status and message
        /// </summary>
        public static (bool IsValid, string Message) GetWeatherApiKeyStatus()
        {
            var apiKey = AppConfig.Runtime.WeatherApiKey;
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                return (false, "Weather API key is not configured. Please set your WeatherAPI.com key in Settings.");
            }
            
            if (apiKey.Length < 10)
            {
                return (false, "Weather API key appears to be invalid. Please check your key.");
            }
            
            return (true, "Weather API key is configured.");
        }
        
        /// <summary>
        /// Clear all stored configuration (useful for logout or reset)
        /// </summary>
        public static async Task ClearAllConfigurationAsync()
        {
            try
            {
                SecureStorage.Remove("WeatherApiKey");
                SecureStorage.Remove("UserName");
                SecureStorage.Remove("UseMetricUnits");
                
                // Reset runtime values to defaults
                AppConfig.Runtime.WeatherApiKey = "YOUR_API_KEY_HERE";
                AppConfig.Runtime.UserName = "Angler";
                AppConfig.Runtime.UseMetricUnits = false;
                
                System.Diagnostics.Debug.WriteLine("Configuration cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to clear configuration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get all current configuration values (for debugging)
        /// </summary>
        public static Dictionary<string, string> GetCurrentConfiguration()
        {
            return new Dictionary<string, string>
            {
                ["WeatherApiKey"] = AppConfig.Runtime.WeatherApiKey == "YOUR_API_KEY_HERE" ? "Not Set" : "Configured",
                ["UserName"] = AppConfig.Runtime.UserName,
                ["UseMetricUnits"] = AppConfig.Runtime.UseMetricUnits.ToString(),
                ["DatabaseName"] = AppConfig.Constants.DatabaseName,
                ["DefaultLocation"] = $"{AppConfig.Constants.DefaultLatitude}, {AppConfig.Constants.DefaultLongitude}"
            };
        }
    }
}