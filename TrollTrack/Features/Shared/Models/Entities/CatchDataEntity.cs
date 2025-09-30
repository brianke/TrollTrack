using SQLiteNetExtensions.Attributes;
using TrollTrack.Models.Entities;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("CatchData")]
    public class CatchDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [Indexed]
        public DateTime Timestamp { get; set; }

        [ForeignKey(typeof(LocationDataEntity))]
        public Guid? LocationId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public LocationDataEntity Location { get; set; }

        [ForeignKey(typeof(ProgramDataEntity))]
        public Guid? ProgramDataId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public ProgramDataEntity ProgramData { get; set; }

        [ForeignKey(typeof(FishInfoEntity))]
        public Guid? FishInfoId { get; set; }
        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public FishInfoEntity FishInfo { get; set; }
    }
}