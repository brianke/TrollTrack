using System.ComponentModel.DataAnnotations;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Shared.Models
{
    /// <summary>
    /// Weather data model for fishing conditions
    /// </summary>
    public class WeatherData
    {
        public WeatherDataEntity WeatherEntity { get; set; }

        // Derived properties for fishing
        public bool IsFishingWeatherGood => CalculateFishingConditions();
        public string FishingForecast => GetFishingForecast();
        public double BarometricTrend => WeatherEntity.PressureTrend;

        /// <summary>
        /// Calculates whether conditions are good for fishing
        /// </summary>
        private bool CalculateFishingConditions()
        {
            // Basic fishing weather logic
            bool goodWind = WeatherEntity.WindSpeed <= 15; // Less than 15 mph wind
            bool goodVisibility = WeatherEntity.Visibility >= 1; // At least 1 mile visibility
            bool noStorms = !WeatherEntity.WeatherCondition.ToLower().Contains("storm") &&
                           !WeatherEntity.WeatherCondition.ToLower().Contains("thunder");
            bool reasonableTemp = WeatherEntity.Temperature >= 32 && WeatherEntity.Temperature <= 100; // Above freezing, below 100F
            bool lowPrecip = WeatherEntity.PrecipitationChance <= 70; // Less than 70% chance of rain

            return goodWind && goodVisibility && noStorms && reasonableTemp && lowPrecip;
        }

        /// <summary>
        /// Gets a fishing forecast description
        /// </summary>
        private string GetFishingForecast()
        {
            if (WeatherEntity.PressureTrend > 1)
                return "Rising pressure - Fish may be less active";
            else if (WeatherEntity.PressureTrend < -1)
                return "Falling pressure - Great for fishing!";
            else if (WeatherEntity.WindSpeed < 5)
                return "Calm conditions - Good for surface fishing";
            else if (WeatherEntity.WindSpeed > 20)
                return "Too windy for most fishing";
            else if (WeatherEntity.WeatherCondition.ToLower().Contains("overcast"))
                return "Overcast skies - Excellent fishing conditions";
            else if (WeatherEntity.WeatherCondition.ToLower().Contains("rain") && WeatherEntity.PrecipitationChance < 50)
                return "Light rain possible - Fish may be more active";
            else
                return "Fair fishing conditions";
        }

        /// <summary>
        /// Gets wind direction in cardinal format
        /// </summary>
        public string GetCardinalDirection()
        {
            if (WeatherEntity.WindDirection >= 0 && WeatherEntity.WindDirection < 11.25) return "N";
            if (WeatherEntity.WindDirection >= 11.25 && WeatherEntity.WindDirection < 33.75) return "NNE";
            if (WeatherEntity.WindDirection >= 33.75 && WeatherEntity.WindDirection < 56.25) return "NE";
            if (WeatherEntity.WindDirection >= 56.25 && WeatherEntity.WindDirection < 78.75) return "ENE";
            if (WeatherEntity.WindDirection >= 78.75 && WeatherEntity.WindDirection < 101.25) return "E";
            if (WeatherEntity.WindDirection >= 101.25 && WeatherEntity.WindDirection < 123.75) return "ESE";
            if (WeatherEntity.WindDirection >= 123.75 && WeatherEntity.WindDirection < 146.25) return "SE";
            if (WeatherEntity.WindDirection >= 146.25 && WeatherEntity.WindDirection < 168.75) return "SSE";
            if (WeatherEntity.WindDirection >= 168.75 && WeatherEntity.WindDirection < 191.25) return "S";
            if (WeatherEntity.WindDirection >= 191.25 && WeatherEntity.WindDirection < 213.75) return "SSW";
            if (WeatherEntity.WindDirection >= 213.75 && WeatherEntity.WindDirection < 236.25) return "SW";
            if (WeatherEntity.WindDirection >= 236.25 && WeatherEntity.WindDirection < 258.75) return "WSW";
            if (WeatherEntity.WindDirection >= 258.75 && WeatherEntity.WindDirection < 281.25) return "W";
            if (WeatherEntity.WindDirection >= 281.25 && WeatherEntity.WindDirection < 303.75) return "WNW";
            if (WeatherEntity.WindDirection >= 303.75 && WeatherEntity.WindDirection < 326.25) return "NW";
            if (WeatherEntity.WindDirection >= 326.25 && WeatherEntity.WindDirection < 348.75) return "NNW";
            if (WeatherEntity.WindDirection >= 348.75 && WeatherEntity.WindDirection <= 360) return "N";

            return "N/A";
        }

        /// <summary>
        /// Gets a color indicator for fishing conditions
        /// </summary>
        public string GetFishingConditionColor()
        {
            if (IsFishingWeatherGood) return "Green";
            if (WeatherEntity.WindSpeed > 20 || WeatherEntity.PrecipitationChance > 80) return "Red";
            return "Orange";
        }

        /// <summary>
        /// Gets the Beaufort scale description for wind
        /// </summary>
        public string GetBeaufortScale()
        {
            return WeatherEntity.WindSpeed switch
            {
                < 1 => "Calm",
                >= 1 and < 4 => "Light Air",
                >= 4 and < 7 => "Light Breeze",
                >= 7 and < 11 => "Gentle Breeze",
                >= 11 and < 16 => "Moderate Breeze",
                >= 16 and < 22 => "Fresh Breeze",
                >= 22 and < 28 => "Strong Breeze",
                >= 28 and < 34 => "Near Gale",
                >= 34 and < 41 => "Gale",
                >= 41 and < 48 => "Strong Gale",
                >= 48 and < 56 => "Storm",
                >= 56 and < 64 => "Violent Storm",
                >= 64 => "Hurricane",
                _ => "Unknown"
            };
        }
    }
}