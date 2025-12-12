using System.Xml.Linq;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("Lures")]
    public class LureDataEntity
    {

        /// <summary>
        /// Override the default GetHashCode() so SelectedItem can find the matching item
        /// </summary>
        /// <returns>hashcode of <see cref="LureDataEntity"/></returns>
        public override int GetHashCode()
        {
            return HelperClass.CalculateHashCode(this,
                    () => this.Id,
                    () => this.Manufacturer,
                    () => this.Length,
                    () => this.Color ?? string.Empty,
                    () => this.Buoyancy ?? string.Empty,
                    () => this.Weight);
        }

        /// <summary>
        /// Override Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            var item = obj as LureDataEntity;

            if (item == null)
            {
                return false;
            }

            // ?? String.Empty will return empty string if LureDataEntity is null (avoids a object is null error)
            return (this.Id).Equals(item.Id)
                    && this.Manufacturer.Equals(item.Manufacturer)
                    && this.Length.Equals(item.Length)
                    && (this.Color ?? string.Empty).Equals(item.Color ?? string.Empty);
        }

        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public String Manufacturer { get; set; } = string.Empty;

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

        // Display name for picker
        [Ignore]
        public string DisplayName => $"{Manufacturer} - {Color}";

    }
}