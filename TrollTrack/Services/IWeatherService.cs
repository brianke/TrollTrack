using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrollTrack.Features.Shared.Models;

namespace TrollTrack.Services
{
    public interface IWeatherService
    {
        Task<WeatherData?> GetCurrentWeatherAsync(double currentLatitude, double currentLongitude);

        Task<List<WeatherData>> GetWeatherForecastAsync(double latitude, double longitude, int days = 3);

        Task<WeatherData?> GetWeatherByCityAsync(string cityName);

        //event EventHandler<WeatherData> WeatherUpdated;
    }
}
