namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("LureImages")]
    public class LureImageEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Path { get; set; } = string.Empty;

        // Add this foreign key property
        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureDataEntityId { get; set; }

    }
}