namespace TrollTrack.Features.Shared.Models.Entities
{
    public class RodEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Reference to the lure being used on this rod
        [ForeignKey(typeof(LureDataEntity))]
        public Guid? LureId { get; set; }
    }
}