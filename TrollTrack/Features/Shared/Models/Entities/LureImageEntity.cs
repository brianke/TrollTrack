using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Shared.Models.Entities
{
    [Table("LureImages")]
    public class LureImageEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string ImagePath { get; set; }

        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureId { get; set; }

        [ManyToOne]
        public LureDataEntity Lure { get; set; }
    }
}