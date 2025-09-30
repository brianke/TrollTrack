using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TrollTrack.Converters;

namespace TrollTrack.Features.Lures
{
    public class LureData
    {
        [Key]
        public Guid Id { get; set; }

        public string Manufacturer { get; set; }

        public string Color { get; set; }

        public string Buoyancy { get; set; }

        [JsonConverter(typeof(StringToDoubleConverter))] 
        public double Weight { get; set; }

        [JsonConverter(typeof(StringToDoubleConverter))] 
        public double Length { get; set; }

        public List<string> ImagePaths { get; set; } = new List<string>();

        //public string PrimaryImagePath => ImagePaths?.FirstOrDefault();
        public string PrimaryImagePath
        {
            get
            {
                var path = ImagePaths?.FirstOrDefault();
                Debug.WriteLine($"PrimaryImagePath: {path}");
                return path;
            }
        }
    }
}
