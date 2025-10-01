using SQLiteNetExtensions.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("LureData")]
    public class LureDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [Required]
        public String Manufacturer { get; set; }

        public Double Length { get; set; }

        public String? Color { get; set; }

        public String? Buoyancy { get; set; }

        public Double Weight { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<LureImageEntity>? Images { get; set; }

        public LureImageEntity? PrimaryImage
        {
            get
            {
                if (Images != null && Images.Count > 0)
                {
                    return Images[0];
                }
                return null;
            }
        }
    }
}