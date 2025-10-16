using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("ProgramData")]
    public class ProgramDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity>? Catches { get; set; }
    }
}