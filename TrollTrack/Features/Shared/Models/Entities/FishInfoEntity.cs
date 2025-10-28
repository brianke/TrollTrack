using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("FishInfo")]
    public class FishInfoEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string CommonName { get; set; }

        [Required]
        public string ScientificName { get; set; }

        [Required]
        public string Habitat { get; set; }

        // Add a parameterless constructor to satisfy the 'new()' constraint  
        public FishInfoEntity()
        {
            CommonName = string.Empty;
            ScientificName = string.Empty;
            Habitat = string.Empty;
        }
    }
}