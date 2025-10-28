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

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        // Foreign key to Trip - NO ManyToOne navigation property
        [ForeignKey(typeof(TripDataEntity))]
        public Guid TripId { get; set; }

        [ForeignKey(typeof(ProgramDataEntity))]
        public Guid ProgramDataId { get; set; }

        [ForeignKey(typeof(FishInfoEntity))]
        public Guid FishInfoId { get; set; }

        [Ignore]
        public string FishName
        {
            get
            {
                return FishData.GetFishNameById(FishInfoId);
            }
        }
    }
}