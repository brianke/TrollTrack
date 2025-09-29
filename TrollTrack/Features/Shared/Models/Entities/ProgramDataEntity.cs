using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Models.Entities
{
    [Table("ProgramData")]
    public class ProgramDataEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<CatchDataEntity> Catches { get; set; }
    }
}