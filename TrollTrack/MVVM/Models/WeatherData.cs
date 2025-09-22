using System.ComponentModel.DataAnnotations;

namespace TrollTrack.MVVM.Models
{
    /// <summary>
    /// Weather data model for fishing conditions
    /// </summary>
    public class WeatherData
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Location
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? LocationName { get; set; }

        // Temperature
        public double Temperature { get; set; } = 70;
        public double FeelsLike { get; set; }
        public double TemperatureMin { get; set; }
        public double TemperatureMax { get; set; }
        public string TemperatureUnit { get; set; } = "F"; // F or C

        // Humidity and Pressure
        public double Humidity { get; set; } // Percentage
        public double Pressure { get; set; } // hPa
        public double PressureTrend { get; set; } // Positive = rising, negative = falling

        // Wind
        public double WindSpeed { get; set; } = 0;
        public double WindGust { get; set; }
        public double WindDirection { get; set; } // Degrees 0-360
        public string WindDirectionCardinal { get; set; } = string.Empty; // N, NE, E, etc.
        public string WindSpeedUnit { get; set; } = "mph"; // mph or kph

        // Visibility and Cloud Cover
        public double Visibility { get; set; } // Miles or kilometers
        public double CloudCover { get; set; } // Percentage
        public string WeatherCondition { get; set; } = "Clear"; // Clear, Cloudy, Rain, etc.
        public string WeatherDescription { get; set; } = string.Empty; // Detailed description

        // Precipitation
        public double PrecipitationChance { get; set; } // Percentage
        public double RainfallAmount { get; set; } // Inches or mm in last hour
        public double SnowfallAmount { get; set; } // Inches or cm in last hour

        // Sun and Moon
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public DateTime Moonrise { get; set; }
        public DateTime Moonset { get; set; }
        public string MoonPhase { get; set; } = string.Empty;
        public double MoonIllumination { get; set; } // Percentage

        // Water conditions (if available from marine weather services)
        public double? WaterTemperature { get; set; }
        public double? WaveHeight { get; set; }
        public double? SwellDirection { get; set; }
        public double? SwellPeriod { get; set; }

        // UV Index
        public double UvIndex { get; set; }

        // Air Quality (if available)
        public double? AirQualityIndex { get; set; }
        public string? AirQualityDescription { get; set; }

        // Derived properties for fishing
        public bool IsFishingWeatherGood => CalculateFishingConditions();
        public string FishingForecast => GetFishingForecast();
        public double BarometricTrend => PressureTrend;

        /// <summary>
        /// Calculates whether conditions are good for fishing
        /// </summary>
        private bool CalculateFishingConditions()
        {
            // Basic fishing weather logic
            bool goodWind = WindSpeed <= 15; // Less than 15 mph wind
            bool goodVisibility = Visibility >= 1; // At least 1 mile visibility
            bool noStorms = !WeatherCondition.ToLower().Contains("storm") &&
                           !WeatherCondition.ToLower().Contains("thunder");
            bool reasonableTemp = Temperature >= 32 && Temperature <= 100; // Above freezing, below 100F
            bool lowPrecip = PrecipitationChance <= 70; // Less than 70% chance of rain

            return goodWind && goodVisibility && noStorms && reasonableTemp && lowPrecip;
        }

        /// <summary>
        /// Gets a fishing forecast description
        /// </summary>
        private string GetFishingForecast()
        {
            if (PressureTrend > 1)
                return "Rising pressure - Fish may be less active";
            else if (PressureTrend < -1)
                return "Falling pressure - Great for fishing!";
            else if (WindSpeed < 5)
                return "Calm conditions - Good for surface fishing";
            else if (WindSpeed > 20)
                return "Too windy for most fishing";
            else if (WeatherCondition.ToLower().Contains("overcast"))
                return "Overcast skies - Excellent fishing conditions";
            else if (WeatherCondition.ToLower().Contains("rain") && PrecipitationChance < 50)
                return "Light rain possible - Fish may be more active";
            else
                return "Fair fishing conditions";
        }

        /// <summary>
        /// Gets wind direction in cardinal format
        /// </summary>
        public string GetCardinalDirection()
        {
            if (WindDirection >= 0 && WindDirection < 11.25) return "N";
            if (WindDirection >= 11.25 && WindDirection < 33.75) return "NNE";
            if (WindDirection >= 33.75 && WindDirection < 56.25) return "NE";
            if (WindDirection >= 56.25 && WindDirection < 78.75) return "ENE";
            if (WindDirection >= 78.75 && WindDirection < 101.25) return "E";
            if (WindDirection >= 101.25 && WindDirection < 123.75) return "ESE";
            if (WindDirection >= 123.75 && WindDirection < 146.25) return "SE";
            if (WindDirection >= 146.25 && WindDirection < 168.75) return "SSE";
            if (WindDirection >= 168.75 && WindDirection < 191.25) return "S";
            if (WindDirection >= 191.25 && WindDirection < 213.75) return "SSW";
            if (WindDirection >= 213.75 && WindDirection < 236.25) return "SW";
            if (WindDirection >= 236.25 && WindDirection < 258.75) return "WSW";
            if (WindDirection >= 258.75 && WindDirection < 281.25) return "W";
            if (WindDirection >= 281.25 && WindDirection < 303.75) return "WNW";
            if (WindDirection >= 303.75 && WindDirection < 326.25) return "NW";
            if (WindDirection >= 326.25 && WindDirection < 348.75) return "NNW";
            if (WindDirection >= 348.75 && WindDirection <= 360) return "N";

            return "N/A";
        }

        /// <summary>
        /// Gets a color indicator for fishing conditions
        /// </summary>
        public string GetFishingConditionColor()
        {
            if (IsFishingWeatherGood) return "Green";
            if (WindSpeed > 20 || PrecipitationChance > 80) return "Red";
            return "Orange";
        }

        /// <summary>
        /// Gets the Beaufort scale description for wind
        /// </summary>
        public string GetBeaufortScale()
        {
            return WindSpeed switch
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