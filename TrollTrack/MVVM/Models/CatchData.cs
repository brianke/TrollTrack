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
        public Guid Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Location
        public Location Location { get; set; }

        // Program
        public ProgramData? CatchProgram { get; set; }

        // Fish Caught
        public FishCommonName? FishCaught { get; set; }
    }

}
