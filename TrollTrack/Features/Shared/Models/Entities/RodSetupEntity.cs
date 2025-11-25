namespace TrollTrack.Features.Shared.Models.Entities
{
    public class RodSetupEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// User-friendly name for this rod setup (e.g., "Walleye Special", "Deep Diver", "Rod 1")
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the lure being used on this rod
        /// </summary>
        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureId { get; set; }

        /// <summary>
        /// Refernce to the diver being used on this rod
        /// </summary>
        [ForeignKey(typeof(DiverDataEntity))]                           
        public Guid? DiverId { get; set; }

        /// <summary>
        /// Distance of line from boat to lure in feet
        /// </summary>
        public int LineOut { get; set; } = 0;

        /// <summary>
        /// Whether this rod is currently active/deployed
        /// </summary>
        public bool IsActive { get; set; } = true;

        [Ignore, Required]
        public LureDataEntity Lure { get; set; }

        /// <summary>
        /// The diver details (populated by relationships, not stored directly)
        /// </summary>
        [Ignore]
        public DiverDataEntity? Diver { get; set; }
    }
}
