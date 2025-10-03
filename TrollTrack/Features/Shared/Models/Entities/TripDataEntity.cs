using SQLiteNetExtensions.Attributes;
using System.ComponentModel.DataAnnotations;
using TrollTrack.Models.Entities;

namespace TrollTrack.Features.Shared.Models.Entities
{
    public class TripDataEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string TripName { get; set; } = string.Empty;

        [Required]
        public DateTime TripDate { get; set; } = DateTime.Now.Date;

        public List<ProgramDataEntity>? ProgramDataEntities { get; set; }
    }
}
