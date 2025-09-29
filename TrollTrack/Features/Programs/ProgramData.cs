using System.ComponentModel.DataAnnotations;

namespace TrollTrack.Features.Programs
{
    public class ProgramData
    {
        [Key]
        public Guid Id { get; set; }
    }
}
