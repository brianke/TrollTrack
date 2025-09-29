using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Shared.Models.Entities
{
    [Table("LureData")]
    public class LureDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public String Manufacturer { get; set; }

        public Double Length { get; set; }

        public String Color { get; set; }

        public String Buoyancy { get; set; }

        public Double Weight { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<LureImageEntity> Images { get; set; }
    }
}