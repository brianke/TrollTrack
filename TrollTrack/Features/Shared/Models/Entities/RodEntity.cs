using SQLiteNetExtensions.Attributes;

namespace TrollTrack.Features.Shared.Models.Entities
{
    public class RodEntity
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string? Name { get; set; }

        // Weather snapshot at trip start
        [ForeignKey(typeof(ProgramDataEntity))]
        public Guid? ProgramEntityId { get; set; }

    }
}