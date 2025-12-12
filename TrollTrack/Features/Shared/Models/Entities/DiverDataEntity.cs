using System.Text.Json.Serialization;

namespace TrollTrack.Features.Shared.Models.Entities
{
    /*
    /// <summary>
    /// Represents all Dipsy Diver size and direction settings
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DipsySettings
    {
        // Size 0 Settings
        Size0_Straight = 0,
        Size0_Slight = 1,
        Size0_Moderate = 2,
        Size0_Aggressive = 3,

        // Size 1 Settings (most popular)
        Size1_Straight = 10,
        Size1_Slight = 11,
        Size1_Moderate = 12,
        Size1_Aggressive = 13,

        // Size 3 Settings
        Size3_Straight = 30,
        Size3_Slight = 31,
        Size3_Moderate = 32,
        Size3_Aggressive = 33,
    }
    */

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
                    () => this.Id,
                    () => this.Manufacturer,
                    () => this.Name,
                    () => this.Setting); 
        }

        /// <summary>
        /// Override Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object obj)
        {
            var item = obj as DiverDataEntity;

            if (item == null)
            {
                return false;
            }

            // ?? String.Empty will return empty string if TimeFileStructureId is null (avoids a object is null error)
            return (this.Id).Equals(item.Id)
                    && this.Manufacturer.Equals(item.Manufacturer)
                    && this.Name.Equals(item.Name)
                    && this.Setting.Equals(item.Setting);
        }

        [PrimaryKey]
        public Guid Id { get; set; } = new Guid();

        [Required]
        public String Manufacturer { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Setting { get; set; } = string.Empty;

        // Display name for picker
        [Ignore]
        public string DisplayName => 
            string.IsNullOrWhiteSpace(Setting)
                ? Name
                : $"{Name} - {Setting} Setting";

        // Display name for picker
        //[Ignore]
        //public string DisplayName => $"{Name}" +
        //    (DipsySetting.HasValue ? $" - {FormatDipsySetting(DipsySetting.Value)}" : "");

        //private string FormatDipsySetting(DipsySettings setting)
        //{
        //    var settingStr = setting.ToString();
        //    // Convert "Size1_Moderate" to "Size 1 - Moderate"
        //    return settingStr.Replace("_", " - ").Replace("Size", "Size ");
        //}

    }
}
