using System.Text.Json;
using System.Text.Json.Serialization;
using TrollTrack.MVVM.Models;
using TrollTrack.Configuration;

namespace TrollTrack.Services
{
    /// <summary>
    /// Service for fetching weather data from WeatherAPI.com
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(AppConfig.Constants.WeatherApiTimeoutSeconds);
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Gets current weather data for specified coordinates
        /// </summary>
        public async Task<WeatherData> GetCurrentWeatherAsync(double latitude, double longitude)
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
                var currentWeatherUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/current.json?key={apiKey}&q={latitude},{longitude}&aqi=yes";
                var currentResponse = await _httpClient.GetStringAsync(currentWeatherUrl);
                var weatherApiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(currentResponse, _jsonSerializerOptions);

                if (weatherApiResponse == null)
                {
                    throw new Exception("Failed to deserialize weather data.");
                }

                var weather = ParseCurrentWeatherData(weatherApiResponse, latitude, longitude);

                // Try to get astronomy data (optional)
                try
                {
                    var astronomyUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/astronomy.json?key={apiKey}&q={latitude},{longitude}&dt={DateTime.Now:yyyy-MM-dd}";
                    var astronomyResponse = await _httpClient.GetStringAsync(astronomyUrl);
                    var astronomyApiResponse = JsonSerializer.Deserialize<AstronomyApiResponse>(astronomyResponse, _jsonSerializerOptions);
                    if (astronomyApiResponse != null)
                    {
                        AddAstronomyData(weather, astronomyApiResponse);
                    }
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
            if (days > AppConfig.Constants.MaxForecastDays) days = AppConfig.Constants.MaxForecastDays;
            if (days < 1) days = 1;

            try
            {
                var forecastUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/forecast.json?key={apiKey}&q={latitude},{longitude}&days={days}&aqi=no&alerts=no";
                var response = await _httpClient.GetStringAsync(forecastUrl);
                var forecastApiResponse = JsonSerializer.Deserialize<ForecastApiResponse>(response, _jsonSerializerOptions);

                var forecasts = new List<WeatherData>();
                if (forecastApiResponse?.Forecast?.ForecastDay != null)
                {
                    foreach (var day in forecastApiResponse.Forecast.ForecastDay)
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
                var currentWeatherUrl = $"{AppConfig.Constants.WeatherApiBaseUrl}/current.json?key={apiKey}&q={Uri.EscapeDataString(cityName)}&aqi=yes";
                var response = await _httpClient.GetStringAsync(currentWeatherUrl);
                var weatherApiResponse = JsonSerializer.Deserialize<WeatherApiResponse>(response, _jsonSerializerOptions);

                if (weatherApiResponse?.Location == null) return null;

                var weather = ParseCurrentWeatherData(weatherApiResponse, weatherApiResponse.Location.Lat, weatherApiResponse.Location.Lon);
                weather.LocationName = weatherApiResponse.Location.Name;
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

        private static WeatherData ParseCurrentWeatherData(WeatherApiResponse apiResponse, double latitude, double longitude)
        {
            var current = apiResponse.Current;
            var location = apiResponse.Location;
            var condition = current.Condition;

            var weather = new WeatherData
            {
                Timestamp = DateTime.UtcNow,
                Latitude = latitude,
                Longitude = longitude,
                LocationName = location.Name,
                Temperature = current.TempF,
                FeelsLike = current.FeelsLikeF,
                TemperatureUnit = AppConfig.Constants.ImperialTemperatureUnit,
                Humidity = current.Humidity,
                Pressure = current.PressureIn * AppConfig.Constants.InHgToHpaConversionFactor,
                WindSpeed = current.WindMph,
                WindDirection = current.WindDegree,
                WindDirectionCardinal = current.WindDir ?? "",
                WindGust = current.GustMph,
                WindSpeedUnit = AppConfig.Constants.ImperialSpeedUnit,
                Visibility = current.VisMiles,
                CloudCover = current.Cloud,
                WeatherCondition = condition.Text ?? "",
                WeatherDescription = condition.Text ?? "",
                UvIndex = current.Uv,
                RainfallAmount = current.PrecipIn
            };

            if (current.AirQuality != null)
            {
                weather.AirQualityIndex = current.AirQuality.UsEpaIndex;
                weather.AirQualityDescription = GetAirQualityDescription(current.AirQuality.UsEpaIndex);
            }

            return weather;
        }

        private static WeatherData ParseForecastDay(ForecastDay forecastDay, double latitude, double longitude)
        {
            var day = forecastDay.Day;
            var astro = forecastDay.Astro;
            var condition = day.Condition;

            var weather = new WeatherData
            {
                Timestamp = DateTime.Parse(forecastDay.Date ?? DateTime.Now.ToString("yyyy-MM-dd")),
                Latitude = latitude,
                Longitude = longitude,
                Temperature = day.AvgTempF,
                TemperatureMin = day.MinTempF,
                TemperatureMax = day.MaxTempF,
                TemperatureUnit = AppConfig.Constants.ImperialTemperatureUnit,
                Humidity = day.AvgHumidity,
                WindSpeed = day.MaxWindMph,
                WindSpeedUnit = AppConfig.Constants.ImperialSpeedUnit,
                Visibility = day.AvgVisMiles,
                WeatherCondition = condition.Text ?? "",
                WeatherDescription = condition.Text ?? "",
                UvIndex = day.Uv,
                PrecipitationChance = day.DailyChanceOfRain,
                RainfallAmount = day.TotalPrecipIn,
                Sunrise = ParseTimeString(astro.Sunrise),
                Sunset = ParseTimeString(astro.Sunset),
                Moonrise = ParseTimeString(astro.Moonrise),
                Moonset = ParseTimeString(astro.Moonset),
                MoonPhase = astro.MoonPhase ?? "",
                MoonIllumination = double.Parse(astro.MoonIllumination?.Replace("%", "") ?? "0")
            };

            return weather;
        }

        private static void AddAstronomyData(WeatherData weather, AstronomyApiResponse astronomyApiResponse)
        {
            try
            {
                var astro = astronomyApiResponse.Astronomy.Astro;
                weather.Sunrise = ParseTimeString(astro.Sunrise);
                weather.Sunset = ParseTimeString(astro.Sunset);
                weather.Moonrise = ParseTimeString(astro.Moonrise);
                weather.Moonset = ParseTimeString(astro.Moonset);
                weather.MoonPhase = astro.MoonPhase ?? "";
                var moonIllumination = astro.MoonIllumination?.Replace("%", "") ?? "0";
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

    #region Weather API Response Models

    public class WeatherApiResponse
    {
        public LocationInfo Location { get; set; }
        public CurrentWeather Current { get; set; }
    }

    public class ForecastApiResponse
    {
        public LocationInfo Location { get; set; }
        public Forecast Forecast { get; set; }
    }

    public class AstronomyApiResponse
    {
        public Astronomy Astronomy { get; set; }
    }

    public class LocationInfo
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class CurrentWeather
    {
        [JsonPropertyName("temp_f")]
        public double TempF { get; set; }
        [JsonPropertyName("feelslike_f")]
        public double FeelsLikeF { get; set; }
        public Condition Condition { get; set; }
        public double Humidity { get; set; }
        [JsonPropertyName("pressure_in")]
        public double PressureIn { get; set; }
        [JsonPropertyName("wind_mph")]
        public double WindMph { get; set; }
        [JsonPropertyName("wind_degree")]
        public double WindDegree { get; set; }
        [JsonPropertyName("wind_dir")]
        public string WindDir { get; set; }
        [JsonPropertyName("gust_mph")]
        public double GustMph { get; set; }
        [JsonPropertyName("vis_miles")]
        public double VisMiles { get; set; }
        public double Cloud { get; set; }
        public double Uv { get; set; }
        [JsonPropertyName("precip_in")]
        public double PrecipIn { get; set; }
        [JsonPropertyName("air_quality")]
        public AirQuality AirQuality { get; set; }
    }

    public class Condition
    {
        public string Text { get; set; }
    }

    public class AirQuality
    {
        [JsonPropertyName("us-epa-index")]
        public int UsEpaIndex { get; set; }
    }

    public class Forecast
    {
        [JsonPropertyName("forecastday")]
        public List<ForecastDay> ForecastDay { get; set; }
    }

    public class ForecastDay
    {
        public string Date { get; set; }
        public Day Day { get; set; }
        public Astro Astro { get; set; }
    }

    public class Day
    {
        [JsonPropertyName("avgtemp_f")]
        public double AvgTempF { get; set; }
        [JsonPropertyName("mintemp_f")]
        public double MinTempF { get; set; }
        [JsonPropertyName("maxtemp_f")]
        public double MaxTempF { get; set; }
        [JsonPropertyName("avghumidity")]
        public double AvgHumidity { get; set; }
        [JsonPropertyName("maxwind_mph")]
        public double MaxWindMph { get; set; }
        [JsonPropertyName("avgvis_miles")]
        public double AvgVisMiles { get; set; }
        public Condition Condition { get; set; }
        public double Uv { get; set; }
        [JsonPropertyName("daily_chance_of_rain")]
        public double DailyChanceOfRain { get; set; }
        [JsonPropertyName("totalprecip_in")]
        public double TotalPrecipIn { get; set; }
    }

    public class Astronomy
    {
        public Astro Astro { get; set; }
    }

    public class Astro
    {
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string Moonrise { get; set; }
        public string Moonset { get; set; }
        [JsonPropertyName("moon_phase")]
        public string MoonPhase { get; set; }
        [JsonPropertyName("moon_illumination")]
        public string MoonIllumination { get; set; }
    }

    #endregion
}