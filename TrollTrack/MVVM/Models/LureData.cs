using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrollTrack.MVVM.Models
{
    public class LureData
    {
        [Key]
        public Guid Id { get; set; }

        public String Manufacturer { get; set; }

        public String Color { get; set; }

        public String Buoyancy { get; set; }

        public Double Weight { get; set; }

        public Double Length { get; set; }

        public List<string> ImagePaths { get; set; } = new List<string>();

        public string PrimaryImagePath => ImagePaths?.FirstOrDefault();
    }
}
