using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("CatchData")]
    public class CatchDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public DateTime Timestamp { get; set; }

        // Foreign key to Trip - NO ManyToOne navigation property
        [ForeignKey(typeof(TripDataEntity))]
        public Guid TripId { get; set; }

        [ForeignKey(typeof(LocationDataEntity))]
        public Guid LocationId { get; set; }

        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureId { get; set; }

        [ForeignKey(typeof(DiverDataEntity))]
        public DiverDataEntity? DiverType { get; set; } // Dipsy, Jet, Weight, etc.

        public int LineOut { get; set; }

        [ForeignKey(typeof(FishInfoEntity))]
        public Guid FishInfoId { get; set; }

        [Ignore]
        public double? Latitude { get; set; }

        [Ignore]
        public double? Longitude { get; set; }

        [Ignore]
        public string FishName { get; set; }
    }
}