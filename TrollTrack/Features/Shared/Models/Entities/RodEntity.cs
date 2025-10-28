using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    public class RodEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        // Foreign key to Trip - NO ManyToOne navigation property
        [ForeignKey(typeof(TripDataEntity))]
        public Guid TripId { get; set; }


        // Weather snapshot at trip start
        [ForeignKey(typeof(ProgramDataEntity))]
        public Guid ProgramDataId { get; set; }

    }
}