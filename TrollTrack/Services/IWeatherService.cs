using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Services
{
    public interface IWeatherService
    {
        Task<WeatherDataEntity?> GetCurrentWeatherAsync(double currentLatitude, double currentLongitude);

        Task<List<WeatherDataEntity>> GetWeatherForecastAsync(double latitude, double longitude, int days = 3);

        Task<WeatherDataEntity?> GetWeatherByCityAsync(string cityName);

        //event EventHandler<WeatherData> WeatherUpdated;
    }
}
