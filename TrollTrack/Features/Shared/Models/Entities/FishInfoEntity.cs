using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("FishInfo")]
    public class FishInfoEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? CommonName { get; set; }
        public string? ScientificName { get; set; }
        public string? Habitat { get; set; }
    }
}