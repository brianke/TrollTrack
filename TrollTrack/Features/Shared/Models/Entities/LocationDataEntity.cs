using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("Locations")]
    public class LocationDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Course { get; set; }
        public double? Speed { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity> Catches { get; set; }
    }
}