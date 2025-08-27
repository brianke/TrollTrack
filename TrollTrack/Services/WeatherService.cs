using System.Text.Json;
using TrollTrack.MVVM.Models;
using TrollTrack.Configuration;

namespace TrollTrack.Services
{
    /// <summary>
    /// Service for fetching weather data from WeatherAPI.com
    /// </summary>
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Set timeout for weather requests
            _httpClient.Timeout = TimeSpan.FromSeconds(AppConfig.Constants.WeatherApiTimeoutSeconds);
        }

        /// <summary>
        /// Gets current weather data for specified coordinates
        /// </summary>
        public async Task<WeatherData?> GetCurrentWeatherAsync(double latitude, double longitude)
        {
            var apiKey = AppConfig.Runtime.WeatherApiKey;

            if (!ConfigurationService.IsWeatherApiConfigured())
            {
                var (_, message) = ConfigurationService.GetWeatherApiKeyStatus();
                throw new InvalidOperationException(message);
            }

            try
            {
                // Current weather and air quality endpoint
                var currentWeatherUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/current.json" +
                    $"?key={apiKey}" +
                    $"&q={latitude},{longitude}" +
                    $"&aqi=yes"; // Include air quality data

                var currentResponse = await _httpClient.GetStringAsync(currentWeatherUrl);
                var currentJson = JsonDocument.Parse(currentResponse);

                var weather = ParseCurrentWeatherData(currentJson, latitude, longitude);

                // Try to get astronomy data (optional)
                try
                {
                    var astronomyUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/astronomy.json" +
                        $"?key={apiKey}" +
                        $"&q={latitude},{longitude}" +
                        $"&dt={DateTime.Now:yyyy-MM-dd}";

                    var astronomyResponse = await _httpClient.GetStringAsync(astronomyUrl);
                    var astronomyJson = JsonDocument.Parse(astronomyResponse);
                    AddAstronomyData(weather, astronomyJson);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Astronomy data not available: {ex.Message}");
                    // Continue without astronomy data
                }

                return weather;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weather API HTTP error: {ex.Message}");

                // Provide more specific error messages based on HTTP status
                if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
                {
                    throw new UnauthorizedAccessException("Invalid weather API key. Please check your API key in Settings.");
                }
                else if (ex.Message.Contains("403") || ex.Message.Contains("Forbidden"))
                {
                    throw new UnauthorizedAccessException("Weather API access denied. Your API key may have exceeded its quota.");
                }
                else if (ex.Message.Contains("429"))
                {
                    throw new InvalidOperationException("Too many weather API requests. Please try again in a few minutes.");
                }

                throw new Exception("Unable to fetch weather data. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weather API JSON error: {ex.Message}");
                throw new Exception("Received invalid weather data from the API.", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weather API general error: {ex.Message}");
                throw new Exception("An unexpected error occurred while fetching weather data.", ex);
            }
        }

        /// <summary>
        /// Gets weather forecast for the next few days
        /// </summary>
        public async Task<List<WeatherData>> GetWeatherForecastAsync(double latitude, double longitude, int days = 3)
        {
            if (!ConfigurationService.IsWeatherApiConfigured())
            {
                throw new InvalidOperationException("Weather API not configured");
            }

            var apiKey = AppConfig.Runtime.WeatherApiKey;

            // WeatherAPI.com free tier supports up to 3 days forecast
            if (days > 3) days = 3;
            if (days < 1) days = 1;

            try
            {
                var forecastUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/forecast.json" +
                    $"?key={apiKey}" +
                    $"&q={latitude},{longitude}" +
                    $"&days={days}" +
                    $"&aqi=no" +  // Skip air quality for forecast to save API calls
                    $"&alerts=no";

                var response = await _httpClient.GetStringAsync(forecastUrl);
                var json = JsonDocument.Parse(response);

                var forecasts = new List<WeatherData>();

                // Check if forecast data exists
                if (json.RootElement.TryGetProperty("forecast", out var forecastElement) &&
                    forecastElement.TryGetProperty("forecastday", out var forecastDays))
                {
                    foreach (var day in forecastDays.EnumerateArray())
                    {
                        var weather = ParseForecastDay(day, latitude, longitude);
                        forecasts.Add(weather);
                    }
                }

                return forecasts;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weather forecast error: {ex.Message}");
                throw new Exception("An error occurred while fetching weather forecast.", ex);
            }
        }

        /// <summary>
        /// Gets weather data by city name
        /// </summary>
        public async Task<WeatherData?> GetWeatherByCityAsync(string cityName)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                throw new ArgumentException("City name cannot be empty");
            }

            if (!ConfigurationService.IsWeatherApiConfigured())
            {
                throw new InvalidOperationException("Weather API not configured");
            }

            try
            {
                var apiKey = AppConfig.Runtime.WeatherApiKey;
                var currentWeatherUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/current.json" +
                    $"?key={apiKey}" +
                    $"&q={Uri.EscapeDataString(cityName)}" +
                    $"&aqi=yes";

                var response = await _httpClient.GetStringAsync(currentWeatherUrl);
                var json = JsonDocument.Parse(response);

                var location = json.RootElement.GetProperty("location");
                var lat = location.GetProperty("lat").GetDouble();
                var lon = location.GetProperty("lon").GetDouble();

                var weather = ParseCurrentWeatherData(json, lat, lon);
                weather.LocationName = location.GetProperty("name").GetString();

                return weather;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("400"))
            {
                throw new ArgumentException($"Location '{cityName}' not found. Please check the spelling.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weather by city error: {ex.Message}");
                throw new Exception($"An error occurred while fetching weather for {cityName}.", ex);
            }
        }

        /// <summary>
        /// Test the API key by making a simple request
        /// </summary>
        public async Task<bool> TestApiKeyAsync()
        {
            try
            {
                // Use default location for testing
                await GetCurrentWeatherAsync(
                    AppConfig.Constants.DefaultLatitude,
                    AppConfig.Constants.DefaultLongitude);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch
            {
                // Other errors don't necessarily mean the API key is bad
                return true;
            }
        }

        private static WeatherData ParseCurrentWeatherData(JsonDocument json, double latitude, double longitude)
        {
            var root = json.RootElement;
            var location = root.GetProperty("location");
            var current = root.GetProperty("current");
            var condition = current.GetProperty("condition");

            var weather = new WeatherData
            {
                Timestamp = DateTime.UtcNow,
                Latitude = latitude,
                Longitude = longitude,
                LocationName = location.GetProperty("name").GetString(),

                // Temperature data (always get Fahrenheit from API)
                Temperature = current.GetProperty("temp_f").GetDouble(),
                FeelsLike = current.GetProperty("feelslike_f").GetDouble(),
                TemperatureUnit = "F",

                // Humidity and Pressure
                Humidity = current.GetProperty("humidity").GetDouble(),
                Pressure = current.GetProperty("pressure_in").GetDouble() * 33.8639, // Convert inHg to hPa

                // Wind data
                WindSpeed = current.GetProperty("wind_mph").GetDouble(),
                WindDirection = current.GetProperty("wind_degree").GetDouble(),
                WindDirectionCardinal = current.GetProperty("wind_dir").GetString() ?? "",
                WindGust = current.GetProperty("gust_mph").GetDouble(),
                WindSpeedUnit = "mph",

                // Visibility and conditions
                Visibility = current.GetProperty("vis_miles").GetDouble(),
                CloudCover = current.GetProperty("cloud").GetDouble(),
                WeatherCondition = condition.GetProperty("text").GetString() ?? "",
                WeatherDescription = condition.GetProperty("text").GetString() ?? "",

                // UV Index
                UvIndex = current.GetProperty("uv").GetDouble(),

                // Precipitation
                RainfallAmount = current.GetProperty("precip_in").GetDouble()
            };

            // Add air quality if available
            if (root.TryGetProperty("current", out var currentElement) &&
                currentElement.TryGetProperty("air_quality", out var airQuality))
            {
                if (airQuality.TryGetProperty("us-epa-index", out var epaIndex))
                {
                    weather.AirQualityIndex = epaIndex.GetDouble();
                    weather.AirQualityDescription = GetAirQualityDescription(epaIndex.GetInt32());
                }
            }

            return weather;
        }

        private static WeatherData ParseForecastDay(JsonElement forecastDay, double latitude, double longitude)
        {
            var day = forecastDay.GetProperty("day");
            var astro = forecastDay.GetProperty("astro");
            var condition = day.GetProperty("condition");
            var date = DateTime.Parse(forecastDay.GetProperty("date").GetString() ?? DateTime.Now.ToString("yyyy-MM-dd"));

            var weather = new WeatherData
            {
                Timestamp = date,
                Latitude = latitude,
                Longitude = longitude,

                // Temperature data
                Temperature = day.GetProperty("avgtemp_f").GetDouble(),
                TemperatureMin = day.GetProperty("mintemp_f").GetDouble(),
                TemperatureMax = day.GetProperty("maxtemp_f").GetDouble(),
                TemperatureUnit = "F",

                // Weather conditions
                Humidity = day.GetProperty("avghumidity").GetDouble(),
                WindSpeed = day.GetProperty("maxwind_mph").GetDouble(),
                WindSpeedUnit = "mph",

                Visibility = day.GetProperty("avgvis_miles").GetDouble(),
                WeatherCondition = condition.GetProperty("text").GetString() ?? "",
                WeatherDescription = condition.GetProperty("text").GetString() ?? "",

                // UV and precipitation
                UvIndex = day.GetProperty("uv").GetDouble(),
                PrecipitationChance = day.GetProperty("daily_chance_of_rain").GetDouble(),
                RainfallAmount = day.GetProperty("totalprecip_in").GetDouble(),

                // Astronomy data
                Sunrise = ParseTimeString(astro.GetProperty("sunrise").GetString()),
                Sunset = ParseTimeString(astro.GetProperty("sunset").GetString()),
                Moonrise = ParseTimeString(astro.GetProperty("moonrise").GetString()),
                Moonset = ParseTimeString(astro.GetProperty("moonset").GetString()),
                MoonPhase = astro.GetProperty("moon_phase").GetString() ?? "",
                MoonIllumination = double.Parse(astro.GetProperty("moon_illumination").GetString()?.Replace("%", "") ?? "0")
            };

            return weather;
        }

        private static void AddAstronomyData(WeatherData weather, JsonDocument astronomyJson)
        {
            try
            {
                var astro = astronomyJson.RootElement.GetProperty("astronomy").GetProperty("astro");

                weather.Sunrise = ParseTimeString(astro.GetProperty("sunrise").GetString());
                weather.Sunset = ParseTimeString(astro.GetProperty("sunset").GetString());
                weather.Moonrise = ParseTimeString(astro.GetProperty("moonrise").GetString());
                weather.Moonset = ParseTimeString(astro.GetProperty("moonset").GetString());
                weather.MoonPhase = astro.GetProperty("moon_phase").GetString() ?? "";

                var moonIllumination = astro.GetProperty("moon_illumination").GetString()?.Replace("%", "") ?? "0";
                weather.MoonIllumination = double.Parse(moonIllumination);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing astronomy data: {ex.Message}");
            }
        }

        private static DateTime ParseTimeString(string? timeString)
        {
            if (string.IsNullOrEmpty(timeString)) return DateTime.MinValue;

            try
            {
                var today = DateTime.Today;
                if (DateTime.TryParse($"{today:yyyy-MM-dd} {timeString}", out var result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static string GetAirQualityDescription(int index)
        {
            return index switch
            {
                1 => "Good",
                2 => "Moderate",
                3 => "Unhealthy for Sensitive Groups",
                4 => "Unhealthy",
                5 => "Very Unhealthy",
                6 => "Hazardous",
                _ => "Unknown"
            };
        }
    }
}