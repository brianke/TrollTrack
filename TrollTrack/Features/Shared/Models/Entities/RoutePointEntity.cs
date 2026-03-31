namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("RoutePoints")]
    public class RoutePointEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public Guid TripId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Course { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
