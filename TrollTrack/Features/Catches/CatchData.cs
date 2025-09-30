using System.ComponentModel.DataAnnotations;
using TrollTrack.Features.Programs;
using TrollTrack.Features.Shared.Models;

namespace TrollTrack.Features.Catches
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
