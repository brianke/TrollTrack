using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Shared.Models
{
    public class TripData
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime TripDate { get; set; } = DateTime.Now.Date;


    }
}
