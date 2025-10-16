using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("LureImages")]
    public class LureImageEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [Required]
        public string Path { get; set; } = string.Empty;
    }
}