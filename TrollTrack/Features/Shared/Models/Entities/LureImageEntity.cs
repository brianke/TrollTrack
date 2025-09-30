using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Shared.Models.Entities
{
    [Table("LureImages")]
    public class LureImageEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [Required]
        public string ImagePath { get; set; }

        [ForeignKey(typeof(LureDataEntity))]
        public Guid LureId { get; set; }

        [ManyToOne]
        public LureDataEntity Lure { get; set; }
    }
}