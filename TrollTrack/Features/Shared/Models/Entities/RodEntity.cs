namespace TrollTrack.Features.Shared.Models.Entities
{
    public class RodEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Reference to the lure being used on this rod
        /// </summary>
        [ForeignKey(typeof(LureDataEntity))]
        public Guid? LureId { get; set; }

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
    }
}
