using SQLiteNetExtensions.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("FishInfo")]
    public class FishInfoEntity
    {
        public Guid Id { get; set; }
        
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