using Newtonsoft.Json;
using TrollTrack.Configuration;
using TrollTrack.Features.Shared.Models.Entities;
using TrollTrack.Features.Shared.Models.WeatherApi;

namespace TrollTrack.Services;

/// <summary>
/// Service for fetching weather data from WeatherAPI.com, refactored to use modern practices.
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly string _apiKey;

    public WeatherService(HttpClient httpClient, IConfigurationService configurationService)
    {
        _httpClient = httpClient;
        _configurationService = configurationService;
        _apiKey = _configurationService.ApiKey;

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("Weather API key is not configured.");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(AppConfig.Constants.WeatherApiTimeoutSeconds);
    }

    private async Task<T> FetchAndDeserializeAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Weather API HTTP error: {ex.Message}");
            if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
            {
                throw new UnauthorizedAccessException("Invalid weather API key. Please check your configuration.");
            }
            if (ex.Message.Contains("403") || ex.Message.Contains("Forbidden"))
            {
                throw new UnauthorizedAccessException("Weather API access denied. The key may have exceeded its quota.");
            }
            throw new Exception("Unable to fetch weather data. Check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Weather API JSON error: {ex.Message}");
            throw new Exception("Received invalid weather data from the API.", ex);
        }
    }

    public async Task<WeatherDataEntity?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        var url = $"{AppConfig.Constants.WeatherApiBaseUrl}/current.json?key={_apiKey}&q={latitude},{longitude}&aqi=yes";
        var apiResponse = await FetchAndDeserializeAsync<WeatherApiResponse>(url);

        return MapToWeatherDataEntity(apiResponse.Current, apiResponse.Location);
    }

    public async Task<List<WeatherDataEntity>> GetWeatherForecastAsync(double latitude, double longitude, int days = 3)
    {
        days = Math.Clamp(days, 1, 3); // Free tier supports up to 3 days
        var url = $"{AppConfig.Constants.WeatherApiBaseUrl}/forecast.json?key={_apiKey}&q={latitude},{longitude}&days={days}&aqi=no&alerts=no";
        var apiResponse = await FetchAndDeserializeAsync<WeatherApiResponse>(url);

        var forecasts = new List<WeatherDataEntity>();
        if (apiResponse?.Forecast?.ForecastDay != null)
        {
            foreach (var day in apiResponse.Forecast.ForecastDay)
            {
                forecasts.Add(MapToWeatherDataEntity(day, apiResponse.Location));
            }
        }
        return forecasts;
    }

    public async Task<WeatherDataEntity?> GetWeatherByCityAsync(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            throw new ArgumentException("City name cannot be empty.", nameof(cityName));
        }

        var url = $"{AppConfig.Constants.WeatherApiBaseUrl}/current.json?key={_apiKey}&q={Uri.EscapeDataString(cityName)}&aqi=yes";

        try
        {
            var apiResponse = await FetchAndDeserializeAsync<WeatherApiResponse>(url);
            return MapToWeatherDataEntity(apiResponse.Current, apiResponse.Location);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("400"))
        {
            throw new ArgumentException($"Location '{cityName}' not found. Please check the spelling.", ex);
        }
    }

    private static WeatherDataEntity MapToWeatherDataEntity(Current current, Location location)
    {
        return new WeatherDataEntity
        {
            Timestamp = DateTime.UtcNow,
            Latitude = location.Lat,
            Longitude = location.Lon,
            LocationName = location.Name,
            Temperature = current.TempF,
            FeelsLike = current.FeelslikeF,
            TemperatureUnit = "F",
            Humidity = current.Humidity,
            Pressure = current.PressureIn * 33.8639, // Convert inHg to hPa
            WindSpeed = current.WindMph,
            WindDirection = current.WindDegree,
            WindDirectionCardinal = current.WindDir,
            WindGust = current.GustMph,
            WindSpeedUnit = "mph",
            Visibility = current.VisMiles,
            CloudCover = current.Cloud,
            WeatherCondition = current.Condition?.Text ?? string.Empty,
            WeatherDescription = current.Condition?.Text ?? string.Empty,
            UvIndex = current.Uv,
            RainfallAmount = current.PrecipIn,
            AirQualityIndex = current.AirQuality?.UsEpaIndex,
            AirQualityDescription = GetAirQualityDescription(current.AirQuality?.UsEpaIndex)
        };
    }

    private static WeatherDataEntity MapToWeatherDataEntity(ForecastDay forecastDay, Location location)
    {
        return new WeatherDataEntity
        {
            Timestamp = DateTime.Parse(forecastDay.Date),
            Latitude = location.Lat,
            Longitude = location.Lon,
            LocationName = location.Name,
            Temperature = forecastDay.Day.AvgtempF,
            TemperatureMin = forecastDay.Day.MintempF,
            TemperatureMax = forecastDay.Day.MaxtempF,
            TemperatureUnit = "F",
            Humidity = forecastDay.Day.Avghumidity,
            WindSpeed = forecastDay.Day.MaxwindMph,
            WindSpeedUnit = "mph",
            Visibility = forecastDay.Day.AvgvisMiles,
            WeatherCondition = forecastDay.Day.Condition?.Text ?? string.Empty,
            WeatherDescription = forecastDay.Day.Condition?.Text ?? string.Empty,
            UvIndex = forecastDay.Day.Uv,
            PrecipitationChance = forecastDay.Day.DailyChanceOfRain,
            RainfallAmount = forecastDay.Day.TotalprecipIn,
            Sunrise = ParseTimeString(forecastDay.Astro?.Sunrise),
            Sunset = ParseTimeString(forecastDay.Astro?.Sunset),
            Moonrise = ParseTimeString(forecastDay.Astro?.Moonrise),
            Moonset = ParseTimeString(forecastDay.Astro?.Moonset),
            MoonPhase = forecastDay.Astro?.MoonPhase,
            MoonIllumination = double.TryParse(forecastDay.Astro?.MoonIllumination?.Replace("%", ""), out var illumination) ? illumination : 0
        };
    }

    private static DateTime ParseTimeString(string? timeString)
    {
        if (string.IsNullOrEmpty(timeString)) return DateTime.MinValue;
        if (DateTime.TryParse(timeString, out var result))
        {
            return result;
        }
        return DateTime.MinValue;
    }

    private static string GetAirQualityDescription(int? index)
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