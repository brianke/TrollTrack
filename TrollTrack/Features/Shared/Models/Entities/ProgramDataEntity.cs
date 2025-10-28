using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("ProgramData")]
    public class ProgramDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity>? Catches { get; set; }
    }
}