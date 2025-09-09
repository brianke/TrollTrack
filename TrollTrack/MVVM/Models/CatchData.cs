using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.MVVM.Models
{
    public class CatchData
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Location
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Weather
        public WeatherData? CatchWeather { get; set; }

        // Program
        public ProgramData? CatchProgram { get; set; }

        // Fish Caught
        public FishCommonName? FishCaught { get; set; }
    }

}
