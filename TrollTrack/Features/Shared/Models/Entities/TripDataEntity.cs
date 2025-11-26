namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("Trips")]

    public class TripDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Indexed]
        public string TripName { get; set; } = string.Empty;

        [Indexed]
        public DateTime TripDate { get; set; } = DateTime.Now.Date;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        // Weather snapshot at trip start
        [ForeignKey(typeof(WeatherDataEntity))]
        public Guid? WeatherEntityId { get; set; }

        public int WaterTemperature { get; set; } = 70;

        [Ignore]
        public WeatherDataEntity? WeatherEntity { get; set; }

        public int SecchiDepth { get; set; } = 0; // feet

        public string Clarity { get; set; } = string.Empty;

        // OneToMany relationship - Trip owns the rods
        //[OneToMany(CascadeOperations = CascadeOperation.All)]
        //[Ignore, OneToMany]
        //public List<RodEntity>? Rods { get; set; }


        // OneToMany relationship - Trip owns the catches
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity>? Catches { get; set; }

        // Computed properties
        [Ignore]
        public int CatchCount => Catches?.Count ?? 0;

        [Ignore]
        public TimeSpan? Duration
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue)
                    return EndTime.Value - StartTime.Value;
                if (StartTime.HasValue && IsActive)
                    return DateTime.Now - StartTime.Value;
                return null;
            }
        }

        public bool IsActive { get; set; }

        public string Notes { get; set; } = string.Empty;

    }
}
