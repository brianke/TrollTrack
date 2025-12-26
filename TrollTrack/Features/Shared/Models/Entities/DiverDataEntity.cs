using System.Text.Json.Serialization;

namespace TrollTrack.Features.Shared.Models.Entities
{
    /// <summary>
    /// Represents all diver types
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiverType
    {
        [Description("Not Set")]
        NA = 0,

        [Description("Dipsy Diver")]
        Dipsy = 1,

        [Description("Jet Diver")]
        Jet = 2,

        [Description("Inline Weight")]
        InlineWeight = 3,


    }


    [Table("Divers")]
    public class DiverDataEntity
    {
        /// <summary>
        /// Override the default GetHashCode() so SelectedItem can find the matching item
        /// </summary>
        /// <returns>hashcode of <see cref="DiverDataEntity"/></returns>
        public override int GetHashCode()
        {
            return HelperClass.CalculateHashCode(this,
                    () => this.Id);

                    //() => this.Manufacturer,
                    //() => this.DiverType,
                    //() => this.Name,
                    //() => this.Size,
                    //() => this.Color,
                    //() => this.Setting); 
        }

        /// <summary>
        /// Override Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object? obj)
        {
            var item = obj as DiverDataEntity;

            if (item == null)
            {
                return false;
            }

            return (this.Id).Equals(item.Id);

                    //&& this.Manufacturer.Equals(item.Manufacturer)
                    //&& this.DiverType.Equals(item.DiverType)
                    //&& this.Name.Equals(item.Name)
                    //&& this.Size.Equals(item.Size)
                    //&& this.Color.Equals(item.Color)
                    //&& this.Setting.Equals(item.Setting);
        }

        [PrimaryKey]
        public Guid Id { get; set; } = new Guid();

        [Required]
        public String Manufacturer { get; set; } = string.Empty;

        [Required]
        public DiverType DiverType { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public string Setting { get; set; } = string.Empty;

        // Display name for picker
        [Ignore]
        public string DisplayName => 
            string.IsNullOrWhiteSpace(Setting)
                ? Name
                : $"{Name} - {Setting} Setting";

    }
}
