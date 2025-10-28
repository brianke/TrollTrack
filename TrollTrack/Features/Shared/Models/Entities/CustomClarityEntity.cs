// CustomClarityEntity.cs
namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("CustomClarities")]
    public class CustomClarityEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Unique]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}