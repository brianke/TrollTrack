using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TrollTrack.Converters;

namespace TrollTrack.MVVM.Models
{
    public class LureData
    {
        [Key]
        public Guid Id { get; set; }

        public String Manufacturer { get; set; }

        public String Color { get; set; }

        public String Buoyancy { get; set; }

        [JsonConverter(typeof(StringToDoubleConverter))] 
        public Double Weight { get; set; }

        [JsonConverter(typeof(StringToDoubleConverter))] 
        public Double Length { get; set; }

        public List<string> ImagePaths { get; set; } = new List<string>();

        //public string PrimaryImagePath => ImagePaths?.FirstOrDefault();
        public string PrimaryImagePath
        {
            get
            {
                var path = ImagePaths?.FirstOrDefault();
                System.Diagnostics.Debug.WriteLine($"PrimaryImagePath: {path}");
                return path;
            }
        }
    }
}
