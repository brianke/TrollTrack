using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace TrollTrack.Features.Shared.Models.Entities
{
    /// <summary>
    /// Represents all Buoyancy types
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LureTypes
    {
        [Description("Not Applicable")]
        NA = 0,

        [Description("Crankbait")]
        Crankbait = 1,

        [Description("Deep Crankbait")]
        DeepCrankbait = 2,

        [Description("Shallow Crankbait")]
        ShallowCrankbait = 3,

        [Description("Jerkbait")]
        Jerkbait = 10,

        [Description("Spoon")]
        Spoon = 20,

        [Description("Walley Rig")]
        WalleyeRig = 30
    }

    /// <summary>
    /// Represents all Buoyancy types
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LureBuoyancys
    {
        [Description("Not Applicable")]
        NA = 0,

        [Description("Floating")]
        Floating = 1,

        [Description("Suspending")]
        Suspending = 2,

        [Description("Sinking")]
        Sinking = 3
    }



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
                    () => this.Id);
            //,
            //        () => this.Manufacturer,
            //        () => this.LureType,
            //        () => this.Description,
            //        () => this.Buoyancy,
            //        () => this.Length,
            //        () => this.Weight);
        }

        /// <summary>
        /// Override Equals method
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(System.Object? obj)
        {
            var item = obj as LureDataEntity;

            if (item == null)
            {
                return false;
            }

            // ?? String.Empty will return empty string if LureDataEntity is null (avoids a object is null error)
            return (this.Id).Equals(item.Id);

                    //&& this.Manufacturer.Equals(item.Manufacturer)                    
                    //&& this.LureType.Equals(item.LureType)
                    //&& this.Description.Equals(item.Description)
                    //&& this.Buoyancy.Equals(item.Buoyancy)
                    //&& this.Length.Equals(item.Length)
                    //&& this.Weight.Equals(item.Weight);
        }

        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public String Manufacturer { get; set; } = string.Empty;

        public LureTypes LureType { get; set; }

        public String Description { get; set; } = string.Empty;

        public LureBuoyancys Buoyancy { get; set; }

        public Double Length { get; set; }

        public Double Weight { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<LureImageEntity>? Images { get; set; }

        public Guid PrimaryImageId { get; set; } = Guid.Empty;

        public LureImageEntity? PrimaryImage
        {
            get
            {
                if (Images == null || Images.Count == 0)
                    return null;

                if (PrimaryImageId != Guid.Empty)
                    return Images.FirstOrDefault(i => i.Id == PrimaryImageId) ?? Images[0];

                return Images[0];
            }
        }

        [JsonIgnore]
        public string FrontColorsJson { get; set; } = "[]";

        [JsonIgnore]
        public string BackColorsJson { get; set; } = "[]";

        // JSON-facing properties (NOT stored as DB columns)
        [Ignore]
        [JsonPropertyName("FrontColors")]
        public List<string> FrontColors
        {
            get => JsonSerializer.Deserialize<List<string>>(FrontColorsJson) ?? new();
            set => FrontColorsJson = JsonSerializer.Serialize(value ?? new());
        }

        [Ignore]
        [JsonPropertyName("BackColors")]
        public List<string> BackColors
        {
            get => JsonSerializer.Deserialize<List<string>>(BackColorsJson) ?? new();
            set => BackColorsJson = JsonSerializer.Serialize(value ?? new());
        }

        // Optional: your enum lists for in-app usage (also not DB columns)
        [Ignore]
        public List<LureColor> FrontColorsList =>
            FrontColors.Select(s => Enum.TryParse<LureColor>(s, true, out var c) ? c : (LureColor?)null)
                       .Where(c => c.HasValue)
                       .Select(c => c!.Value)
                       .ToList();

        [Ignore]
        public List<LureColor> BackColorsList =>
            BackColors.Select(s => Enum.TryParse<LureColor>(s, true, out var c) ? c : (LureColor?)null)
                      .Where(c => c.HasValue)
                      .Select(c => c!.Value)
                      .ToList();

        // Display name for picker
        [Ignore]
        public string DisplayName => $"{Manufacturer} - {Description}";

    }

}