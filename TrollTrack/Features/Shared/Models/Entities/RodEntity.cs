using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    public class RodEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        // Foreign key to Trip - NO ManyToOne navigation property
        [ForeignKey(typeof(TripDataEntity))]
        public Guid? TripId { get; set; }


        // Weather snapshot at trip start
        [ForeignKey(typeof(ProgramDataEntity))]
        public Guid? ProgramEntityId { get; set; }

    }
}