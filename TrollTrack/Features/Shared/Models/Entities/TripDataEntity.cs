using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Shared.Models.Entities
{
    public class TripDataEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime TripDate { get; set; } = DateTime.Now.Date;


    }
}
