using TrollTrack.Features.Shared.Models.Entities;

namespace TrollTrack.Features.Shared.Models
{
    /// <summary>
    /// Represents a rod configuration during an active fishing trip.
    /// Can be linked to a saved RodSetupEntity for persistence across trips.
    /// </summary>
    public class RodConfiguration
    {
        // Link to saved setup (null if this is a new/unsaved configuration)
        public int? SavedSetupId { get; set; }

        public string Name { get; set; } = string.Empty;
        public Guid? LureId { get; set; }
        public LureDataEntity Lure { get; set; }
        public int LineOut { get; set; }
        public Guid? DiverId { get; set; }
        //public string DiverType { get; set; } = string.Empty;
        //public int? DiverSetting { get; set; }
        //public string? Notes { get; set; }

        // Display helpers
        public string DisplayName => Name;
        public string SetupDescription => GetSetupDescription();
        public bool IsSaved => SavedSetupId.HasValue;

        private string GetSetupDescription()
        {
            var parts = new List<string>();

            //if (!string.IsNullOrEmpty(LureName))
            //    parts.Add(LureName);

            //if (!string.IsNullOrEmpty(DiverType))
            //{
            //    if (DiverSetting.HasValue)
            //        parts.Add($"{DiverType} ({DiverSetting})");
            //    else
            //        parts.Add(DiverType);
            //}

            if (LineOut > 0)
                parts.Add($"{LineOut} ft");

            return string.Join(" - ", parts);
        }

        /// <summary>
        /// Create a copy for catch logging
        /// </summary>
        public RodConfiguration Clone()
        {
            return new RodConfiguration
            {
                SavedSetupId = SavedSetupId,
                Name = Name,
                LureId = LureId,
                LineOut = LineOut,
                DiverId = DiverId
            };
        }

        /// <summary>
        /// Check if configuration has changed from saved setup
        /// </summary>
        public bool HasChanges(RodSetupEntity savedSetup)
        {
            if (savedSetup == null) return true;

            return Name != savedSetup.Name ||
                   Lure != savedSetup.Lure ||
                   LineOut != savedSetup.LineOut ||
                   DiverId != savedSetup.DiverId;
        }
    }
}