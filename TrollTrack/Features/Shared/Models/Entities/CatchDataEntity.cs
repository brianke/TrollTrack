using TrollTrack.Features.Shared;

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
        public Guid? DiverDataId { get; set; }  // Foreign key to DiverDataEntity

        public int LineOut { get; set; }

        [ForeignKey(typeof(FishInfoEntity))]
        public Guid FishInfoId { get; set; }

        [Ignore]
        public double? Latitude { get; set; }

        [Ignore]
        public double? Longitude { get; set; }

        /// <summary>
        /// Latitude in degrees, minutes, seconds format (e.g. "47° 36' 37.2\" N")
        /// </summary>
        [Ignore]
        public string FormattedLatitude => Latitude.HasValue
            ? CoordinateFormatter.ToDegreesMinutesSeconds(Latitude.Value, true)
            : "N/A";

        /// <summary>
        /// Longitude in degrees, minutes, seconds format (e.g. "122° 19' 58.1\" W")
        /// </summary>
        [Ignore]
        public string FormattedLongitude => Longitude.HasValue
            ? CoordinateFormatter.ToDegreesMinutesSeconds(Longitude.Value, false)
            : "N/A";

        [Ignore]
        public string FishName { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the lure (populated when loading from database)
        /// </summary>
        [Ignore]
        public string LureDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Path to the lure's primary image (populated when loading from database)
        /// </summary>
        [Ignore]
        public string? LureImagePath { get; set; }

        /// <summary>
        /// Whether the lure has an image to display
        /// </summary>
        [Ignore]
        public bool HasLureImage => !string.IsNullOrWhiteSpace(LureImagePath);

        /// <summary>
        /// Display name for the diver (populated when loading from database)
        /// </summary>
        [Ignore]
        public string DiverDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Speed in knots from location at time of catch
        /// </summary>
        [Ignore]
        public double? Speed { get; set; }

        /// <summary>
        /// Direction/course in degrees (0-360) from location at time of catch
        /// </summary>
        [Ignore]
        public double? Direction { get; set; }

        /// <summary>
        /// Formatted speed for display (e.g. "2.5 kt" or "N/A")
        /// </summary>
        [Ignore]
        public string SpeedDisplay => Speed.HasValue ? $"{Speed.Value:F1} kt" : "N/A";

        /// <summary>
        /// Formatted direction for display (e.g. "270°" or "N/A")
        /// </summary>
        [Ignore]
        public string DirectionDisplay => Direction.HasValue ? $"{Direction.Value:F0}°" : "N/A";

        /// <summary>
        /// Time portion of Timestamp for display (e.g. "2:30 PM")
        /// </summary>
        [Ignore]
        public string FormattedTime => Timestamp.ToString("h:mm tt");

        /// <summary>
        /// Date portion of Timestamp for display (e.g. "2/27/2025")
        /// </summary>
        [Ignore]
        public string FormattedDate => Timestamp.ToString("M/d/yyyy");

        public CatchDataEntity()
        {
            Timestamp = DateTime.Now;
        }

    }
}