namespace TrollTrack.Features.Shared.Models.Entities
{

    /// <summary>
    /// Represents all Dipsy Diver size and direction settings
    /// </summary>
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
        Size3_Aggressive = 33
    }


    [Table("Divers")]
    public class DiverDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = new Guid();

        [Required]
        public String Manufacturer { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public DipsySettings? Setting { get; set; }

    }
}
